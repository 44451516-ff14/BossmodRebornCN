namespace BossMod.Endwalker.Alliance.A14Naldthal;

class HeavensTrialCone(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCone _shape = new(60f, 15f.Degrees());

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HeavensTrialConeStart:
                var target = WorldState.Actors.Find(spell.MainTargetID);
                if (target != null)
                    CurrentBaits.Add(new(caster, target, _shape));
                break;
            case (uint)AID.HeavensTrialSmelting:
                CurrentBaits.Clear();
                ++NumCasts;
                break;
        }
    }
}

class HeavensTrialStack(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HeavensTrialAOE, 6f, 8, 8);
