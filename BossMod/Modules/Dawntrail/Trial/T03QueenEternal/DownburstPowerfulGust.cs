namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class PowerfulGustKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.PowerfulGust), 20, kind: Kind.DirForward, stopAfterWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default && !IsImmune(slot, source.Activation))
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(source.Origin, source.Direction, 9.5f, 0, 20), source.Activation);
    }
}

class DownburstKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Downburst), 10, stopAfterWall: true)
{
    private Angle offset;
    private static readonly WPos botLeft = new(92.5f, 100);
    private static readonly WPos botRight = new(107.5f, 100);
    private static readonly WPos topRight = new(107.5f, 85);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default && !IsImmune(slot, source.Activation))
        {
            if (source.Origin != Arena.Center)
            {
                offset = source.Origin == topRight ? -90.Degrees() : source.Origin == botLeft ? 90.Degrees() : source.Origin == botRight ? 180.Degrees() : 0.Degrees();
                hints.AddForbiddenZone(ShapeDistance.InvertedCone(source.Origin, 5, source.Direction + offset, 10.Degrees()), source.Activation);
            }
            else
            {
                var forbidden = new List<Func<WPos, float>>();
                for (var i = 0; i < 4; ++i)
                    forbidden.Add(ShapeDistance.InvertedCone(source.Origin, 5, source.Direction + Angle.AnglesCardinals[i], 10.Degrees()));
                hints.AddForbiddenZone(p => forbidden.Max(f => f(p)), source.Activation);
            }
        }
    }
}

class PowerfulGustRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PowerfulGust));
class DownburstRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Downburst));
