using Dalamud.Bindings.ImGui;

namespace BossMod.Global.DeepDungeon;

abstract partial class AutoClear : ZoneModule
{
    public override void DrawExtra()
    {
        var player = World.Party.Player()!;
        var lenP = Palace.Party.Length;
        var playerSlot = -1;
        var id = player.InstanceID;
        for (var i = 0; i < lenP; ++i)
        {
            ref readonly var p = ref Palace.Party[i];
            if (p.EntityId == id)
            {
                playerSlot = i;
                break;
            }
        }

        var targetRoom = new Minimap(Palace, player?.Rotation ?? default, DesiredRoom, Math.Max(0, playerSlot), player?.InstanceID ?? default).Draw();
        if (targetRoom >= 0)
            DesiredRoom = targetRoom;

        ImGui.Text($"Kills: {Kills}");

        var maxPull = Config.MaxPull;

        ImGui.SetNextItemWidth(200);
        if (ImGui.DragInt("Max mobs to pull", ref maxPull, 0.05f, 1, 15))
        {
            Config.MaxPull = maxPull;
            Config.Modified.Fire();
        }

        if (ImGui.Button("Reload obstacles"))
        {
            _obstacles.Dispose();
            _obstacles = new(World);
        }

        if (player == null)
            return;

        var (entry, data) = _obstacles.Find(player.PosRot.XYZ());
        if (entry == null)
        {
            ImGui.SameLine();
            UIMisc.HelpMarker(() => "Obstacle map missing for floor!", Dalamud.Interface.FontAwesomeIcon.ExclamationTriangle);
        }

        if (data != null && data.PixelSize != 0.5f)
        {
            ImGui.SameLine();
            UIMisc.HelpMarker(() => $"Wrong resolution for map; should be 0.5, got {data.PixelSize}", Dalamud.Interface.FontAwesomeIcon.ExclamationTriangle);
        }

        if (ImGui.Button("Set closest trap location as ignored"))
        {
            WPos? pos = null;
            var minDistanceSq = float.MaxValue;
            var lenCurrent = _trapsCurrentZone.Length;
            var countProblematic = ProblematicTrapLocations.Count;
            for (var i = 0; i < lenCurrent; ++i)
            {
                ref var trap = ref _trapsCurrentZone[i];
                var isProblematic = false;
                for (var j = 0; j < countProblematic; ++j)
                {
                    if (trap == ProblematicTrapLocations[j])
                    {
                        isProblematic = true;
                        break;
                    }
                }

                if (isProblematic)
                    continue;

                var distanceSq = (trap - player.Position).LengthSq();

                if (distanceSq < minDistanceSq)
                {
                    minDistanceSq = distanceSq;
                    pos = trap;
                }
            }
            if (pos is WPos position)
            {
                pos = position.Rounded(0.1f);
                ProblematicTrapLocations.Add(position);
                IgnoreTraps.Add(position);
            }
        }
    }
}
