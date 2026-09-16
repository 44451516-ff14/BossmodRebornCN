using Dalamud.Bindings.ImGui;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod;

[ConfigDisplay(Name = "BOSS模块和雷达", Order = 1)]
public sealed class BossModuleConfig : ConfigNode
{
    public bool RadarResize;

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        if (ImGui.Button("窗口居中"))
        {
            Service.BossModWindow?.RecenterWindow();
        }
    }

    // boss module settings
    [PropertyDisplay("模块加载的最低完成度", tooltip: "部分模块带有\"WIP\"（开发中）状态，修改此设置可强制加载这些模块")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    // Modules explicitly disabled from Supported fights. Primary actor OIDs are used as unique module IDs.
    public uint[] DisabledModuleOIDs = [];
    [JsonIgnore]
    internal HashSet<uint>? _disabledModuleOIDs;

    [PropertyDisplay("允许模块自动使用技能", tooltip: "例如：在击退发生前自动使用防击退技能")]
    public bool AllowAutomaticActions = true;

    [PropertyDisplay("[设置调整用]显示测试雷达和提示窗口", tooltip: "无需进入首领战即可配置雷达和提示窗口，便于调试", separator: true, depends: nameof(EnableRadar))]
    public bool ShowDemo = false;

    // radar window settings
    [PropertyDisplay("启用雷达", separator: true)]
    public bool EnableRadar = true;

    [PropertyDisplay("将雷达投影到 3D 世界")]
    public bool ProjectRadarInto3DWorld = false;

    [PropertyDisplay("在 3D 世界显示角色三角标记", tooltip: "显示普通角色三角标记。关闭后机制标记（含击退落点）仍会显示。", depends: nameof(ProjectRadarInto3DWorld))]
    public bool ShowActorTrianglesIn3DWorld = true;

    [PropertyDisplay("在 3D 世界中绘制竞技场轮廓", tooltip: "启用雷达 3D 投影后，可同时绘制竞技场轮廓", depends: nameof(ProjectRadarInto3DWorld))]
    public bool EnableArenaOutlineIn3DWorld = true;

    [PropertyDisplay("允许在 3D 世界绘制文本与图标广告牌", tooltip: "启用雷达 3D 投影后，可绘制文本与图标广告牌", depends: nameof(ProjectRadarInto3DWorld))]
    public bool EnableTextIconBillboards = true;

    [PropertyDisplay("广告牌高度偏移", tooltip: "广告牌相对地面的高度（yalms）。包含视线、文本与图标。", depends: nameof(ProjectRadarInto3DWorld))]
    [PropertySlider(0f, 20f, Speed = 0.1f, Logarithmic = true)]
    public float BillboardHeightOffset = 5f;

    [PropertyDisplay("文本广告牌字号", tooltip: "调整 3D 世界文本广告牌大小", depends: nameof(ProjectRadarInto3DWorld))]
    [PropertySlider(17f, 250f, Speed = 0.5f, Logarithmic = true)]
    public float TextBillboardFontSize = 110f;

    [PropertyDisplay("图标广告牌字号", tooltip: "调整 3D 世界图标广告牌大小", separator: true, depends: nameof(ProjectRadarInto3DWorld))]
    [PropertySlider(17f, 250f, Speed = 0.5f, Logarithmic = true)]
    public float IconBillboardFontSize = 110f;

    [PropertyDisplay("锁定雷达和提示窗口的位置及鼠标交互")]
    public bool Lock = false;

    [PropertyDisplay("透明雷达窗口背景", tooltip: "移除雷达周围的黑色背景（跨显示器时可能失效）")]
    public bool TrishaMode = true;

    [PropertyDisplay("为雷达竞技场添加不透明背景")]
    public bool OpaqueArenaBackground = true;

    [PropertyDisplay("显示雷达标记的轮廓和阴影")]
    public bool ShowOutlinesAndShadows = true;

    [PropertyDisplay("雷达竞技场缩放比例", tooltip: "雷达窗口中竞技场的显示比例")]
    [PropertySlider(0.1f, 10f, Speed = 0.1f, Logarithmic = true)]
    public float ArenaScale = 1f;

    [PropertyDisplay("雷达元素厚度缩放因子", tooltip: "全局缩放雷达元素轮廓厚度")]
    [PropertySlider(0.1f, 10f, Speed = 0.1f, Logarithmic = true)]
    public float ThicknessScale = 1f;

    [PropertyDisplay("根据镜头方向旋转雷达")]
    public bool RotateArena = true;

    [PropertyDisplay("当禁用旋转时镜像翻转180°")]
    public bool FlipArena = false;

    [PropertyDisplay("为雷达旋转预留额外空间", tooltip: "开启旋转功能时，可为雷达两侧预留空间以防止边缘裁剪，适应战斗中的镜头旋转或方向标识需求")]
    [PropertySlider(1f, 2f, Speed = 0.1f, Logarithmic = true)]
    public float SlackForRotations = 1.5f;

    [PropertyDisplay("显示雷达竞技场边框")]
    public bool ShowBorder = true;

    [PropertyDisplay("玩家处于危险区域时改变边框颜色", tooltip: "当可能受到机制攻击时，边框会从白色变为红色")]
    public bool ShowBorderRisk = true;

    [PropertyDisplay("玩家危险时屏幕边缘脉冲", tooltip: "出现玩家警告时，屏幕边缘会以危险边框色（敌人色）脉冲。独立于雷达与 3D 投影设置。")]
    public bool ShowScreenRiskBorder = false;

    [PropertyDisplay("屏幕危险脉冲强度", depends: nameof(ShowScreenRiskBorder))]
    [PropertySlider(0f, 10f, Speed = 0.1f)]
    public float ScreenRiskBorderIntensity = 2.5f;

    [PropertyDisplay("在雷达上显示方位名称")]
    public bool ShowCardinals = false;

    [PropertyDisplay("方位名称字体大小", depends: nameof(ShowCardinals))]
    [PropertySlider(0.1f, 100f, Speed = 1f)]
    public float CardinalsFontSize = 17f;

    [PropertyDisplay("在雷达上显示标记")]
    public bool ShowWaymarks = false;

    [PropertyDisplay("标记字体大小", depends: nameof(ShowWaymarks))]
    [PropertySlider(0.1f, 100f, Speed = 1f)]
    public float WaymarkFontSize = 22f;

    [PropertyDisplay("在雷达上显示标记（'攻击'、'束缚'、'忽略'和形状标记）")]
    public bool ShowSigns = false;

    [PropertyDisplay("始终显示所有存活的队伍成员")]
    public bool ShowIrrelevantPlayers = false;

    [PropertyDisplay("为雷达中无默认颜色的玩家按职责显示颜色")]
    public bool ColorPlayersBasedOnRole = false;

    [PropertyDisplay("始终显示焦点目标队友", separator: true)]
    public bool ShowFocusTargetPlayer = false;

    [PropertyDisplay("角色三角标记缩放比例")]
    [PropertySlider(0.1f, 10f, Speed = 0.1f)]
    public float ActorScale = 1f;

    // hint window settings
    [PropertyDisplay("显示开怪前遭遇提示弹窗", tooltip: "开怪前显示该遭遇的专属提示。可在弹窗中永久隐藏单个遭遇，并在该遭遇配置窗口中重新启用。")]
    public bool ShowPrePullHints = true;

    // Persisted separately from module-specific config so every encounter can support "Never show again". Primary actor OIDs are used as unique module IDs.
    public uint[] SuppressedPrePullHintOIDs = [];

    [JsonIgnore]
    internal HashSet<uint>? _suppressedPrePullHintOIDs;

    [PropertyDisplay("在独立窗口显示文本提示", tooltip: "将提示窗口与雷达分离以便独立布局")]
    public bool HintsInSeparateWindow = false;

    [PropertyDisplay("独立提示窗口透明化")]
    public bool HintsInSeparateWindowTransparent = false;

    [PropertyDisplay("显示机制序列和计时提示")]
    public bool ShowMechanicTimers = true;

    [PropertyDisplay("显示全屏机制提示")]
    public bool ShowGlobalHints = true;

    [PropertyDisplay("显示玩家专属提示和警告", separator: true)]
    public bool ShowPlayerHints = true;

    // misc. settings
    [PropertyDisplay("在游戏中显示移动提示", tooltip: "使用较少，但可以在游戏中显示箭头，指示在某些机制中移动的位置")]
    public bool ShowWorldArrows = false;

    [PropertyDisplay("显示近战范围指示器")]
    public bool ShowMeleeRangeIndicator = false;

    [PropertyDisplay("最大加载距离", tooltip: "最大加载距离（yalms，出于安全会限制到至少 100）。首领距离超过此值时，模块不会加载；若已激活则会卸载。")]
    [PropertySlider(100f, 500f, Speed = 0.1f, Logarithmic = true)]
    public float MaxLoadDistance = 500f;

    public override void Deserialize(JsonElement j, JsonSerializerOptions ser)
    {
        base.Deserialize(j, ser);
        _disabledModuleOIDs = null;
        _suppressedPrePullHintOIDs = null;
    }

    public bool IsModuleEnabled(uint primaryActorOID) => !DisabledModuleOIDSet().Contains(primaryActorOID);

    public bool IncludeInSupportedFightControls(BossModuleRegistry.Info info)
        => info.Maturity != BossModuleInfo.Maturity.Dummy || MinMaturity == BossModuleInfo.Maturity.Dummy;

    public void SetModuleEnabled(uint primaryActorOID, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var disabled = set.Contains(primaryActorOID);
        if (enabled == !disabled)
        {
            return;
        }

        if (enabled)
        {
            set.Remove(primaryActorOID);
        }
        else
        {
            set.Add(primaryActorOID);
        }

        PersistDisabledModuleOIDs(set);
        Modified.Fire();
    }

    public void SetModulesEnabled(List<uint> primaryActorOIDs, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var changed = false;
        var count = primaryActorOIDs.Count;
        for (var i = 0; i < count; ++i)
        {
            changed |= enabled ? set.Remove(primaryActorOIDs[i]) : set.Add(primaryActorOIDs[i]);
        }

        if (changed)
        {
            PersistDisabledModuleOIDs(set);
            Modified.Fire();
        }
    }

    public (bool anyEnabled, bool allEnabled) ModulesEnabledState(List<uint> primaryActorOIDs)
    {
        var set = DisabledModuleOIDSet();
        var anyEnabled = false;
        var allEnabled = true;
        var count = primaryActorOIDs.Count;
        for (var i = 0; i < count; ++i)
        {
            var enabled = !set.Contains(primaryActorOIDs[i]);
            anyEnabled |= enabled;
            allEnabled &= enabled;
        }
        return (anyEnabled, count > 0 && allEnabled);
    }

    public void SetExpansionEnabled(BossModuleInfo.Expansion expansion, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var changed = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Expansion != expansion || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            changed |= enabled ? set.Remove(info.PrimaryActorOID) : set.Add(info.PrimaryActorOID);
        }

        if (changed)
        {
            PersistDisabledModuleOIDs(set);
            Modified.Fire();
        }
    }

    public void SetCategoryEnabled(BossModuleInfo.Category category, bool enabled)
    {
        var set = DisabledModuleOIDSet();
        var changed = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Category != category || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            changed |= enabled ? set.Remove(info.PrimaryActorOID) : set.Add(info.PrimaryActorOID);
        }

        if (changed)
        {
            PersistDisabledModuleOIDs(set);
            Modified.Fire();
        }
    }

    public (bool anyEnabled, bool allEnabled) ExpansionEnabledState(BossModuleInfo.Expansion expansion)
    {
        var set = DisabledModuleOIDSet();
        var anyEnabled = false;
        var allEnabled = true;
        var any = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Expansion != expansion || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            any = true;
            var enabled = !set.Contains(info.PrimaryActorOID);
            anyEnabled |= enabled;
            allEnabled &= enabled;
        }
        return (anyEnabled, any && allEnabled);
    }

    public (bool anyEnabled, bool allEnabled) CategoryEnabledState(BossModuleInfo.Category category)
    {
        var set = DisabledModuleOIDSet();
        var anyEnabled = false;
        var allEnabled = true;
        var any = false;
        foreach (var info in BossModuleRegistry.RegisteredModules.Values)
        {
            if (info.Category != category || !IncludeInSupportedFightControls(info))
            {
                continue;
            }
            any = true;
            var enabled = !set.Contains(info.PrimaryActorOID);
            anyEnabled |= enabled;
            allEnabled &= enabled;
        }
        return (anyEnabled, any && allEnabled);
    }

    private HashSet<uint> DisabledModuleOIDSet()
    {
        if (_disabledModuleOIDs != null)
        {
            return _disabledModuleOIDs;
        }

        var len = DisabledModuleOIDs.Length;
        var set = new HashSet<uint>(len);
        for (var i = 0; i < len; ++i)
        {
            set.Add(DisabledModuleOIDs[i]);
        }
        return _disabledModuleOIDs = set;
    }

    private void PersistDisabledModuleOIDs(HashSet<uint> set)
    {
        var persisted = new uint[set.Count];
        set.CopyTo(persisted);
        Array.Sort(persisted);
        DisabledModuleOIDs = persisted;
    }

    public bool ShowPrePullHintsFor(uint primaryActorOID) => !SuppressedPrePullHintOIDSet().Contains(primaryActorOID);

    public void SetShowPrePullHintsFor(uint primaryActorOID, bool show)
    {
        var set = SuppressedPrePullHintOIDSet();
        var suppressed = set.Contains(primaryActorOID);
        if (show == !suppressed)
        {
            return;
        }

        if (show)
        {
            set.Remove(primaryActorOID);
        }
        else
        {
            set.Add(primaryActorOID);
        }

        var persisted = new uint[set.Count];
        set.CopyTo(persisted);
        Array.Sort(persisted);
        SuppressedPrePullHintOIDs = persisted;
        Modified.Fire();
    }

    private HashSet<uint> SuppressedPrePullHintOIDSet()
    {
        if (_suppressedPrePullHintOIDs != null)
        {
            return _suppressedPrePullHintOIDs;
        }

        var len = SuppressedPrePullHintOIDs.Length;
        var set = new HashSet<uint>(len);
        for (var i = 0; i < len; ++i)
        {
            set.Add(SuppressedPrePullHintOIDs[i]);
        }
        return _suppressedPrePullHintOIDs = set;
    }
}
