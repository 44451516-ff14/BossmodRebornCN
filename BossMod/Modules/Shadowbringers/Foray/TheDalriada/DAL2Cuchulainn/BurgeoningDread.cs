namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL2Cuchulainn;

sealed class BurgeoningDread(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, stopAtWall: true)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => FleshNecromass.Circles.Check(pos, Arena.Center, default);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0)
        {
            return;
        }

        ref var move0 = ref state.PendingMoves.Ref(0);
        var act = move0.activation;
        var aoes = FleshNecromass.Positions;
        var len = aoes.Length;
        var pos = actor.Position;
        var moveDir = move0.dir.ToDirection();
        for (var i = 0; i < len; ++i)
        {
            var origin = aoes[i];
            var d = origin - pos;
            var dist = d.Length();

            if (dist is <= 6f or >= 25f) // inside voidzone or max distance 3s * 6 + ~6 radius + 1 safety margin
            {
                continue; // inside voidzone or impossible to run into this mine from current position
            }

            var forward = d.Dot(moveDir);
            var sideways = d.Dot(moveDir.OrthoL());

            hints.ForbiddenDirections.Add(new(Angle.Atan2(sideways, forward), Angle.Asin(6f / dist), act));
        }
    }
}
