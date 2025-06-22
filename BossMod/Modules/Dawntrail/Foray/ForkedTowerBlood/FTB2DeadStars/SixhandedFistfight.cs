namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class SixHandedFistfightRaidwide(BossModule module) : Components.CastCounter(module, (uint)AID.SixHandedFistfight);
sealed class SixHandedFistfightArenaChange(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SixHandedFistfightVisual2, 12f)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SixHandedFistfightVisual2)
        {
            Arena.Bounds = FTB2DeadStars.FistFightArena;
        }
    }
}

sealed class CollateralColdGasHeatJet(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CollateralColdJet, (uint)AID.CollateralHeatJet, (uint)AID.CollateralGasJet], new AOEShapeCone(40f, 30f.Degrees()), 3);

sealed class CollateralDamage(BossModule module) : Components.GenericStackSpread(module, true)
{
    public int NumCasts;
    private readonly Actor[] allPlayers = AllPlayers(module);

    private static Actor[] AllPlayers(BossModule module)
    {
        List<Actor> actors = new(module.WorldState.Actors.Actors.Values.Count);
        foreach (var a in module.WorldState.Actors.Actors.Values)
            if (a.OID == default)
                actors.Add(a);
        return [.. actors];
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Spreads.Count == 0 && spell.Action.ID == (uint)AID.CollateralDamage)
        {
            var len = allPlayers.Length;
            var act = Module.CastFinishAt(spell, 7.2f);
            Spreads.Capacity = len;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref allPlayers[i];
                Spreads.Add(new(p, 4f, act));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CollateralBioball or (uint)AID.CollateralFireball or (uint)AID.CollateralIceball)
        {
            ++NumCasts;
            var count = Spreads.Count;
            var id = spell.MainTargetID;
            for (var i = 0; i < count; ++i)
            {
                if (Spreads[i].Target.InstanceID == id)
                {
                    Spreads.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
