using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

namespace BossMod;

sealed class DebugGraphics
{
    private class WatchedRenderObject
    {
        public List<uint> Data = [];
        public List<(int, int)> Modifications = [];
        public bool Live;
    }

    private bool _showGraphicsLeafCharactersOnly = true;
    private readonly Dictionary<IntPtr, WatchedRenderObject> _watchedRenderObjects = [];
    private bool _overlayCircle; // enables circular polar grid.
    private bool _overlayAllCustom; // The switch for drawing custom shapes on arena.
    private Vector2 _overlayCenter = new(100, 100);
    private Vector2 _overlayStep = new(2, 2);
    private Vector2 _overlayMaxOffset = new(20, 20);
    private Angle _overlayRotation = new(0); // Rotates the rectangle grid.
    private Angle _angleVisRotation = new(0); // Rotates the angle visualizer arm.
    private float _angVisRotDegrees = 0;
    private Vector2 _placedCenter = new(100, 100);
    private float _placedOffset = 4.0f; // for placing a drawn shape on overlay.
    private float _placedWidth = 0.5f; // width of drawn shape on overlay. Used  as radius in circles.
    private float _placedHeight = 0.5f;
    private Angle _placedRotation = new(0);
    private Angle _placedHalfAngle = new(0);
    private Vector3 _placedVec3 = Vector3.Zero;
    private Vector3 _placedOffsetLocation = Vector3.Zero;
    private int _placedEdges = 7;

    private readonly string[] _shapeTemplates =
    [
        "No Shape Template Selected", "Circle", "Rectangle", "Donut", "Cross", "DonutSegmentHA", "Ellipse", "Capsule"
    ]; // Need to implement shape functions if new options added.

    private int _selectedShapeIndex = -1;
    private List<Shape> _unionShapes = [];
    private List<Shape> _diffShapes = [];
    private List<Shape> _additionalShapes = [];
    private int _selectedUnionShapesIdx = -1;
    private int _selectedDiffShapesIdx = -1;
    private int _selectedAdditionalShapesIdx = -1;

    public unsafe void DrawSceneTree()
    {
        ImGui.Checkbox("Show only leafs of Character type", ref _showGraphicsLeafCharactersOnly);
        var root = FindSceneRoot();
        if (root != null)
        {
            DrawSceneNode(root);
        }
    }

    private unsafe void DrawSceneNode(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o)
    {
        var start = o;
        do
        {
            var nodeText = $"{SceneNodeText(o)}###{(IntPtr)o}";
            var nodeFlags = (o->ChildObject != null ? ImGuiTreeNodeFlags.None : ImGuiTreeNodeFlags.Leaf) |
                            ImGuiTreeNodeFlags.OpenOnArrow;
            var showNode = !_showGraphicsLeafCharactersOnly || o->ChildObject != null ||
                           o->GetObjectType() == ObjectType.CharacterBase;
            if (showNode && ImGui.TreeNodeEx(nodeText, nodeFlags))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    var watched = _watchedRenderObjects.ContainsKey((IntPtr)o);
                    if (!watched)
                    {
                        var size = 0x80;
                        switch (o->GetObjectType())
                        {
                            case ObjectType.CharacterBase:
                                size = 0x8F0;
                                break;
                            case ObjectType.VfxObject:
                                size = 0x1C8;
                                break;
                        }

                        WatchObject(o, size);
                    }
                    else
                    {
                        _watchedRenderObjects.Remove((IntPtr)o);
                    }
                }

                if (o->ChildObject != null)
                {
                    DrawSceneNode(o->ChildObject);
                }

                ImGui.TreePop();
            }

            o = o->NextSiblingObject;
        } while (o != start);
    }

    public unsafe void DrawWatchedMods()
    {
        if (ImGui.Button("Clear watch list"))
        {
            _watchedRenderObjects.Clear();
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear modifications"))
        {
            foreach (var v in _watchedRenderObjects)
            {
                v.Value.Modifications.Clear();
            }
        }

        if (_watchedRenderObjects.Count == 0)
        {
            return;
        }

        foreach (var v in _watchedRenderObjects)
        {
            v.Value.Live = false;
        }

        var root = FindSceneRoot();
        if (root != null)
        {
            UpdateWatchedMods(root);
        }

        List<IntPtr> del = [];
        foreach (var v in _watchedRenderObjects)
        {
            if (!v.Value.Live)
            {
                del.Add(v.Key);
            }
        }

        foreach (var v in del)
        {
            _watchedRenderObjects.Remove(v);
        }

        ImGui.BeginTable("watched_graphics", 2);
        ImGui.TableSetupColumn("Ptr", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("Data");
        ImGui.TableHeadersRow();
        foreach (var v in _watchedRenderObjects)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"0x{v.Key:X}");
            ImGui.TableNextColumn();
            DrawMods(v.Value);
        }

        ImGui.EndTable();

        foreach (var v in _watchedRenderObjects)
        {
            var obj = (FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object*)v.Key;
            Camera.Instance?.DrawWorldLine(Service.ObjectTable.LocalPlayer!.Position, obj->Position, Colors.TextColor3);
        }
    }

    public unsafe void WatchObject(void* o, int size)
    {
        if (_watchedRenderObjects.ContainsKey((IntPtr)o))
        {
            return;
        }

        var w = new WatchedRenderObject();
        for (var i = 0; i < size / 4; ++i)
        {
            w.Data.Add(((uint*)o)[i]);
        }

        _watchedRenderObjects.Add((IntPtr)o, w);
    }

    private unsafe void UpdateWatchedMods(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o)
    {
        var start = o;
        do
        {
            var watch = _watchedRenderObjects.GetValueOrDefault((IntPtr)o);
            if (watch != null)
            {
                UpdateWatchedMod(o, watch);
            }

            if (o->ChildObject != null)
            {
                UpdateWatchedMods(o->ChildObject);
            }

            o = o->NextSiblingObject;
        } while (o != start);
    }

    private unsafe void UpdateWatchedMod(void* o, WatchedRenderObject w)
    {
        w.Live = true;

        var start = 0;
        for (var i = 0; i < w.Modifications.Count; ++i)
        {
            (var end, var nextStart) = w.Modifications[i];
            var mods = CheckUnmodRange((uint*)o, w, start, end);
            if (mods != null)
            {
                w.Modifications.InsertRange(i, mods);
                i += mods.Count;
            }

            start = nextStart;
        }

        var endMods = CheckUnmodRange((uint*)o, w, start, w.Data.Count);
        if (endMods != null)
        {
            w.Modifications.AddRange(endMods);
        }

        for (var i = 0; i < w.Data.Count; ++i)
        {
            w.Data[i] = ((uint*)o)[i];
        }
    }

    private unsafe List<(int, int)>? CheckUnmodRange(uint* o, WatchedRenderObject w, int start, int end)
    {
        while (start < end && o[start] == w.Data[start])
        {
            ++start;
        }

        if (start == end)
        {
            return null; // nothing changed
        }

        List<(int, int)> res = [];
        while (start < end)
        {
            var m = start + 1;
            while (m < end && o[m] != w.Data[m])
            {
                ++m;
            }

            res.Add((start, m));
            start = m;
            while (start < end && o[start] == w.Data[start])
            {
                ++start;
            }
        }

        return res;
    }

    private void DrawMods(WatchedRenderObject w)
    {
        var start = 0;
        var sb = new StringBuilder();
        foreach ((var end, var nextStart) in w.Modifications)
        {
            DrawHexString(w, ref start, end, Colors.PlayerGeneric, sb);
            DrawHexString(w, ref start, nextStart, Colors.TextColor3, sb);
        }

        sb.Clear();
        DrawHexString(w, ref start, w.Data.Count, Colors.PlayerGeneric, sb);
    }

    private void DrawHexString(WatchedRenderObject w, ref int start, int end, uint color, StringBuilder sb)
    {
        sb.Clear();
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        while (start < end)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            sb.AppendFormat("{0:X8}", w.Data[start++]);

            if ((start & 15) == 0)
            {
                ImGui.TextUnformatted(sb.ToString());
                sb.Clear();
            }
        }

        ImGui.TextUnformatted(sb.ToString());
        ImGui.SameLine();
        ImGui.PopStyleColor();
    }

    public unsafe void DrawMatrices()
    {
        var camera = CameraManager.Instance()->CurrentCamera;
        if (camera == null)
        {
            return;
        }

        using var table = ImRaii.Table("matrices", 2);
        if (!table)
        {
            return;
        }

        ImGui.TableSetupColumn("名称");
        ImGui.TableSetupColumn("Value");
        ImGui.TableHeadersRow();

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("VP");
        ImGui.TableNextColumn();
        DrawMatrix(camera->ViewMatrix * camera->RenderCamera->ProjectionMatrix);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("P");
        ImGui.TableNextColumn();
        DrawMatrix(camera->RenderCamera->ProjectionMatrix);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("P2");
        ImGui.TableNextColumn();
        DrawMatrix(camera->RenderCamera->ProjectionMatrix2);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("V");
        ImGui.TableNextColumn();
        DrawMatrix(camera->ViewMatrix);

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("V2");
        ImGui.TableNextColumn();
        DrawMatrix(camera->RenderCamera->ViewMatrix);

        var altitude = MathF.Asin(camera->ViewMatrix.M23);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Camera Altitude");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(altitude.Radians().ToString());

        var azimuth = MathF.Atan2(camera->ViewMatrix.M13, camera->ViewMatrix.M33);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Camera Azimuth");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(azimuth.Radians().ToString());

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Origin");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(Utils.Vec3String(camera->RenderCamera->Origin));

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Near/far/aspect");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(
            $"{camera->RenderCamera->NearPlane} / {camera->RenderCamera->FarPlane} / {camera->RenderCamera->AspectRatio}");

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Projection flags");
        ImGui.TableNextColumn();
        if (ImGui.Button(camera->RenderCamera->IsOrtho
                ? $"ortho ({camera->RenderCamera->OrthoHeight})"
                : "perspective"))
        {
            camera->RenderCamera->IsOrtho ^= true;
        }

        ImGui.SameLine();
        if (ImGui.Button(camera->RenderCamera->StandardZ ? "standard-z" : "reverse-z"))
        {
            camera->RenderCamera->StandardZ ^= true;
        }

        ImGui.SameLine();
        if (ImGui.Button(camera->RenderCamera->FiniteFarPlane ? "finite-far" : "infinite-far"))
        {
            camera->RenderCamera->FiniteFarPlane ^= true;
        }

        var view = camera->ViewMatrix;
        var lx = new Vector3(view.M11, view.M21, view.M31);
        var ly = new Vector3(view.M12, view.M22, view.M32);
        var lz = new Vector3(view.M13, view.M23, view.M33);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("View handedness");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{Vector3.Dot(lz, Vector3.Cross(lx, ly))}");

        view.M44 = 1;
        FFXIVClientStructs.FFXIV.Common.Math.Matrix4x4.Invert(view, out var world);
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("W");
        ImGui.TableNextColumn();
        DrawMatrix(world);

        var device = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device.Instance();
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Viewport size");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{device->Width:f6} {device->Height:f6}");
    }

    private void DrawMatrix(FFXIVClientStructs.FFXIV.Common.Math.Matrix4x4 mtx)
    {
        ImGui.TextUnformatted($"{mtx[0]:f6} {mtx[1]:f6} {mtx[2]:f6} {mtx[3]:f6}");
        ImGui.TextUnformatted($"{mtx[4]:f6} {mtx[5]:f6} {mtx[6]:f6} {mtx[7]:f6}");
        ImGui.TextUnformatted($"{mtx[8]:f6} {mtx[9]:f6} {mtx[10]:f6} {mtx[11]:f6}");
        ImGui.TextUnformatted($"{mtx[12]:f6} {mtx[13]:f6} {mtx[14]:f6} {mtx[15]:f6}");
    }

    public void DrawOverlay()
    {
        if (Camera.Instance == null || Service.ObjectTable.LocalPlayer == null)
        {
            return;
        }

        ImGui.Checkbox("圆形", ref _overlayCircle);
        ImGui.DragFloat2("Center", ref _overlayCenter);
        ImGui.SameLine();
        // We can place the grid at players feet as long as we check player is not null
        if (ImGui.Button("Snap center to Player Location"))
        {
            if (Service.ObjectTable.LocalPlayer != null)
            {
                var newX = Service.ObjectTable.LocalPlayer.Position.X;
                var newY = Service.ObjectTable.LocalPlayer.Position.Z;

                _overlayCenter.X = newX;
                _overlayCenter.Y = newY;
                // adjust the shape template center also so that everything is aligned
                _placedCenter.X = newX;
                _placedCenter.Y = newY;
            }
        }

        ImGui.DragFloat2("Step", ref _overlayStep, 0.25f, 1, 10);
        ImGui.DragFloat2("Max offset", ref _overlayMaxOffset);

        var rotationDegrees = _overlayRotation.Deg;
        ImGui.SliderFloat("Rotation (Degrees) ## Grid overlay rotation", ref rotationDegrees, -180, 180, "%.2f");
        ImGui.SameLine();
        ImGui.InputFloat("##RotationInput", ref rotationDegrees, 0.1f, 1, "%.2f");
        _overlayRotation = new Angle(rotationDegrees * Angle.DegToRad);

        // instead of dividing by zero just provide the value.
        var mx = _overlayStep.X != 0 ? (int)(_overlayMaxOffset.X / _overlayStep.X) : 0;
        var mz = _overlayStep.Y != 0 ? (int)(_overlayMaxOffset.Y / _overlayStep.Y) : 0;
        var y = Service.ObjectTable.LocalPlayer!.Position.Y;

        var rotationMatrix = Matrix3x2.CreateRotation(-_overlayRotation.Rad);

        Vector2 TransformPoint(Vector2 point) =>
            Vector2.Transform(point - _overlayCenter, rotationMatrix) + _overlayCenter;

        if (_overlayCircle)
        {
            var center = new Vector3(_overlayCenter.X, y, _overlayCenter.Y);
            for (var ir = 0; ir <= mx; ++ir)
            {
                Camera.Instance.DrawWorldCircle(center, ir * _overlayStep.X, Colors.PC);
            }

            for (var ia = 0; ia < 8; ++ia)
            {
                var offset = ((ia * 22.5f.Degrees()).ToDirection() * _overlayMaxOffset.X).ToVec3();
                Camera.Instance.DrawWorldLine(center - offset, center + offset, Colors.PC);
            }
        }
        else
        {
            for (var ix = -mx; ix <= mx; ++ix)
            {
                var x = _overlayCenter.X + ix * _overlayStep.X;
                var start = TransformPoint(new Vector2(x, _overlayCenter.Y - _overlayMaxOffset.Y));
                var end = TransformPoint(new Vector2(x, _overlayCenter.Y + _overlayMaxOffset.Y));
                Camera.Instance.DrawWorldLine(new(start.X, y, start.Y), new(end.X, y, end.Y), Colors.PC);
            }

            for (var iz = -mz; iz <= mz; ++iz)
            {
                var z = _overlayCenter.Y + iz * _overlayStep.Y;
                var start = TransformPoint(new Vector2(_overlayCenter.X - _overlayMaxOffset.X, z));
                var end = TransformPoint(new Vector2(_overlayCenter.X + _overlayMaxOffset.X, z));
                Camera.Instance.DrawWorldLine(new(start.X, y, start.Y), new(end.X, y, end.Y), Colors.PC);
            }
        }

        ImGui.NewLine();
        ImGui.Separator();
        ImGui.Spacing();
        /*
         * Dropdown box that determines what shape template is being placed, circle, rectangle, etc.
         * Also has a 'no shape selected' option for putting the angle visualizer arm and shape template to invisible.
         */
        ImGui.Combo("Select Shape Template", ref _selectedShapeIndex, _shapeTemplates, _shapeTemplates.Length);
        if (_selectedShapeIndex > -0 && _selectedShapeIndex < _shapeTemplates.Length)
            InsertShapesIntoOverlay(_shapeTemplates[_selectedShapeIndex]);

        ArenaBoundsShapesTripleListTable();
        ImGui.NewLine();
    }

    /*
     * Draw the angle visualizer arm. An arm that rotates around center point of grid.
     * Used for seeing angles and placing selected shapes in overlay.
     */
    public void AngleVisualizer()
    {
        // These two get duplicated often. Probably should be higher in the hierarchy
        var y = Service.ObjectTable.LocalPlayer!.Position.Y;
        var center = new Vector3(_overlayCenter.X, y, _overlayCenter.Y);

        _angVisRotDegrees = _angleVisRotation.Deg;
        ImGui.SliderFloat("Arm Rotation (Degrees) ## Angle visualizer rotation", ref _angVisRotDegrees, -180, 180,
            "%.2f");

        /* Angle Visualizer for circular overlay. Show an angle with vec 3 coordinates.
         * This is drawing the line to move around and show the angle we are at.
         * Use the degrees slider to move the angle visualizer around the circle.
         */
        var pickOffset = (_angleVisRotation.ToDirection() * (_overlayMaxOffset.X)).ToVec3();
        var outsideVec3 = center + pickOffset;
        Camera.Instance!.DrawWorldLine(center, outsideVec3, Colors.Danger, thickness: 3);

        // Show the coordinates where the outside point of the visualizer arm is.
        ImGui.TextUnformatted("Vec3 coordinates at the end of angle visualizer arm : ");
        ImGui.TextUnformatted(Utils.Vec3String(outsideVec3));
    }

    // Create a method that will let you grab a shape and draw it in the overlay.
    // It should create boilerplate code for creating that shape in radar.
    public void InsertShapesIntoOverlay(string shape)
    {
        AngleVisualizer();
        // TODO figure out how to adjust the location by manual position entry or click on screen
        var y = Service.ObjectTable.LocalPlayer!.Position.Y;
        var origin = new Vector3(_overlayCenter.X, y, _overlayCenter.Y);
        ImGui.SliderFloat($"Placed {shape.ToLower()} offset", ref _placedOffset, 0f, 30f, "%.2f");
        _placedOffsetLocation = (_angleVisRotation.ToDirection() * _placedOffset).ToVec3();
        _placedVec3 = origin + _placedOffsetLocation;
        _placedCenter.X = _placedVec3.X;
        _placedCenter.Y = _placedVec3.Z;

        // Pattern match the shapes in order to call the correct ui features.
        // Should show sliders for width/height/rotation based on Shape.cs params.
        // Also define StringBuilder for each shape so that code generation can work

        /* TODO probably worth adding an option to enter coordinates for a shape directly instead of trying to
         * finesse it using the arm. Like if you get coordinates for something from logs, you could just enter and test
         * it matches. Will need a string parsing engine for that.
         */
        switch (shape)
        {
            case "Circle":
                PlacedRadiusSlider(shape);
                // TODO try sending to DrawWorldShapes to see if it looks janky like customBounds does
                Camera.Instance!.DrawWorldCircle(_placedVec3, _placedWidth, Colors.Danger, thickness: 3);
                var placedCirc = new Circle(new WPos(_placedCenter.X, _placedCenter.Y), _placedWidth);
                // Show the coordinates for the center of the placed shape
                PlaceCoordinates(_placedVec3, shape);
                AddShapeButtons(placedCirc);
                // Show sample generated code snippet for shape above code export button.
                ImGui.TextUnformatted(ShapeString(placedCirc).ToString());
                ShapeStringButton(placedCirc);
                break;
            case "Rectangle":
                PlacedHalfWidthSlider(shape);
                PlacedHalfHeightSlider(shape);
                // Rotation slider for the shape template
                PlacedRotationSlider(shape);
                var placedRect = new Rectangle(new WPos(_placedCenter.X, _placedCenter.Y), _placedWidth, _placedHeight,
                    _placedRotation);
                Camera.Instance!.DrawWorldRectangle(_placedVec3, placedRect, Colors.Danger, thickness: 3);
                // Show the coordinates for the center of the placed shape
                PlaceCoordinates(_placedVec3, shape);
                AddShapeButtons(placedRect);
                // Show sample generated code snippet for shape above code export button.
                ImGui.TextUnformatted(ShapeString(placedRect).ToString());
                ShapeStringButton(placedRect);
                break;
            case "Donut":
                PlacedInnerRadiusSlider(shape);
                PlacedOuterRadiusSlider(shape);
                var placedDonut = new Donut(new WPos(_placedCenter.X, _placedCenter.Y), _placedWidth, _placedHeight);
                Camera.Instance!.DrawWorldShape(_placedVec3, placedDonut, Colors.Danger, thickness: 3);
                // Show the coordinates for the center of the placed shape
                PlaceCoordinates(_placedVec3, shape);
                AddShapeButtons(placedDonut);
                // Show sample generated code snippet for shape above code export button.
                ImGui.TextUnformatted(ShapeString(placedDonut).ToString());
                ShapeStringButton(placedDonut);
                break;
            case "Cross":
                PlacedLengthSlider(shape);
                PlacedHalfWidthSlider(shape);
                PlacedRotationSlider(shape);
                var placedCross = new Cross(new WPos(_placedCenter.X, _placedCenter.Y), _placedHeight, _placedWidth,
                    _placedRotation);
                Camera.Instance!.DrawWorldShape(_placedVec3, placedCross, Colors.Danger, thickness: 3);
                // Show the coordinates for the center of the placed shape
                PlaceCoordinates(_placedVec3, shape);
                AddShapeButtons(placedCross);
                // Show sample generated code snippet for shape above code export button.
                ImGui.TextUnformatted(ShapeString(placedCross).ToString());
                ShapeStringButton(placedCross);
                break;
            case "DonutSegmentHA":
                PlacedInnerRadiusSlider(shape);
                PlacedOuterRadiusSlider(shape);
                PlacedRotationSlider(shape);
                PlacedHalfAngleSlider(shape);
                var placedDonutHA = new DonutSegmentHA(new WPos(_placedCenter.X, _placedCenter.Y), _placedHeight,
                    _placedWidth, _placedRotation, HalfAngle: _placedHalfAngle);
                Camera.Instance!.DrawWorldShape(_placedVec3, placedDonutHA, Colors.Danger, thickness: 3);
                // Show the coordinates for the center of the placed shape
                PlaceCoordinates(_placedVec3, shape);
                AddShapeButtons(placedDonutHA);
                // Show sample generated code snippet for shape above code export button.
                ImGui.TextUnformatted(ShapeString(placedDonutHA).ToString());
                ShapeStringButton(placedDonutHA);
                break;
            case "Ellipse":
                PlacedHalfWidthSlider(shape);
                PlacedHalfHeightSlider(shape);
                PlacedEdgesInput(shape);
                PlacedRotationSlider(shape);
                var placedEllipse = new Ellipse(new WPos(_placedCenter.X, _placedCenter.Y), _placedWidth, _placedHeight,
                    _placedEdges, _placedRotation);
                Camera.Instance!.DrawWorldShape(_placedVec3, placedEllipse, Colors.Danger, thickness: 3);
                // Show the coordinates for the center of the placed shape
                PlaceCoordinates(_placedVec3, shape);
                AddShapeButtons(placedEllipse);
                // Show sample generated code snippet for shape above code export button.
                ImGui.TextUnformatted(ShapeString(placedEllipse).ToString());
                ShapeStringButton(placedEllipse);
                break;
            case "Capsule":
                PlacedHalfHeightSlider(shape);
                PlacedHalfWidthSlider(shape);
                PlacedEdgesInput(shape);
                PlacedRotationSlider(shape);
                var placedCapsule = new Capsule(new WPos(_placedVec3.X, _placedVec3.Z), halfHeight: _placedHeight,
                    halfWidth: _placedWidth, edges: _placedEdges, rotation: _placedRotation);
                Camera.Instance!.DrawWorldShape(_placedVec3, placedCapsule, Colors.Danger, thickness: 3);
                // Show the coordinates for the center of the placed shape
                PlaceCoordinates(_placedVec3, shape);
                AddShapeButtons(placedCapsule);
                // Show sample generated code snippet for shape above code export button.
                ImGui.TextUnformatted(ShapeString(placedCapsule).ToString());
                ShapeStringButton(placedCapsule);
                break;
            default:
                break;
        }

        ;
    }

    public void PlaceCoordinates(Vector3 placedCoords, string shape)
    {
        // Show the coordinates for the center of the placed shape
        ImGui.TextUnformatted($"Coordinates in the center of placed {shape}: ");
        // Sets a minimum of 0.0f and a maximum of 100.0f for both X and Y
        // Originally wanted to be able to just enter coordinates and have template jump, but it is difficult because
        // location is tied to angle visualizer arm and offset variables also.
        ImGui.DragFloat2($"Center of Placed {shape}", ref _placedCenter, 0.25f, vMin: -(_overlayMaxOffset.X + 10),
            vMax: (_overlayMaxOffset.X + 10));
        _angleVisRotation = new Angle(_angVisRotDegrees * Angle.DegToRad);
    }

    ///
    /// Various widgets for the different parameters of shapes.
    /// Buttons and sliders for things like radius, half height, half width, etc.
    ///
    public void PlacedRotationSlider(string shape)
    {
        var placedRotationDegrees = _placedRotation.Deg;
        ImGui.SliderFloat($"Placed {shape.ToLower()} rotation (Degrees) ## Rotation for template shape",
            ref placedRotationDegrees, -180, 180, "%.2f");
        _placedRotation = new Angle(placedRotationDegrees * Angle.DegToRad);
    }

    public void PlacedHalfAngleSlider(string shape)
    {
        var placedHalfAngleDegrees = _placedHalfAngle.Deg;
        ImGui.SliderFloat($"Placed {shape.ToLower()} half angle (Degrees) ## Half angle slider",
            ref placedHalfAngleDegrees, -180, 180, "%.2f");
        _placedHalfAngle = new Angle(placedHalfAngleDegrees * Angle.DegToRad);
    }

    public void PlacedHalfWidthSlider(string shape)
    {
        ImGui.SliderFloat($"Placed {shape.ToLower()} half width", ref _placedWidth, 0, _overlayMaxOffset.X + 10,
            "%.2f");
    }

    public void PlacedHalfHeightSlider(string shape)
    {
        ImGui.SliderFloat($"Placed {shape.ToLower()} half height", ref _placedHeight, 0, _overlayMaxOffset.Y + 10,
            "%.2f");
    }

    public void PlacedLengthSlider(string shape)
    {
        ImGui.SliderFloat($"Placed {shape.ToLower()} length", ref _placedHeight, 0, _overlayMaxOffset.Y + 10,
            "%.2f");
    }

    public void PlacedRadiusSlider(string shape)
    {
        ImGui.SliderFloat($"Placed {shape.ToLower()} radius", ref _placedWidth, 0, _overlayMaxOffset.X + 10, "%.2f");
    }

    public void PlacedInnerRadiusSlider(string shape)
    {
        ImGui.SliderFloat($"Placed {shape.ToLower()} inner radius", ref _placedWidth, 0, _overlayMaxOffset.X + 10,
            "%.2f");
    }

    public void PlacedOuterRadiusSlider(string shape)
    {
        ImGui.SliderFloat($"Placed {shape.ToLower()} outer radius", ref _placedHeight, 0, _overlayMaxOffset.X + 10,
            "%.2f");
    }

    public void PlacedEdgesInput(string shape)
    {
        // Minus Button
        if (ImGui.Button(" - ##minus_int"))
        {
            _placedEdges -= 1;
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        ImGui.InputInt("Number of edges ", ref _placedEdges);
        ImGui.SameLine();

        // Plus Button
        if (ImGui.Button(" + ##plus_int"))
        {
            _placedEdges += 1;
        }
    }

    /// StringBuilder methods for generating one off Shape code snippets.
    /// outputs c# code. May want to clean up the coordinates and rotations for accuracy after output.
    /// It will only be as accurate as the placed shape template placement.
    /// Semicolons intentionally left off the end for use in generating code snippets for larger
    /// lists of shapes.
    ///
    /// Define a function for each shape that can be drawn.
    public StringBuilder CircleString(Circle circ)
    {
        var exportedCircle =
            new StringBuilder($"new Circle(new WPos({circ.Center.X:F2}f, {circ.Center.Z:F2}f), {circ.Radius:F2}f)");
        ImGui.TextUnformatted(exportedCircle.ToString());

        if (ImGui.Button("Copy Circle To Clipboard"))
        {
            ImGui.SetClipboardText(exportedCircle.ToString());
        }
        return exportedCircle;
    }

    public StringBuilder RectangleString(Rectangle rect)
    {
        var exportedRectangle =
            new StringBuilder(
                $"new Rectangle(new WPos({rect.Center.X:F2}f, {rect.Center.Z:F2}f), {rect.HalfWidth:F2}f, {rect.HalfHeight:F2}f, {rect.Rotation}f.Degrees()");
        ImGui.TextUnformatted(exportedRectangle.ToString());

        return exportedRectangle;
    }

    // Generate a code snippet for a single shape. X and Z coordinates are rounded from 3 decimal places down to 2.
    public StringBuilder ShapeString(Shape shape)
    {
        StringBuilder exportedShape = new StringBuilder();
        switch (shape)
        {
            case Circle c:
                exportedShape.Append(
                    $"new {shape.GetType().Name}(new WPos({c.Center.X:F2}f, {c.Center.Z:F2}f), {c.Radius:F2}f)");
                break;
            case Rectangle r:
                exportedShape.Append(
                    $"new {shape.GetType().Name}(new WPos({r.Center.X:F2}f, {r.Center.Z:F2}f), {r.HalfWidth:F2}f, {r.HalfHeight:F2}f, {r.Rotation}f.Degrees())");
                break;
            case Donut d:
                exportedShape.Append(
                    $"new {shape.GetType().Name}(new WPos({d.Center.X:F2}f, {d.Center.Z:F2}f), {d.InnerRadius:F2}f, {d.OuterRadius:F2}f)");
                break;
            case Cross cross:
                exportedShape.Append(
                    $"new {shape.GetType().Name}(new WPos({cross.Center.X:F2}f, {cross.Center.Z:F2}f), {cross.Length:F2}f, {cross.HalfWidth:F2}f, {cross.Rotation:F2}f.Degrees())");
                break;
            case DonutSegmentHA donutSegmentHA:
                exportedShape.Append(
                    $"new {shape.GetType().Name}(new WPos({donutSegmentHA.Center.X:F2}f, {donutSegmentHA.Center.Z:F2}f), {donutSegmentHA.InnerRadius:F2}f, {donutSegmentHA.OuterRadius:F2}f, {donutSegmentHA.EndAngle:F2}f.Degrees(), {donutSegmentHA.EndAngle}f.Degrees())");
                break;
            case Ellipse ellipse:
                exportedShape.Append(
                    $"new {shape.GetType().Name}(new WPos({ellipse.Center.X:F2}f, {ellipse.Center.Z:F2}f), {ellipse.HalfWidth:F2}f, {ellipse.HalfHeight:F2}f, {ellipse.Edges}, {ellipse.Rotation}f.Degrees())");
                break;
            case Capsule capsule:
                exportedShape.Append(
                    $"new {shape.GetType().Name}(new WPos({capsule.Center.X:F2}f, {capsule.Center.Z:F2}f), {capsule.HalfHeight:F2}f, {capsule.HalfWidth:F2}f, {capsule.Edges}, {capsule.Rotation}f.Degrees())");
                break;
            default:
                break;
        }

        ;
        return exportedShape;
    }

    public void ShapeStringButton(Shape shape)
    {
        if (ImGui.Button($"Copy {shape.GetType().Name} To Clipboard"))
        {
            ImGui.SetClipboardText(ShapeString(shape).ToString());
        }
    }

    public StringBuilder ListString(List<Shape> shapes, string listName)
    {
        var exportedList = new StringBuilder($"List<Shape>  {listName} = [");

        // TODO iterate through list and call StringBuilder function based off Shape Type pattern match.
        for (int i = 0; i < shapes.Count; i++)
        {
            // Pattern matching Shape type lets us cast to the shape.
            var shape = shapes[i];
            if (i == 0)
                exportedList.Append(ShapeString(shape));
            else
            {
                exportedList.Append($", {ShapeString(shape)}");
            }
        }
        exportedList.Append("];");

        return exportedList;
    }

    // Export a generated code snippet for a List<Shape> to clipboard.
    public void ListStringButton(List<Shape> shapes, string listName)
    {
        if (ImGui.Button($"Copy {listName} To Clipboard"))
        {
            ImGui.SetClipboardText(ListString(shapes, listName).ToString());
        }
    }

    public void AddShapeButtons(Shape addShape)
    {
        Type typeInfo = addShape.GetType();
        string shapeName = typeInfo.Name;
        // Only add the shape if it does not already exist in one of the lists.
        bool shapeExists = (_unionShapes.Exists(shape => shape.ToString() == addShape.ToString()) ||
                            _diffShapes.Exists(shape => shape.ToString() == addShape.ToString()) ||
                            _additionalShapes.Exists(shape => shape.ToString() == addShape.ToString()));

        if (ImGui.Button($"Add {shapeName} To UnionShapes[]"))
        {
            if (!shapeExists)
                _unionShapes.Add(addShape);
        }

        ImGui.SameLine();
        if (ImGui.Button($"Add {shapeName} To DiffShapes[]"))
        {
            if (!shapeExists)
                _diffShapes.Add(addShape);
        }

        ImGui.SameLine();
        if (ImGui.Button($"Add {shapeName} To AdditionalShapes[]"))
        {
            if (!shapeExists)
                _additionalShapes.Add(addShape);
        }
    }

    // TODO might be worth having an import box that can parse a custom shape or custom arena bound for visually checking existing code.
    public void DrawCameraShapes()
    {
        // TODO add checkboxes for additional arena bounds custom options. maybe a drop box for the colors.
        ArenaBoundsCustom arena = new([.. _unionShapes], [.. _diffShapes], [.. _additionalShapes]);

        if (_overlayAllCustom)
        {
            // DrawWorldPoly for drawing the arena bounds or a custom polygon as needed.
            var center = new Vector3(arena.Center.X, Service.ObjectTable.LocalPlayer!.Position.Y, arena.Center.Z);
            Camera.Instance!.DrawWorldPoly(center, arena.Polygon, Colors.Border, 3f);
        }
    }

    // Draw a single shape
    public void DrawCameraShape(WPos center, Shape shape, uint color, float thickness)
    {
        // TODO add checkboxes for additional arena bounds custom options. maybe a drop box for the colors.
        var centerVec3 = new Vector3(center.X, Service.ObjectTable.LocalPlayer!.Position.Y, center.Z);
        Camera.Instance!.DrawWorldShape(centerVec3, shape, color, thickness);
    }

    /* This table represents the visual arrays of arena bounds custom.
     * Has a list for each array and what items are in them.
     * Has buttons to allow moving and removing shapes from between the different lists.
     * Only appears once a single shape has been added. Not  all lists need shapes.
     */
    public void ArenaBoundsShapesTripleListTable()
    {
        if (_unionShapes.Count > 0 || _diffShapes.Count > 0 || _additionalShapes.Count > 0)
        {
            ImGui.Checkbox("Draw Shapes on Arena", ref _overlayAllCustom);
            // Draw all custom polygon arena bounds in the world.
            DrawCameraShapes();

            ImGui.BeginTable("triple_list_table", 5, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("UnionShapes", ImGuiTableColumnFlags.WidthStretch, 200f);
            ImGui.TableSetupColumn("Buttons1", ImGuiTableColumnFlags.WidthFixed, 50f);
            ImGui.TableSetupColumn("DiffShapes", ImGuiTableColumnFlags.WidthStretch, 200f);
            ImGui.TableSetupColumn("Buttons2", ImGuiTableColumnFlags.WidthFixed, 50f);
            ImGui.TableSetupColumn("AdditionalShapes", ImGuiTableColumnFlags.WidthStretch, 200f);
            ImGui.TableNextRow();

            // Column 1: _unionShapes List
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("UnionShapes");
            ImGui.BeginChild("UnionRegion", new System.Numerics.Vector2(0, 100), true);
            for (int i = 0; i < _unionShapes.Count; i++)
            {
                bool isUnionSelected = (_selectedUnionShapesIdx == i);
                if (ImGui.Selectable(_unionShapes[i].ToString(), isUnionSelected))
                {
                    _selectedUnionShapesIdx = i;
                    // Highlight a selected shape briefly so we can visually see which one we are focused on.
                    var selectedShape = _unionShapes[_selectedUnionShapesIdx];
                    // Briefly highlight selected shape in list so we can visually tell where things are.
                    HighlightSelection(selectedShape);
                }
            }

            ImGui.EndChild();

            // Column 2: Transfer Buttons
            ImGui.TableSetColumnIndex(1);
            ImGui.Dummy(new System.Numerics.Vector2(0, 40)); // spacing
            if (ImGui.Button(" > ## column 2 move right") && _selectedUnionShapesIdx >= 0 &&
                _selectedUnionShapesIdx < _unionShapes.Count)
            {
                _diffShapes.Add(_unionShapes[_selectedUnionShapesIdx]);
                _unionShapes.RemoveAt(_selectedUnionShapesIdx);
                _selectedUnionShapesIdx = -1;
            }

            if (ImGui.Button(" < ## column 2 move left") && _selectedDiffShapesIdx >= 0 &&
                _selectedDiffShapesIdx < _diffShapes.Count)
            {
                _unionShapes.Add(_diffShapes[_selectedDiffShapesIdx]);
                _diffShapes.RemoveAt(_selectedDiffShapesIdx);
                _selectedDiffShapesIdx = -1;
            }

            // Column 3: _diffShapes List
            ImGui.TableSetColumnIndex(2);
            ImGui.Text("DiffShapes");
            ImGui.BeginChild("DiffRegion", new System.Numerics.Vector2(0, 100), true);
            for (int i = 0; i < _diffShapes.Count; i++)
            {
                bool isSelected = (_selectedDiffShapesIdx == i);
                if (ImGui.Selectable(_diffShapes[i].ToString(), isSelected))
                {
                    _selectedDiffShapesIdx = i;
                    // Highlight a selected shape briefly so we can visually see which one we are focused on.
                    var selectedShape = _diffShapes[_selectedDiffShapesIdx];
                    // Briefly highlight selected shape in list so we can visually tell where things are.
                    // Can tap the shape repeatedly to make it flash again.
                    HighlightSelection(selectedShape);
                }
            }
            ImGui.EndChild();

            // Column 3: Transfer Buttons between _diffShapes and _additionalShapes
            // Transfer buttons become '>>' and '<<' to avoid problems with existing buttons called '>' and '<'
            ImGui.TableSetColumnIndex(3);
            ImGui.Dummy(new System.Numerics.Vector2(0, 40)); // spacing
            if (ImGui.Button(" > ## column 4 move right") && _selectedDiffShapesIdx >= 0 &&
                _selectedDiffShapesIdx < _diffShapes.Count)
            {
                _additionalShapes.Add(_diffShapes[_selectedDiffShapesIdx]);
                _diffShapes.RemoveAt(_selectedDiffShapesIdx);
                _selectedDiffShapesIdx = -1;
            }

            if (ImGui.Button(" < ## column 4 move left") && _selectedAdditionalShapesIdx >= 0 &&
                _selectedAdditionalShapesIdx < _additionalShapes.Count)
            {
                _diffShapes.Add(_additionalShapes[_selectedAdditionalShapesIdx]);
                _additionalShapes.RemoveAt(_selectedAdditionalShapesIdx);
                _selectedAdditionalShapesIdx = -1;
            }

            // Column 5: _additionalShapes List
            ImGui.TableSetColumnIndex(4);
            ImGui.Text("AdditionalShapes");
            ImGui.BeginChild("AdditionalRegion", new System.Numerics.Vector2(0, 100), true);
            for (int i = 0; i < _additionalShapes.Count; i++)
            {
                bool isSelected = (_selectedAdditionalShapesIdx == i);
                if (ImGui.Selectable(_additionalShapes[i].ToString(), isSelected))
                {
                    _selectedAdditionalShapesIdx = i;
                    // Highlight a selected shape briefly so we can visually see which one we are focused on.
                    var selectedShape = _additionalShapes[_selectedAdditionalShapesIdx];
                    // Briefly highlight selected shape in list so we can visually tell where things are.
                    HighlightSelection(selectedShape);
                }
            }

            ImGui.EndChild();
            ImGui.EndTable();

            // Various item deletion options from Shapes lists
            if (ImGui.Button(" Remove Single Shape") &&
                ((_selectedDiffShapesIdx >= 0 && _selectedDiffShapesIdx < _diffShapes.Count) ||
                 (_selectedUnionShapesIdx >= 0 && _selectedUnionShapesIdx < _unionShapes.Count) ||
                 (_selectedAdditionalShapesIdx >= 0 && _selectedAdditionalShapesIdx < _additionalShapes.Count)))
            {
                if (_selectedDiffShapesIdx >= 0)
                {
                    _diffShapes.RemoveAt(_selectedDiffShapesIdx);
                    _selectedDiffShapesIdx = -1;
                }
                else if (_selectedUnionShapesIdx >= 0)
                {
                    _unionShapes.RemoveAt(_selectedUnionShapesIdx);
                    _selectedUnionShapesIdx = -1;
                }
                else if (_selectedAdditionalShapesIdx >= 0)
                {
                    _additionalShapes.RemoveAt(_selectedAdditionalShapesIdx);
                    _selectedAdditionalShapesIdx = -1;
                }
            }

            if (ImGui.Button("Clear Union List"))
            {
                _unionShapes.Clear();
            }

            ImGui.SameLine();
            if (ImGui.Button("Clear Diff List"))
            {
                _diffShapes.Clear();
            }

            ImGui.SameLine();
            if (ImGui.Button("Clear Additional List"))
            {
                _additionalShapes.Clear();
            }

            ImGui.SameLine();
            if (ImGui.Button("Clear All Shapes"))
            {
                _unionShapes.Clear();
                _selectedUnionShapesIdx = -1;
                _diffShapes.Clear();
                _selectedDiffShapesIdx = -1;
                _additionalShapes.Clear();
                _selectedAdditionalShapesIdx = -1;
            }

            if (ImGui.CollapsingHeader("Union Shapes List code snippet generation."))
            {
                ListCodeGenWidget(_unionShapes, "unionShapes");
            }

            if (ImGui.CollapsingHeader("Difference Shapes List code snippet generation."))
            {
                ListCodeGenWidget(_diffShapes, "diffShapes");
            }

            if (ImGui.CollapsingHeader("Additional Shapes List code snippet generation."))
            {
                ListCodeGenWidget(_additionalShapes, "additionalShapes");
            }
        }
    }

    // This is the code export button for a whole List<Shapes>
    public void ListCodeGenWidget(List<Shape> shapes, string listName)
    {
        if (shapes.Count > 0)
        {
            // Show the preview of generated code. Then export button.
            ImGui.TextWrapped(ListString(shapes, listName).ToString());
            ListStringButton(shapes, listName);
        }
    }

    // Pattern match the shapes so we can pull out the Center parameter. Then draw briefly for visual identification.
    public void HighlightSelection(Shape shape)
    {
        switch (shape)
        {
            case Circle c:
                // We use Camera.Instance.DrawWorldCircle because it looks better.
                Camera.Instance!.DrawWorldCircle(
                    new Vector3(c.Center.X, Service.ObjectTable.LocalPlayer!.Position.Y, c.Center.Z), c.Radius,
                    Colors.Focus, 3f);
                break;
            case Rectangle r:
                DrawCameraShape(r.Center, r, Colors.Focus, 3f);
                break;
            case Donut d:
                DrawCameraShape(d.Center, d, Colors.Focus, 3f);
                break;
            case Cross cross:
                DrawCameraShape(cross.Center, cross, Colors.Focus, 3f);
                break;
            case DonutSegmentHA donutSegmentHA:
                DrawCameraShape(donutSegmentHA.Center, donutSegmentHA, Colors.Focus, 3f);
                break;
            case Ellipse ellipse:
                DrawCameraShape(ellipse.Center, ellipse, Colors.Focus, 3f);
                break;
            case Capsule capsule:
                DrawCameraShape(capsule.Center, capsule, Colors.Focus, 3f);
                break;
            default:
                break;
        }

        ;
    }

    public static unsafe FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* FindSceneRoot()
    {
        var player = Utils.GameObjectInternal(Service.ObjectTable.LocalPlayer);
        if (player == null || player->DrawObject == null)
        {
            return null;
        }

        var obj = &player->DrawObject->Object;
        while (obj->ParentObject != null)
        {
            obj = obj->ParentObject;
        }

        return obj;
    }

    public static unsafe void DumpScene()
    {
        var res = new StringBuilder("--- graphics scene dump ---");
        var root = FindSceneRoot();
        if (root != null)
        {
            DumpSceneNode(res, root, "");
        }

        Service.Log(res.ToString());
    }

    private static unsafe void DumpSceneNode(StringBuilder res,
        FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o, string prefix)
    {
        var start = o;
        do
        {
            res.Append($"\n{prefix} {SceneNodeText(o)}");
            if (o->ChildObject != null)
            {
                DumpSceneNode(res, o->ChildObject, prefix + "-");
            }

            o = o->NextSiblingObject;
        } while (o != start);
    }

    private static unsafe string SceneNodeText(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o)
    {
        var t = o->GetObjectType();
        var s =
            $"0x{(IntPtr)o:X}: t={t}, flags={Utils.SceneObjectFlags(o):X}, pos={Utils.Vec3String(o->Position)}, rot={Utils.QuatString(o->Rotation)}, scale={Utils.Vec3String(o->Scale)}";
        switch (t)
        {
            case ObjectType.VfxObject:
                s += $", ac={Utils.ReadField<int>(o, 0x128):X}, at={Utils.ReadField<int>(o, 0x130):X}, sc={Utils.ReadField<int>(o, 0x1B8):X}, st={Utils.ReadField<int>(o, 0x1C0)}:X";
                break;
        }
        return s;
    }
}
