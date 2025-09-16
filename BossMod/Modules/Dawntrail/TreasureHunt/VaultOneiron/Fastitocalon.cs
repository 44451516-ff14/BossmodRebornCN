namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.Fastitocalon;

public enum OID : uint
{
    Fastitocalon = 0x48A8, // R5.5
    VaultLaserCannon = 0x48A9, // R2.0

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
    AutoAttack1 = 870, // Fastitocalon->player, no cast, single-target
    AutoAttack2 = 43673, // VaultLaserCannon->player, no cast, single-target
    Visual = 43601, // Fastitocalon->self, no cast, single-target

    TremblorVisual = 43663, // Fastitocalon->self, 3.0+1,0s cast, single-target
    Tremblor1 = 43664, // Helper->self, 4.0s cast, range 10 circle
    Tremblor2 = 43665, // Helper->self, 7.0s cast, range 10-20 donut
    Tremblor3 = 43666, // Helper->self, 10.0s cast, range 20-30 donut

    UpliftVisual = 43667, // Fastitocalon->self, 4.0+1,0s cast, single-target
    Uplift = 43668, // Helper->player, 5.0s cast, range 5 circle, tankbuster
    UpliftSequenceVisual = 43658, // Fastitocalon->self, 4.0+1,0s cast, single-target
    UpliftSequence1 = 43659, // Helper->self, 5.0s cast, range 6 circle
    UpliftSequence2 = 43660, // Helper->self, 7.0s cast, range 6 circle
    UpliftSequence3 = 43661, // Helper->self, 9.0s cast, range 6 circle
    UpliftSequence4 = 43662, // Helper->self, 11.0s cast, range 6 circle

    EarthshakeVisual = 43669, // Fastitocalon->self, 2.5+0,5s cast, single-target
    Earthshake = 43670, // Helper->location, 3.0s cast, range 6 circle
    CanyonVisual = 43671, // Fastitocalon->self, 4.0+1,0s cast, single-target
    Canyon = 43672, // Helper->self, 5.0s cast, range 45 circle

    AutoAttack3 = 871, // Vaultkeeper->player, no cast, single-target
    Thunderlance = 43727, // Vaultkeeper->self, 3.5s cast, range 20 width 3 rect
    LanceSwing = 43726, // Vaultkeeper->self, 4.0s cast, range 8 circle
    TearyTwirl = 32301, // VaultOnion->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // VaultEggplant->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // VaultGarlic->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // VaultTomato->self, 3.5s cast, range 7 circle
    Pollen = 32305, // VaultQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class UpliftSequence : Components.SimpleAOEGroups
{
    public UpliftSequence(BossModule module) : base(module, [(uint)AID.UpliftSequence1, (uint)AID.UpliftSequence2,
    (uint)AID.UpliftSequence3, (uint)AID.UpliftSequence4], 6f, expectedNumCasters: 12, riskyWithSecondsLeft: 3d) // can have 12 or 16 casters
    {
        MaxDangerColor = 4;
    }
}
sealed class Uplift(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Uplift, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class Earthshake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Earthshake, 6f);
sealed class Canyon(BossModule module) : Components.RaidwideCast(module, (uint)AID.Canyon);

sealed class Tremblor(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Tremblor1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.Tremblor1 => 0,
                (uint)AID.Tremblor2 => 1,
                (uint)AID.Tremblor3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(3d));
        }
    }
}

sealed class Thunderlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderlance, new AOEShapeRect(20f, 1.5f));
sealed class LanceSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanceSwing, 8f);
sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

sealed class FastitocalonStates : StateMachineBuilder
{
    public FastitocalonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<UpliftSequence>()
            .ActivateOnEnter<Tremblor>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<Earthshake>()
            .ActivateOnEnter<Canyon>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Thunderlance>()
            .ActivateOnEnter<LanceSwing>()
            .Raw.Update = () => AllDeadOrDestroyed(Fastitocalon.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Fastitocalon, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 13999u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 4)]
public sealed class Fastitocalon : SharedBoundsBoss
{
    public Fastitocalon(WorldState ws, Actor primary) : base(ws, primary)
    {
        lasercannons = Enemies((uint)OID.VaultLaserCannon);
    }

    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultGarlic, (uint)OID.VaultEggplant, (uint)OID.VaultQueen, (uint)OID.Vaultkeeper, (uint)OID.GoldyCat];
    public static readonly uint[] All = [(uint)OID.Fastitocalon, (uint)OID.VaultLaserCannon, .. bonusAdds];
    private readonly List<Actor> lasercannons;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(lasercannons);
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
                (uint)OID.VaultOnion => 7,
                (uint)OID.VaultEggplant => 6,
                (uint)OID.VaultGarlic => 5,
                (uint)OID.VaultTomato => 4,
                (uint)OID.VaultQueen or (uint)OID.GoldyCat => 3,
                (uint)OID.Vaultkeeper => 2,
                (uint)OID.VaultLaserCannon => 1,
                _ => 0
            };
        }
    }
}
