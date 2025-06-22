namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class PowerfulGustKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.PowerfulGust, 20f, kind: Kind.DirForward, stopAfterWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            var act = Module.CastFinishAt(source.CastInfo);
            if (!IsImmune(slot, act))
                hints.AddForbiddenZone(ShapeDistance.InvertedRect(source.Position, source.Rotation, 9.5f, default, 20f), act);
        }
    }
}

class DownburstKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Downburst, 10f, stopAfterWall: true)
{
    private Angle offset;
    private static readonly WPos botLeft = new(92.5f, 100f), botRight = new(107.5f, 100f), topRight = new(107.5f, 85f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            var act = Module.CastFinishAt(source.CastInfo);
            if (!IsImmune(slot, act))
            {
                if (source.Position != Arena.Center)
                {
                    offset = source.Position == topRight ? -90f.Degrees() : source.Position == botLeft ? 90f.Degrees() : source.Position == botRight ? 180f.Degrees() : default;
                    hints.AddForbiddenZone(ShapeDistance.InvertedCone(source.Position, 5f, source.Rotation + offset, 10f.Degrees()), act);
                }
                else
                {
                    var forbidden = new Func<WPos, float>[4];
                    for (var i = 0; i < 4; ++i)
                        forbidden[i] = ShapeDistance.InvertedCone(source.Position, 5, source.Rotation + Angle.AnglesCardinals[i], 10f.Degrees());
                    hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), act);
                }
            }
        }
    }
}

class PowerfulGustRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.PowerfulGust);
class DownburstRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.Downburst);
