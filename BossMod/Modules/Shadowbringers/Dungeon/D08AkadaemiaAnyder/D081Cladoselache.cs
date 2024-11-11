namespace BossMod.Shadowbringers.Dungeon.D08AkadaemiaAnyder.D081Cladoselache;

public enum OID : uint
{
    Boss = 0x27AB, // R2.47
    Doliodus = 0x27AC, // R2.47
    Voidzone = 0x1E909F,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Doliodus->player, no cast, single-target

    ProtolithicPuncture = 15876, // Boss/Doliodus->player, 4.0s cast, single-target
    PelagicCleaver1 = 15881, // Doliodus->self, 3.5s cast, range 40 60-degree cone
    PelagicCleaver2 = 15883, // Doliodus->location, 8.0s cast, range 50 60-degree cone
    TidalGuillotine1 = 15880, // Boss->self, 4.0s cast, range 13 circle
    TidalGuillotine2 = 15882, // Boss->location, 8.0s cast, range 13 circle
    AquaticLance = 15877, // Boss/Doliodus->player, 4.0s cast, range 8 circle, spread + voidzone
    MarineMayhem = 15878, // Doliodus/Boss->self, 3.5s cast, range 40 circle
    MarineMayhemRepeat = 16241, // Boss/Doliodus->self, no cast, range 40 circle
    CarcharianVerve = 15879 // Boss/Doliodus->self, 2.0s cast, single-target, damage up after partner dies
}

class MarineMayhem(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MarineMayhem));
class ProtolithicPuncture(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.ProtolithicPuncture));
class PelagicCleaver1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PelagicCleaver1), new AOEShapeCone(40, 30.Degrees()));
class PelagicCleaver2(BossModule module) : Components.LocationTargetedAOEsOther(module, ActionID.MakeSpell(AID.PelagicCleaver2), new AOEShapeCone(50, 30.Degrees()));
class TidalGuillotine1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TidalGuillotine1), new AOEShapeCircle(13));
class TidalGuillotine2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.TidalGuillotine2), 13);
class AquaticLance(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.AquaticLance), 8);
class AquaticLanceVoidzone(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.Voidzone).Where(x => x.EventState != 7));

class D081CladoselacheStates : StateMachineBuilder
{
    public D081CladoselacheStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MarineMayhem>()
            .ActivateOnEnter<ProtolithicPuncture>()
            .ActivateOnEnter<PelagicCleaver1>()
            .ActivateOnEnter<PelagicCleaver2>()
            .ActivateOnEnter<TidalGuillotine1>()
            .ActivateOnEnter<TidalGuillotine2>()
            .ActivateOnEnter<AquaticLance>()
            .ActivateOnEnter<AquaticLanceVoidzone>()
            .Raw.Update = () => module.Enemies(OID.Doliodus).Concat([module.PrimaryActor]).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 661, NameID = 8235)]
public class D081Cladoselache(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-305, 211.5f), 19.5f / MathF.Cos(MathF.PI / 60), 60)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Doliodus).Concat([PrimaryActor]));
    }
}
