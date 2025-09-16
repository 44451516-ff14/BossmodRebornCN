namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.SpiritOfThunder;

public enum OID : uint
{
    SpiritOfThunder = 0x48B5, // R3.5
    PortalOfLevin = 0x48B6, // R1.0

    VaultOnion = 0x48B9, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    VaultEggplant = 0x48BA, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    VaultGarlic = 0x48BB, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    VaultTomato = 0x48BC, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    VaultQueen = 0x48BD, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GoldyCat = 0x48B7, // R1.87
    Vaultkeeper = 0x48B8, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 872, // SpiritOfThunder->player, no cast, single-target

    ElectrayVisual = 43772, // SpiritOfThunder->self, 5.0s cast, single-target
    Electray1 = 43773, // Helper->self, 5.0s cast, range 20 width 10 rect
    Electray2 = 43774, // Helper->self, 8.0s cast, range 20 width 10 rect

    TidalThunderVisual = 43775, // SpiritOfThunder->self, 2.3+0,7s cast, single-target
    TidalThunder = 43776, // Helper->player, 3.0s cast, range 30 30-degree 

    HighVoltageElectrayStart = 43601, // SpiritOfThunder->self, no cast, single-target
    HighVoltageElectrayVisual = 43777, // SpiritOfThunder->self, 5.0s cast, single-target
    HighVoltageElectray1 = 43778, // Helper->self, 5.0s cast, range 20 width 20 rect
    HighVoltageElectray2 = 43779, // Helper->self, 6.0s cast, range 20 width 20 rect
    HighVoltageElectray3 = 43780, // Helper->self, 7.0s cast, range 20 width 20 rect
    HighVoltageElectray4 = 43781, // Helper->self, 8.0s cast, range 20 width 20 rect

    ArcaneRevelation = 43787, // SpiritOfThunder->self, 3.0s cast, single-target
    PowerLine = 43788, // PortalOfLevin->self, 1.0s cast, range 40 width 10 rect
    ElectrowaveVisual = 43785, // SpiritOfThunder->self, 5.0s cast, single-target
    Electrowave = 43786, // Helper->self, 5.0s cast, range 40 circle
    ElectricVortex = 43782, // Helper->player, 5.0s cast, range 5 circle
    HypercondensedThunderVisual = 43783, // SpiritOfThunder->self, 4.3+0,7s cast, single-target
    HypercondensedThunder = 43784, // Helper->player, 5.0s cast, range 6 circle, spread

    AutoAttack2 = 871, // Vaultkeeper->player, no cast, single-target
    Thunderlance = 43727, // Vaultkeeper->self, 3.5s cast, range 20 width 3 rect
    LanceSwing = 43726, // Vaultkeeper->self, 4.0s cast, range 8 circle
    TearyTwirl = 32301, // VaultOnion->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // VaultEggplant->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // VaultGarlic->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // VaultTomato->self, 3.5s cast, range 7 circle
    Pollen = 32305, // VaultQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class Electray(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Electray1, (uint)AID.Electray2], new AOEShapeRect(20f, 5f), 4, 8);
sealed class TidalThunder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TidalThunder, new AOEShapeCone(30f, 15f.Degrees()));
sealed class Electrowave(BossModule module) : Components.RaidwideCast(module, (uint)AID.Electrowave);
sealed class ElectricVortex(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ElectricVortex, 5f);
sealed class HypercondensedThunder(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HypercondensedThunder, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class HighVoltageElectray : Components.SimpleAOEGroups
{
    public HighVoltageElectray(BossModule module) : base(module, [(uint)AID.HighVoltageElectray1, (uint)AID.HighVoltageElectray2,
    (uint)AID.HighVoltageElectray3, (uint)AID.HighVoltageElectray4], new AOEShapeRect(20f, 10f), 3, 4)
    {
        MaxDangerColor = 1;
    }
}

sealed class PowerLine(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private readonly AOEShapeRect rect = new(40f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.PortalOfLevin)
        {
            _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(7.7d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PowerLine)
        {
            _aoes.Clear();
        }
    }
}

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);
sealed class Thunderlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderlance, new AOEShapeRect(20f, 1.5f));
sealed class LanceSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanceSwing, 8f);

sealed class SpiritOfThunderStates : StateMachineBuilder
{
    public SpiritOfThunderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<HighVoltageElectray>()
            .ActivateOnEnter<TidalThunder>()
            .ActivateOnEnter<PowerLine>()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<ElectricVortex>()
            .ActivateOnEnter<HypercondensedThunder>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Thunderlance>()
            .ActivateOnEnter<LanceSwing>()
            .Raw.Update = () => AllDeadOrDestroyed(SpiritOfThunder.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.SpiritOfThunder, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 14012u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 12)]
public sealed class SpiritOfThunder(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultGarlic, (uint)OID.VaultEggplant, (uint)OID.VaultQueen, (uint)OID.Vaultkeeper, (uint)OID.GoldyCat];
    public static readonly uint[] All = [(uint)OID.SpiritOfThunder, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.VaultOnion => 6,
                (uint)OID.VaultEggplant => 5,
                (uint)OID.VaultGarlic => 4,
                (uint)OID.VaultTomato => 3,
                (uint)OID.VaultQueen or (uint)OID.GoldyCat => 2,
                (uint)OID.Vaultkeeper => 1,
                _ => 0
            };
        }
    }
}
