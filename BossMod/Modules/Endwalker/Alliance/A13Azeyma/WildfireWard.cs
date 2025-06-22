namespace BossMod.Endwalker.Alliance.A13Azeyma;

class WildfireWard(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.IlluminatingGlimpse, 15f, false, 1, kind: Kind.DirLeft)
{
    private static readonly WPos south = new(-750f, -744.5f), east = new(-745f, -753f), west = new(-755f, -753f);
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            var act = Module.CastFinishAt(source.CastInfo);
            if (!IsImmune(slot, act))
            {
                var x = (int)source.Position.X;
                var pos = x == -777 ? south : x == -726 ? east : west;
                hints.AddForbiddenZone(ShapeDistance.InvertedRect(pos, source.Rotation + 90f.Degrees(), 2f, 1f, 1f), act);
            }
        }
    }
}

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Polygon[] triangle = [new(A13Azeyma.NormalCenter, 13.279f, 3, 180f.Degrees())];
    private static readonly AOEShapeCustom triangleCutOut = new([new Square(A13Azeyma.NormalCenter, 29.5f)], triangle);
    private static readonly ArenaBoundsComplex triangleBounds = new(triangle);

    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x1C)
        {
            switch (state)
            {
                case 0x00020001u:
                    _aoe = new(triangleCutOut, A13Azeyma.NormalCenter, default, WorldState.FutureTime(5.7d));
                    break;
                case 0x00200010u:
                    _aoe = null;
                    Arena.Bounds = triangleBounds;
                    Arena.Center = triangleBounds.Center;
                    break;
                case 0x00080004u:
                    Arena.Bounds = A13Azeyma.NormalBounds;
                    Arena.Center = A13Azeyma.NormalBounds.Center;
                    break;
            }
        }
    }
}
