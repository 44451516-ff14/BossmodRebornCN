namespace BossMod.RealmReborn.Extreme.Ex3Titan;

sealed class MountainBuster : Components.Cleave
{
    public MountainBuster(BossModule module) : base(module, (uint)AID.MountainBuster, new AOEShapeCone(21.25f, 60f.Degrees())) // TODO: verify angle
    {
        NextExpected = module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Tankbuster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var boss = hints.PotentialTargets.Find(e => e.Actor.OID == (uint)OID.Boss);
        boss?.AttackStrength += 0.25f;
    }
}

sealed class RockBuster : Components.Cleave
{
    public RockBuster(BossModule module) : base(module, (uint)AID.RockBuster, new AOEShapeCone(11.25f, 60f.Degrees())) // TODO: verify angle
    {
        NextExpected = module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Tankbuster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var boss = hints.PotentialTargets.Find(e => e.Actor.OID == (uint)OID.TitansHeart);
        boss?.AttackStrength += 0.25f;
    }
}
