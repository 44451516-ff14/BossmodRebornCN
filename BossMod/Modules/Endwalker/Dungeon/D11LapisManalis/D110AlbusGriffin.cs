namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D110AlbusGriffin;

public enum OID : uint
{
    CaladriusMaturus = 0x3D56, //R=3.96
    Caladrius = 0x3CE2, //R=1.8
    AlbusGriffin = 0x3E9F, //R=4.6
}

public enum AID : uint
{
    AutoAttack1 = 872, // Caladrius/CaladriusMaturus->player, no cast, single-target
    AutoAttack2 = 870, // AlbusGriffin->player, no cast, single-target

    TransonicBlast = 32535, // Caladrius->self, 4.0s cast, range 9 90-degree cone
    WindsOfWinter = 32785, // AlbusGriffin->self, 5.0s cast, range 40 circle
    Freefall = 32786, // AlbusGriffin->location, 3.5s cast, range 8 circle
    GoldenTalons = 32787, // AlbusGriffin->self, 4.5s cast, range 8 90-degree cone
}

sealed class TransonicBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TransonicBlast, new AOEShapeCone(9f, 45f.Degrees()));
sealed class WindsOfWinter(BossModule module) : Components.RaidwideCast(module, (uint)AID.WindsOfWinter);
sealed class WindsOfWinterStunHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.WindsOfWinter, false, true);
sealed class GoldenTalons(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldenTalons, new AOEShapeCone(8f, 45f.Degrees()));
sealed class Freefall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Freefall, 8f);

sealed class D110AlbusGriffinStates : StateMachineBuilder
{
    public D110AlbusGriffinStates(D110AlbusGriffin module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TransonicBlast>()
            .Raw.Update = () => AllDeadOrDestroyed(D110AlbusGriffin.TrashP1);
        TrivialPhase(1u)
            .ActivateOnEnter<Freefall>()
            .ActivateOnEnter<WindsOfWinter>()
            .ActivateOnEnter<WindsOfWinterStunHint>()
            .ActivateOnEnter<GoldenTalons>()
            .Raw.Update = () => AllDestroyed(D110AlbusGriffin.TrashP1) && (module.BossAlbusGriffin?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.CaladriusMaturus, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896u, NameID = 12245u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.Endwalker, SortOrder = 1)]
public sealed class D110AlbusGriffin(WorldState ws, Actor primary) : BossModule(ws, primary, new(47f, -570.5f), new ArenaBoundsRect(8.5f, 11.5f))
{
    public static readonly uint[] TrashP1 = [(uint)OID.CaladriusMaturus, (uint)OID.Caladrius];
    public Actor? BossAlbusGriffin;

    protected override void UpdateModule()
    {
        BossAlbusGriffin ??= GetActor((uint)OID.AlbusGriffin);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Caladrius));
        Arena.Actors(Enemies((uint)OID.AlbusGriffin));
    }
}
