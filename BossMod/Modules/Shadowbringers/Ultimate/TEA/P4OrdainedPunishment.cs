namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P4OrdainedCapitalPunishment(BossModule module) : Components.GenericSharedTankbuster(module, (uint)AID.OrdainedCapitalPunishmentAOE, 4f)
{
    public override void Update()
    {
        Target = Source != null ? WorldState.Actors.Find(Source.TargetID) : null;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OrdainedCapitalPunishment)
        {
            Source = caster;
            Activation = Module.CastFinishAt(spell, 3.1d);
        }
    }
}

// TODO: dedicated tankbuster component with tankswap hint
[SkipLocalsInit]
sealed class P4OrdainedPunishment(BossModule module) : Components.BaitAwayCast(module, (uint)AID.OrdainedPunishment, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
