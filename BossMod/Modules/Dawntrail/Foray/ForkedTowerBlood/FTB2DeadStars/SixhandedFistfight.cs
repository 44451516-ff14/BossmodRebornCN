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

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Spreads.Count == 0 && spell.Action.ID == (uint)AID.CollateralDamage)
        {
            List<Actor> players = new(Module.WorldState.Actors.Actors.Values.Count);
            foreach (var a in Module.WorldState.Actors.Actors.Values)
            {
                if (a.OID == default)
                {
                    players.Add(a);
                }
            }
            var count = players.Count;
            var act = Module.CastFinishAt(spell, 7.2d);
            Spreads.Capacity = count;
            for (var i = 0; i < count; ++i)
            {
                Spreads.Add(new(players[i], 4f, act));
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
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                ref var spread = ref spreads[i];
                if (spread.Target.InstanceID == id)
                {
                    Spreads.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
