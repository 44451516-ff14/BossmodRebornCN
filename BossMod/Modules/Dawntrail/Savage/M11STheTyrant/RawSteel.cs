namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class RawSteelTrophyAxe(BossModule module) : Components.GenericStackSpread(module, false)
{
    public int NumCasts;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RawSteelTrophy1)
        {
            NumCasts++;
            var activation = Module.CastFinishAt(spell, 0.1d);
            var party = Module.WorldState.Party.WithoutSlot();
            var len = party.Length;
            var tankAssigned = false;

            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p.IsDead)
                    continue;

                if (p.Role == Role.Tank)
                {
                    if (!tankAssigned)
                    {
                        Stacks.Add(new(p, 6f, 2, 2, activation));
                        tankAssigned = true;
                    }
                    continue;
                }
                Spreads.Add(new(p, 6f, activation));
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RawSteel4 or (uint)AID.Impact)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }
}
sealed class RawSteelTrophyScythe(BossModule module) : Components.GenericBaitAway(module, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private readonly AOEShapeCone tankCone = new(60f, 45.Degrees());
    private readonly AOEShapeCone healerCone = new(60f, 15.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RawSteelTrophy)
        {
            var act = Module.CastFinishAt(spell, 0.1d);
            var party = Module.WorldState.Party.WithoutSlot();
            var len = party.Length;
            var maxhealers = 0;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p.IsDead)
                    continue;

                if (p.Role == Role.Tank)
                    CurrentBaits.Add(new(caster, p, tankCone, act));
                else if (p.Role == Role.Healer && maxhealers < 1)
                {
                    CurrentBaits.Add(new(caster, p, healerCone, act));
                    maxhealers++;
                }
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RawSteel2 or (uint)AID.HeavyHitter)
        {
            CurrentBaits.Clear();
        }
    }
}