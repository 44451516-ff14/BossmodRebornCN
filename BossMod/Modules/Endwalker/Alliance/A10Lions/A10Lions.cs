namespace BossMod.Endwalker.Alliance.A10Lions;

class DoubleImmolation(BossModule module) : Components.RaidwideCast(module, (uint)AID.DoubleImmolation);

class A10LionsStates : StateMachineBuilder
{
    public A10LionsStates(A10Lions module) : base(module)
    {
        SimplePhase(0, id => SimpleState(id, 600, "???"), "Single phase")
            .ActivateOnEnter<DoubleImmolation>()
            .ActivateOnEnter<SlashAndBurn>()
            .ActivateOnEnter<RoaringBlaze>()
            .Raw.Update = () => (module.Lion()?.IsDeadOrDestroyed ?? true) && (module.Lioness()?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Lion, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11294, SortOrder = 4)]
public class A10Lions(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-677.256f, -606.256f), 24.5f * CosPI.Pi148th, 148)], [new Rectangle(new(-677f, -581f), 20f, 1.5f),
    new Rectangle(new(-677f, -631f), 20f, 1f)]);

    private Actor? _lioness;

    public Actor? Lion() => PrimaryActor;
    public Actor? Lioness() => _lioness;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _lioness ??= Enemies((uint)OID.Lioness)[0];
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_lioness);
    }
}
