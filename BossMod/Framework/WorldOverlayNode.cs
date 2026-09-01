using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace BossMod;

// Minimal, purpose-built native overlay derived from the architecture used by KamiToolKit's
// background OverlayLayer. FFXIV and Dalamud retain the addon's original vtable after creation, so
// the modified table and its managed Initialize/OnSetup delegates intentionally remain rooted for
// the lifetime of the persistent overlay addon, matching KamiToolKit's overlay lifetime model.
internal sealed unsafe class WorldOverlayNode : IDisposable
{
    private const string OverlayAddonName = "BMR_Overlay_Back";
    private const uint ImageNodeId = 100_055_001;
    private const int OverlayObjectListCapacity = 2; // root node + texture image node
    private static readonly TimeSpan CreateRetryDelay = TimeSpan.FromSeconds(2d);

    private AtkUnitBase* _addon;
    private AtkImageNode* _imageNode;
    private AtkUldPartsList* _partsList;
    private AtkUldPart* _part;
    private AtkUldAsset* _asset;
    private Texture* _currentTexture;
    private DateTime _nextCreateAttempt;
    private int _screenWidth;
    private int _screenHeight;
    private float _inverseGlobalScale;
    private bool _disposed;

    public bool IsAttached => !_disposed && _addon != null && _imageNode != null && ((AtkResNode*)_imageNode)->ParentNode == _addon->RootNode;

    public WorldOverlayNode()
    {
        CreateImageNode();

        Service.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "NamePlate", OnNamePlatePreFinalize);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, OverlayAddonName, OnOverlayAddonPreFinalize);
        Service.Framework.Update += OnFrameworkUpdate;
    }

    public Texture* CreateTexture(int width, int height)
    {
        if (!IsAttached || width <= 0 || height <= 0)
        {
            return null;
        }

        var flags = TextureFlags.TextureType2D | TextureFlags.TextureRenderTarget;
        var texture = Texture.CreateTexture2D(width, height, 1, TextureFormat.B8G8R8A8_UNORM, flags, 0u);
        if (texture != null)
        {
            texture->IncRef();
        }
        return texture;
    }

    // Takes ownership of the reference returned by CreateTexture. AtkTexture deliberately remains non-owning; _currentTexture is the single native wrapper reference owned by this class
    public void SetTexture(Texture* texture, int width, int height)
    {
        if (_disposed || _asset == null || _part == null || _imageNode == null)
        {
            if (texture != null)
            {
                texture->DecRef();
            }
            return;
        }

        SetImageSize(width, height);
        _asset->AtkTexture.KernelTexture = texture;
        _asset->AtkTexture.TextureType = texture != null ? TextureType.KernelTexture : 0;

        var previous = _currentTexture;
        _currentTexture = texture;
        if (previous != null)
        {
            previous->DecRef();
        }
    }

    public void ReleaseTexture()
    {
        if (_asset != null)
        {
            _asset->AtkTexture.KernelTexture = null;
            _asset->AtkTexture.TextureType = 0;
        }

        var previous = _currentTexture;
        _currentTexture = null;
        if (previous != null)
        {
            previous->DecRef();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        Service.Framework.Update -= OnFrameworkUpdate;
        Service.AddonLifecycle.UnregisterListener(OnNamePlatePreFinalize, OnOverlayAddonPreFinalize);

        // During process shutdown the game owns teardown order. Avoid touching UI allocations that may already be invalid; ordinary plugin reload/unload still performs the full cleanup
        if (Service.Framework.IsFrameworkUnloading)
        {
            _addon = null;
            _imageNode = null;
            _partsList = null;
            _part = null;
            _asset = null;
            _currentTexture = null;
            return;
        }

        ReleaseTexture();
        DetachImageNode();
        DestroyImageNode();
        _addon = null;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (_disposed)
        {
            return;
        }

        var unitManager = RaptureAtkUnitManager.Instance();
        if (unitManager == null)
        {
            return;
        }

        var namePlate = unitManager->GetAddonByName("NamePlate");
        if (namePlate == null || !namePlate->IsReady)
        {
            return;
        }

        var addon = unitManager->GetAddonByName(OverlayAddonName);
        if (addon == null && DateTime.UtcNow >= _nextCreateAttempt)
        {
            _nextCreateAttempt = DateTime.UtcNow + CreateRetryDelay;
            try
            {
                addon = NativeOverlayAddonFactory.Create(OverlayAddonName);
            }
            catch (Exception ex)
            {
                Service.Logger.Error(ex, "Failed to create the native world-overlay addon");
            }
        }

        if (addon == null || !addon->IsReady)
            return;

        if (_addon != addon)
        {
            DetachImageNode();
            _addon = addon;
            _screenWidth = 0;
            _screenHeight = 0;
            _inverseGlobalScale = 0f;
        }

        UpdateAddonLayout(addon);
        AttachImageNode(addon);

        var uiHidden = (unitManager->Flags & AtkUnitManagerFlags.UiHidden) != 0;
        ((AtkResNode*)_imageNode)->ToggleVisibility(namePlate->IsVisible && !uiHidden);
    }

    private void OnNamePlatePreFinalize(AddonEvent type, AddonArgs args)
    {
        DetachImageNode();
        _addon = null;
    }

    private void OnOverlayAddonPreFinalize(AddonEvent type, AddonArgs args)
    {
        var finalizingAddon = (AtkUnitBase*)args.Addon.Address;
        if (_addon != null && finalizingAddon != _addon)
        {
            return;
        }

        DetachImageNode();
        _addon = null;
    }

    private void UpdateAddonLayout(AtkUnitBase* addon)
    {
        ref var screenSize = ref AtkStage.Instance()->ScreenSize;
        var width = screenSize.Width;
        var height = screenSize.Height;
        var inverseScale = 1.0f / AtkUnitBase.GetGlobalUIScale();
        if (_screenWidth == width && _screenHeight == height && Math.Abs(_inverseGlobalScale - inverseScale) < 0.0001f)
        {
            return;
        }

        addon->SetScale(inverseScale, true);
        addon->SetSize((ushort)width, (ushort)height);
        addon->SetPosition(0, 0);

        if (addon->RootNode != null)
        {
            addon->RootNode->Width = (ushort)width;
            addon->RootNode->Height = (ushort)height;
            MarkNodeDirty(addon->RootNode);
        }

        _screenWidth = width;
        _screenHeight = height;
        _inverseGlobalScale = inverseScale;
    }

    private void CreateImageNode()
    {
        var uiSpace = IMemorySpace.GetUISpace();
        var imageNode = uiSpace->Create<AtkImageNode>();
        var partsList = MallocZeroed<AtkUldPartsList>(uiSpace);
        var part = AllocateZeroedArray<AtkUldPart>(uiSpace, 1);
        var asset = MallocZeroed<AtkUldAsset>(uiSpace);
        if (imageNode == null || partsList == null || part == null || asset == null)
        {
            if (asset != null)
            {
                IMemorySpace.Free(asset);
            }
            if (part != null)
            {
                IMemorySpace.Free(part);
            }
            if (partsList != null)
            {
                IMemorySpace.Free(partsList);
            }
            if (imageNode != null)
            {
                ((AtkResNode*)imageNode)->VirtualTable->Destroy((AtkResNode*)imageNode, true);
            }
            throw new InvalidOperationException("Unable to allocate the native world-overlay image node");
        }

        asset->AtkTexture.Ctor();
        part->UldAsset = asset;
        partsList->Id = 0u;
        partsList->PartCount = 1u;
        partsList->Parts = part;

        imageNode->PartsList = partsList;
        imageNode->PartId = 0;

        var resNode = (AtkResNode*)imageNode;
        resNode->Type = NodeType.Image;
        resNode->NodeId = ImageNodeId;
        resNode->NodeFlags |= NodeFlags.Visible | NodeFlags.Enabled;
        resNode->ToggleVisibility(true);

        _imageNode = imageNode;
        _partsList = partsList;
        _part = part;
        _asset = asset;
    }

    private void SetImageSize(int width, int height)
    {
        if (_imageNode == null || _part == null)
        {
            return;
        }

        var clampedWidth = (ushort)Math.Clamp(width, 0, ushort.MaxValue);
        var clampedHeight = (ushort)Math.Clamp(height, 0, ushort.MaxValue);
        _part->Width = clampedWidth;
        _part->Height = clampedHeight;

        var resNode = (AtkResNode*)_imageNode;
        resNode->Width = clampedWidth;
        resNode->Height = clampedHeight;
        MarkNodeDirty(resNode);
    }

    private void AttachImageNode(AtkUnitBase* addon)
    {
        if (_imageNode == null || addon->RootNode == null)
        {
            return;
        }

        var node = (AtkResNode*)_imageNode;
        if (node->ParentNode == addon->RootNode)
        {
            return;
        }

        if (node->ParentNode != null)
        {
            DetachImageNode();
        }

        // This addon owns a fixed two-entry object list allocated during initialization. Add the image before linking it so an unexpected list-state failure leaves the node detached
        if (!AddOverlayNodeToObjectList(addon, node))
        {
            return;
        }

        var root = addon->RootNode;
        if (root->ChildNode == null)
        {
            root->ChildNode = node;
            node->ParentNode = root;
        }
        else
        {
            var last = root->ChildNode;
            while (last->PrevSiblingNode != null)
            {
                last = last->PrevSiblingNode;
            }

            node->ParentNode = root;
            node->NextSiblingNode = last;
            last->PrevSiblingNode = node;
        }
        root->ChildCount++;

        MarkNodeDirty(node);
        addon->UldManager.UpdateDrawNodeList();
        addon->UpdateCollisionNodeList(false);
    }

    private void DetachImageNode()
    {
        if (_imageNode == null)
        {
            return;
        }

        var node = (AtkResNode*)_imageNode;
        var parent = node->ParentNode;
        if (parent == null)
        {
            return;
        }

        if (parent->ChildNode == node)
        {
            parent->ChildNode = node->PrevSiblingNode != null ? node->PrevSiblingNode : node->NextSiblingNode;
        }
        if (node->PrevSiblingNode != null)
        {
            node->PrevSiblingNode->NextSiblingNode = node->NextSiblingNode;
        }
        if (node->NextSiblingNode != null)
        {
            node->NextSiblingNode->PrevSiblingNode = node->PrevSiblingNode;
        }
        if (parent->GetNodeType() != NodeType.Component && parent->ChildCount > 0)
        {
            parent->ChildCount--;
        }

        node->ParentNode = null;
        node->PrevSiblingNode = null;
        node->NextSiblingNode = null;

        if (_addon != null)
        {
            RemoveOverlayNodeFromObjectList(_addon, node);
            _addon->UldManager.UpdateDrawNodeList();
            _addon->UpdateCollisionNodeList(false);
        }
    }

    private void DestroyImageNode()
    {
        if (_imageNode == null)
        {
            return;
        }

        if (_asset != null)
        {
            _asset->AtkTexture.KernelTexture = null;
            _asset->AtkTexture.TextureType = 0;
        }

        _imageNode->PartsList = null;
        if (_asset != null)
        {
            IMemorySpace.Free(_asset);
        }
        if (_part != null)
        {
            IMemorySpace.Free(_part);
        }
        if (_partsList != null)
        {
            IMemorySpace.Free(_partsList);
        }

        var resNode = (AtkResNode*)_imageNode;
        resNode->VirtualTable->Destroy(resNode, true);

        _imageNode = null;
        _partsList = null;
        _part = null;
        _asset = null;
    }

    // These are small internal helpers KamiToolKit normally contributes
    private static T* MallocZeroed<T>(IMemorySpace* memorySpace) where T : unmanaged
    {
        var result = memorySpace->Malloc<T>();
        if (result != null)
        {
            NativeMemory.Clear(result, (nuint)sizeof(T));
        }
        return result;
    }

    private static T* AllocateZeroedArray<T>(IMemorySpace* memorySpace, int count) where T : unmanaged
    {
        if (count <= 0)
        {
            return null;
        }

        var byteCount = (nuint)sizeof(T) * (nuint)count;
        var result = (T*)memorySpace->Malloc(byteCount, 8ul);
        if (result != null)
        {
            NativeMemory.Clear(result, byteCount);
        }
        return result;
    }

    private static void MarkNodeDirty(AtkResNode* node)
    {
        if (node != null)
        {
            node->DrawFlags |= 1u; // bit 0 is AtkResNode.IsDirty
        }
    }

    private static bool AddOverlayNodeToObjectList(AtkUnitBase* addon, AtkResNode* node)
    {
        if (addon == null || node == null || (addon->UldManager.ResourceFlags & AtkUldManagerResourceFlag.Initialized) == 0)
        {
            return false;
        }

        var objects = addon->UldManager.Objects;
        if (objects == null || objects->NodeList == null)
        {
            return false;
        }

        var count = objects->NodeCount;
        for (var i = 0; i < count; ++i)
        {
            if (objects->NodeList[i] == node)
            {
                return true;
            }
        }

        if (count >= OverlayObjectListCapacity)
        {
            return false;
        }

        objects->NodeList[count] = node;
        objects->NodeCount++;
        return true;
    }

    private static void RemoveOverlayNodeFromObjectList(AtkUnitBase* addon, AtkResNode* node)
    {
        if (addon == null || node == null || (addon->UldManager.ResourceFlags & AtkUldManagerResourceFlag.Initialized) == 0)
        {
            return;
        }

        var objects = addon->UldManager.Objects;
        if (objects == null || objects->NodeList == null)
        {
            return;
        }

        var count = objects->NodeCount;
        for (var i = 0; i < count; ++i)
        {
            if (objects->NodeList[i] != node)
            {
                continue;
            }

            for (var j = i + 1; j < count; ++j)
            {
                objects->NodeList[j - 1] = objects->NodeList[j];
            }
            objects->NodeList[count - 1] = null;
            objects->NodeCount--;
            return;
        }
    }

    private sealed class NativeOverlayAddonFactory
    {
        private const int VirtualTableEntryCount = 200;

        private readonly AtkUnitBase* _addon;
        private readonly AtkUnitBase.AtkUnitBaseVirtualTable* _originalVirtualTable;
        private readonly AtkUnitBase.AtkUnitBaseVirtualTable* _modifiedVirtualTable;
        private readonly AtkUnitBase.Delegates.Initialize _initializeDelegate;
        private readonly AtkUnitBase.Delegates.OnSetup _setupDelegate;
        private readonly AtkUnitBase.Delegates.Dtor _destructorDelegate;
        private GCHandle _lifetimeRoot;
        private bool _lifetimeReleased;
        private bool _initializeSucceeded;

        private NativeOverlayAddonFactory(AtkUnitBase* addon)
        {
            _addon = addon;
            _originalVirtualTable = addon->VirtualTable;
            _modifiedVirtualTable = (AtkUnitBase.AtkUnitBaseVirtualTable*)AllocateZeroedArray<nint>(IMemorySpace.GetUISpace(), VirtualTableEntryCount);
            if (_modifiedVirtualTable == null)
            {
                throw new InvalidOperationException("Unable to allocate a native addon vtable");
            }

            NativeMemory.Copy(_originalVirtualTable, _modifiedVirtualTable, (nuint)(sizeof(nint) * VirtualTableEntryCount));
            _initializeDelegate = Initialize;
            _setupDelegate = Setup;
            _destructorDelegate = Destructor;
            _modifiedVirtualTable->Initialize = (delegate* unmanaged<AtkUnitBase*, void>)Marshal.GetFunctionPointerForDelegate(_initializeDelegate);
            _modifiedVirtualTable->OnSetup = (delegate* unmanaged<AtkUnitBase*, uint, AtkValue*, void>)Marshal.GetFunctionPointerForDelegate(_setupDelegate);
            _modifiedVirtualTable->Dtor = (delegate* unmanaged<AtkUnitBase*, byte, AtkEventListener*>)Marshal.GetFunctionPointerForDelegate(_destructorDelegate);

            _lifetimeRoot = GCHandle.Alloc(this, GCHandleType.Normal);
        }

        public static AtkUnitBase* Create(string name)
        {
            var addon = IMemorySpace.GetUISpace()->Create<AtkUnitBase>();
            if (addon == null)
            {
                return null;
            }

            var factory = new NativeOverlayAddonFactory(addon);
            return factory.CreateInternal(name);
        }

        private AtkUnitBase* CreateInternal(string name)
        {
            AtkUnitBase* initializedAddon = _addon;
            _addon->VirtualTable = _modifiedVirtualTable;
            try
            {
                _addon->NameString = name;
                ConfigureOverlayFlags(_addon);

                using var nameString = new Utf8String(name);
                AtkStage.Instance()->RaptureAtkUnitManager->InitializeAddon(&initializedAddon, nameString.StringPtr, 0u, null);
                if (initializedAddon == null || !_initializeSucceeded)
                {
                    return null;
                }

                initializedAddon->Open(0u); // depth layer 1 is passed to the game as zero-based layer 0
                return initializedAddon;
            }
            finally
            {
                // _lifetimeRoot owns the long-term lifetime; these calls also make the synchronous
                // creation-time dependency explicit to the JIT.
                GC.KeepAlive(_lifetimeRoot);
                GC.KeepAlive(_initializeDelegate);
                GC.KeepAlive(_setupDelegate);
                GC.KeepAlive(_destructorDelegate);
            }
        }

        private AtkEventListener* Destructor(AtkUnitBase* addon, byte flags)
        {
            var result = _originalVirtualTable->Dtor(addon, flags);
            if ((flags & 1) != 0 && !_lifetimeReleased)
            {
                _lifetimeReleased = true;
                IMemorySpace.Free(_modifiedVirtualTable);
                if (_lifetimeRoot.IsAllocated)
                {
                    _lifetimeRoot.Free();
                }
            }
            return result;
        }

        private void Initialize(AtkUnitBase* addon)
        {
            _originalVirtualTable->Initialize(addon);

            var uiSpace = IMemorySpace.GetUISpace();
            var widgetInfo = MallocZeroed<AtkUldWidgetInfo>(uiSpace);
            var rootNode = uiSpace->Create<AtkResNode>();
            var objectNodes = AllocateZeroedArray<nint>(uiSpace, OverlayObjectListCapacity);
            if (widgetInfo == null || rootNode == null || objectNodes == null)
            {
                if (objectNodes != null)
                {
                    IMemorySpace.Free(objectNodes);
                }
                if (widgetInfo != null)
                {
                    IMemorySpace.Free(widgetInfo);
                }
                if (rootNode != null)
                {
                    rootNode->VirtualTable->Destroy(rootNode, true);
                }
                return;
            }

            widgetInfo->Id = 1u;
            widgetInfo->NodeCount = 1;
            widgetInfo->NodeList = (AtkResNode**)objectNodes;
            widgetInfo->NodeList[0] = rootNode;
            widgetInfo->WidgetAlignment = new AtkWidgetAlignment
            {
                AlignmentType = AlignmentType.Center,
                X = 50.0f,
                Y = 50.0f,
            };

            rootNode->Type = NodeType.Res;
            rootNode->NodeId = 1u;
            rootNode->NodeFlags = NodeFlags.Visible | NodeFlags.Enabled | NodeFlags.Fill;
            rootNode->ToggleVisibility(true);
            if (!TryCreateRootTimeline(rootNode))
            {
                IMemorySpace.Free(objectNodes);
                IMemorySpace.Free(widgetInfo);
                rootNode->VirtualTable->Destroy(rootNode, true);
                return;
            }

            addon->UldManager.InitializeResourceRendererManager();
            addon->UldManager.ResourceFlags |= AtkUldManagerResourceFlag.Initialized;
            addon->UldManager.Objects = (AtkUldObjectInfo*)widgetInfo;
            addon->UldManager.ObjectCount = 1;
            addon->UldManager.ResourceFlags |= AtkUldManagerResourceFlag.ArraysAllocated;
            addon->RootNode = rootNode;
            addon->FocusNode = rootNode;
            MarkNodeDirty(rootNode);
            addon->UldManager.UpdateDrawNodeList();
            addon->UldManager.LoadedState = AtkLoadState.Loaded;
            addon->LoadState = AtkUnitBaseLoadState.LoadingUldResource;
            addon->WasLoadUldByNameCalled = true;
            addon->UpdateCollisionNodeList(false);
            _initializeSucceeded = true;
        }

        // AtkUnitBase.Open uses the root timeline's standard show/hide labels. This is the small
        // native-only subset of KamiToolKit's TimelineBuilder output needed by an overlay addon.
        private static bool TryCreateRootTimeline(AtkResNode* rootNode)
        {
            const int labelCount = 9;
            var uiSpace = IMemorySpace.GetUISpace();
            var timeline = MallocZeroed<AtkTimeline>(uiSpace);
            var resource = MallocZeroed<AtkTimelineResource>(uiSpace);
            var labelSet = MallocZeroed<AtkTimelineLabelSet>(uiSpace);
            var keyFrames = AllocateZeroedArray<AtkTimelineKeyFrame>(uiSpace, labelCount);
            if (timeline == null || resource == null || labelSet == null || keyFrames == null)
            {
                if (keyFrames != null)
                {
                    IMemorySpace.Free(keyFrames);
                }
                if (labelSet != null)
                {
                    IMemorySpace.Free(labelSet);
                }
                if (resource != null)
                {
                    IMemorySpace.Free(resource);
                }
                if (timeline != null)
                {
                    IMemorySpace.Free(timeline);
                }
                return false;
            }

            for (var i = 0; i < labelCount; ++i)
            {
                keyFrames[i] = new AtkTimelineKeyFrame
                {
                    SpeedCoefficient1 = 0f,
                    SpeedCoefficient2 = 0f,
                    FrameIdx = (ushort)(i == 0 ? 1 : i * 10),
                    Interpolation = AtkTimelineInterpolation.None,
                    Value = new AtkTimelineKeyValue
                    {
                        Label = new AtkTimelineLabel
                        {
                            LabelId = (ushort)(101 + i),
                            JumpBehavior = AtkTimelineJumpBehavior.PlayOnce,
                            JumpLabelId = 0,
                        },
                    },
                };
            }

            labelSet->StartFrameIdx = 1;
            labelSet->EndFrameIdx = 89;
            labelSet->LabelKeyGroup.Type = AtkTimelineKeyGroupType.Label;
            labelSet->LabelKeyGroup.KeyFrameCount = labelCount;
            labelSet->LabelKeyGroup.KeyFrames = keyFrames;

            resource->Id = 2u;
            resource->AnimationCount = 0;
            resource->LabelSetCount = 1;
            resource->Animations = null;
            resource->LabelSets = labelSet;

            timeline->Resource = resource;
            timeline->LabelResource = null;
            timeline->ActiveAnimation = null;
            timeline->OwnerNode = rootNode;
            timeline->LabelFrameIdxDuration = 88;
            timeline->LabelEndFrameIdx = 89;
            rootNode->Timeline = timeline;
            return true;
        }

        private void Setup(AtkUnitBase* addon, uint valueCount, AtkValue* values)
        {
            ref var screenSize = ref AtkStage.Instance()->ScreenSize;
            addon->SetScale(1.0f / AtkUnitBase.GetGlobalUIScale(), true);
            addon->SetSize((ushort)screenSize.Width, (ushort)screenSize.Height);
            addon->SetPosition(0, 0);
            _originalVirtualTable->OnSetup(addon, valueCount, values);
            addon->UldManager.SetupTextRecursive();
        }

        private static void ConfigureOverlayFlags(AtkUnitBase* addon)
        {
            addon->ShowSoundEffectId = 0;
            addon->DisableAddonConfig = true;
            addon->DisableFocusability = true;
            addon->DisableFocusOnShow = true;
            addon->DisableHideTransition = true;
            addon->DisableShowHideSoundEffects = true;
            addon->IgnoreUIDisplayMode = true;
            addon->Flags1A2 |= 0x02; // disable controller navigation
            addon->Flags1A3 |= 0x40; // click-through
        }
    }
}
