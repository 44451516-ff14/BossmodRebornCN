namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

sealed class WickedHypercannon(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 10f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WickedHypercannonVisual2 or (uint)AID.WickedHypercannonVisual3)
        {
            if (Arena.Center == ArenaChanges.EastremovedCenter)
                AddAOE(new(85f, 80f));
            else if (Arena.Center == ArenaChanges.WestRemovedCenter)
                AddAOE(new(115f, 80f));
            void AddAOE(WPos pos) => _aoe = new(rect, pos, default, Module.CastFinishAt(spell, 0.6f), Colors.SafeFromAOE, false);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WickedHypercannon)
        {
            if (++NumCasts == 10)
            {
                _aoe = null;
                NumCasts = 0;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe != null)
        {
            var activation = _aoe?.Activation!;
            hints.AddPredictedDamage(Raid.WithSlot(false, true, true).Mask(), (DateTime)activation);
        }
    }
}
