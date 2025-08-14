namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL2Cuchulainn;

sealed class BurgeoningDread(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, stopAtWall: true)
{
    private static readonly Random random = new();
    private float rng = random.Next(1, 51); // used as pseudo randomisation
    private static readonly Angle a90 = 90f.Degrees(), a175 = 175f.Degrees();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => FleshNecromass.Circles.Check(pos, Arena.Center, default);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        base.OnStatusGain(actor, status);
        if (status.ID == (uint)SID.ForcedMarch)
        {
            rng = random.Next(1, 51);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0)
        {
            return;
        }
        var act = state.PendingMoves[0].activation;
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(DAL2Cuchulainn.ArenaCenter, 5f), act);
        hints.ForbiddenDirections.Add((a90 * rng, a175, act));
    }
}
