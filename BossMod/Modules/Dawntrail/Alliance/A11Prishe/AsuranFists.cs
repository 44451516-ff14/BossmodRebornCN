namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class AsuranFists(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AsuranFistsVisual)
        {
            Towers.Add(new(spell.LocXZ, 6f, PartyState.MaxAllianceSize, PartyState.MaxAllianceSize, activation: Module.CastFinishAt(spell, 0.5f)));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (Towers.Count != 0 && actor.OID == (uint)OID.Tower && state == 0x00100020u)
        {
            Towers[0] = Towers[0] with { Position = actor.Position }; // spell position can be about one or two pixels off the real tower position...
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AsuranFists1 or (uint)AID.AsuranFists2 or (uint)AID.AsuranFists3)
        {
            ++NumCasts;
            if (spell.Action.ID == (uint)AID.AsuranFists3)
                Towers.Clear();
        }
    }
}
