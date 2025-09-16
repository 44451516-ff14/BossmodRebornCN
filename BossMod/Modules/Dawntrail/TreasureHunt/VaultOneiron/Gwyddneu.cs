namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.Gwyddneu;

public enum OID : uint
{
    Gwyddneu = 0x48A4, // R5.0
    VaultLeptocyon = 0x48A5, // R3.15
    BallLightning1 = 0x48A7, // R3.5
    BallLightning2 = 0x48A6, // R1.5

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
    AutoAttack = 872, // Gwyddneu/VaultLeptocyon->player, no cast, single-target
    Teleport = 44877, // Gwyddneu->location, no cast, single-target

    VioletBoltVisual = 43642, // Gwyddneu->self, 4.3+0,7s cast, single-target
    VioletBoltStartDouble = 43601, // Gwyddneu->self, no cast, single-target
    VioletBoltVisualSingle = 43643, // Helper->self, 5.0s cast, single-target
    VioletBoltVisualDouble = 43644, // Helper->self, 5.0s cast, single-target
    VioletBolt = 43645, // Helper->self, 5.0s cast, range 70 45-degree cone

    CracklingHowlVisual = 43647, // Gwyddneu->self, 4.3+0,7s cast, single-target
    CracklingHowl = 43648, // Helper->self, 5.0s cast, range 45 circle, raidwide
    LightningBoltVisual = 43651, // Gwyddneu->self, 2.3+0,7s cast, single-target
    LightningBolt = 43652, // Helper->location, 3.0s cast, range 6 circle

    Gnaw = 43657, // VaultLeptocyon->player, no cast, single-target
    FulgurousHowlVisual = 43653, // Gwyddneu->self, 4.3+0,7s cast, single-target
    FulgurousHowl = 43654, // Helper->location, 5.0s cast, range 35 circle, raidwide
    Electrify = 43656, // BallLightning1->self, 3.0s cast, range 16 circle
    Shock = 43655, // BallLightning2->self, 3.0s cast, range 8 circle
    ThundercrackVisual = 43649, // Gwyddneu->self, 4.3+0,7s cast, single-target
    Thundercrack = 43650, // Helper->player, 5.0s cast, range 5 circle, tankbuster

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

sealed class VioletBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VioletBolt, new AOEShapeCone(70f, 22.5f.Degrees()), 4);
sealed class CracklingHowlFulgurousHowl(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.CracklingHowl, (uint)AID.FulgurousHowl]);
sealed class Thundercrack(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Thundercrack, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, 6f);
sealed class ShockElectrify(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private readonly AOEShapeCircle circleSmall = new(8f), circleBig = new(16f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        var shape = actor.OID switch
        {
            (uint)OID.BallLightning1 => circleBig,
            (uint)OID.BallLightning2 => circleSmall,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, actor.Position.Quantized(), default, WorldState.FutureTime(7.9d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Electrify) // always 2 big and 3 small orbs
        {
            _aoes.Clear();
        }
    }
}

sealed class Thunderlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderlance, new AOEShapeRect(20f, 1.5f));
sealed class LanceSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanceSwing, 8f);
sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

sealed class GwyddneuStates : StateMachineBuilder
{
    public GwyddneuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VioletBolt>()
            .ActivateOnEnter<CracklingHowlFulgurousHowl>()
            .ActivateOnEnter<Thundercrack>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<ShockElectrify>()
            .ActivateOnEnter<Thunderlance>()
            .ActivateOnEnter<LanceSwing>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(Gwyddneu.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Gwyddneu, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 13996u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 11)]
public sealed class Gwyddneu : SharedBoundsBoss
{
    public Gwyddneu(WorldState ws, Actor primary) : base(ws, primary)
    {
        leptocyons = Enemies((uint)OID.VaultLeptocyon);
    }

    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultGarlic, (uint)OID.VaultEggplant, (uint)OID.VaultQueen, (uint)OID.Vaultkeeper, (uint)OID.GoldyCat];
    public static readonly uint[] All = [(uint)OID.Gwyddneu, (uint)OID.VaultLeptocyon, .. bonusAdds];
    private readonly List<Actor> leptocyons;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(leptocyons);
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
