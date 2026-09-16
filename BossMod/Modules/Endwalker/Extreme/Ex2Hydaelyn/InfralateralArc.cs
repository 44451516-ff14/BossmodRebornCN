namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for infralateral arc mechanic (role stacks)
sealed class InfralateralArc(BossModule module) : Components.CastCounter(module, (uint)AID.InfralateralArcAOE)
{
    private readonly Angle _coneHalfAngle = 45f.Degrees();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var pcRole = EffectiveRole(actor);
        var pcDir = Angle.FromDirection(actor.Position - Module.PrimaryActor.Position);
        if (Raid.WithoutSlot(false, true, true).Any(a => EffectiveRole(a) != pcRole && a.Position.InCone(Module.PrimaryActor.Position, pcDir, _coneHalfAngle)))
            hints.Add("Spread by roles!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var pcRole = EffectiveRole(pc);
        var pcDir = Angle.FromDirection(pc.Position - Module.PrimaryActor.Position);
        foreach (var actor in Raid.WithoutSlot(false, true, true).Where(a => EffectiveRole(a) != pcRole))
        {
            if (EffectiveRole(actor) == pcRole)
            {
                continue;
            }
            var isincone = actor.Position.InCone(Module.PrimaryActor.Position, pcDir, _coneHalfAngle);
            Arena.Actor(actor, isincone ? Colors.Danger : Colors.PlayerGeneric, drawWorld: isincone ? true : null);
        }
    }

    private static Role EffectiveRole(Actor a) => a.Role == Role.Ranged ? Role.Melee : a.Role;
}
