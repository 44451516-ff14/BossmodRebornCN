namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

// TODO rewrite this class -> there is a better way to do tiles rather than making your own grid map
class AquaSpear(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    
    private float cellSize = 8f;
    private int gridSize = 5;
    private AOEShapeRect cellShape = new(4, 4, 4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
    
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.WaterZone1) {
            int idx = PositionToIndex(caster.Position);
            var pos = IndexToPosition(idx);
            aoes.Add(new AOEInstance(cellShape, pos, default, WorldState.CurrentTime, Colors.Danger));
        }
    }
    
    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if ((AID)spell.Action.ID == AID.WaterZone3 || (AID)spell.Action.ID == AID.WaterZone2) {
            int idx = PositionToIndex(caster.Position);
            var pos = IndexToPosition(idx);
            aoes.Add(new AOEInstance(cellShape, pos, default, WorldState.CurrentTime, Colors.Danger));
            NumCasts++;
        }
    }
    
    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaBackground(pcSlot, pc);
        
        var center = Arena.Center;
        
        float total = gridSize * cellSize;
        float half = total / 2f;
        float startX = center.X - half + cellSize / 2f;
        float startZ = center.Z - half + cellSize / 2f;
        uint white = 0xFFFFFFFF;
        float thickness = 2f;

        float front = cellSize / 2f;
        float back = cellSize / 2f;
        float halfWidth = cellSize / 2f;
        var dir = new WDir(1, 0);

        for (int iz = 0; iz < gridSize; ++iz) {
            for (int ix = 0; ix < gridSize; ++ix) {
                var pos = new WPos(startX + ix * cellSize, startZ + iz * cellSize);
                Arena.AddRect(pos, dir, front, back, halfWidth, white, thickness);
            }
        }
    }
    
    private WPos IndexToPosition(int index) {
        float startX = Arena.Center.X - ((gridSize * cellSize) / 2f) + cellSize / 2f;
        float startZ = Arena.Center.Z - ((gridSize * cellSize) / 2f) + cellSize / 2f;
        float x = startX + (index % gridSize) * cellSize;
        float z = startZ + (int)(index / gridSize) * cellSize;
        return new WPos(x, z);
    }
    
    private int PositionToIndex(WPos pos) {
        float startX = Arena.Center.X - ((gridSize * cellSize) / 2f) + cellSize / 2f;
        float startZ = Arena.Center.Z - ((gridSize * cellSize) / 2f) + cellSize / 2f;
        float relX = pos.X - startX;
        float relZ = pos.Z - startZ;

        int x = (int)MathF.Round(relX / cellSize);
        int z = (int)MathF.Round(relZ / cellSize);

        x = Math.Clamp(x, 0, gridSize - 1);
        z = Math.Clamp(z, 0, gridSize - 1);
        return z * gridSize + x;
    }
}