namespace BossMod.Endwalker.Alliance.A10Lions;

class DoubleImmolation(BossModule module) : Components.RaidwideCast(module, (uint)AID.DoubleImmolation);

class A10LionsStates : StateMachineBuilder
{
    public A10LionsStates(A10Lions module) : base(module)
    {
        SimplePhase(default, id => SimpleState(id, 600f, "???"), "Single phase")
            .ActivateOnEnter<DoubleImmolation>()
            .ActivateOnEnter<SlashAndBurn>()
            .ActivateOnEnter<RoaringBlaze>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && (module.Lioness?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Lion, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866u, NameID = 11294u, SortOrder = 4)]
public class A10Lions(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-677.256f, -606.256f), 24.5f * CosPI.Pi148th, 148)], [new Rectangle(new(-677f, -581f), 20f, 1.5f),
    new Rectangle(new(-677f, -631f), 20f, 1f)]);
    public Actor? Lioness;

    protected override void UpdateModule()
    {
        Lioness ??= GetActor((uint)OID.Lioness);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(Lioness);
    }
}
