namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.VaultRoadripper;

public enum OID : uint
{
    VaultBot = 0x48A1, // R2.0
    VaultAerostat = 0x48A2, // R2.76
    VaultRoadripper = 0x48A3, // R2.56

    VaultOnion = 0x48B9, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    VaultEggplant = 0x48BA, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    VaultGarlic = 0x48BB, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    VaultTomato = 0x48BC, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    VaultQueen = 0x48BD, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GoldyCat = 0x48B7, // R1.87
    Vaultkeeper = 0x48B8 // R2.0
}

public enum AID : uint
{
    AutoAttack1 = 43634, // VaultBot->player, no cast, single-target
    AutoAttack2 = 872, // VaultAerostat/VaultRoadripper->player, no cast, single-target

    HomingShot = 43635, // VaultBot->location, 3.0s cast, range 6 circle
    ThrownFlames = 43636, // VaultAerostat->self, 3.0s cast, range 8 circle
    IncendiaryCircle = 43637, // VaultAerostat->self, 3.0s cast, range 3-12 donut
    RunAmok = 43640, // VaultRoadripper->player, 4.0s cast, width 8 rect charge
    WheelingShot = 43641, // VaultRoadripper->self, 5.0s cast, range 40 180-degree cone
    Electroflame = 43639, // VaultRoadripper->self, 5.0s cast, range 40 circle
    Wheel = 43638, // VaultRoadripper->player, 5.0s cast, single-target

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

sealed class HomingShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HomingShot, 6f);
sealed class ThrownFlames(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThrownFlames, 8f);
sealed class IncendiaryCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncendiaryCircle, new AOEShapeDonut(3f, 12f));
sealed class RunAmok(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.RunAmok, 4f);
sealed class Wheel(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Wheel);
sealed class WheelingShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WheelingShot, new AOEShapeCone(40f, 90f.Degrees()));
sealed class Electroflame(BossModule module) : Components.RaidwideCast(module, (uint)AID.Electroflame);

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);
sealed class Thunderlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderlance, new AOEShapeRect(20f, 1.5f));
sealed class LanceSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanceSwing, 8f);

sealed class VaultRoadripperStates : StateMachineBuilder
{
    public VaultRoadripperStates(VaultRoadripper module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HomingShot>()
            .ActivateOnEnter<ThrownFlames>()
            .ActivateOnEnter<IncendiaryCircle>()
            .ActivateOnEnter<RunAmok>()
            .ActivateOnEnter<Wheel>()
            .ActivateOnEnter<WheelingShot>()
            .ActivateOnEnter<Electroflame>()
            .ActivateOnEnter<Thunderlance>()
            .ActivateOnEnter<LanceSwing>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDestroyed(VaultRoadripper.All) && (module.BossRoadripper?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.VaultBot, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 13995u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 3)]
public sealed class VaultRoadripper(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultGarlic, (uint)OID.VaultEggplant, (uint)OID.VaultQueen, (uint)OID.GoldyCat, (uint)OID.Vaultkeeper];
    private static readonly uint[] trash = [(uint)OID.VaultBot, (uint)OID.VaultAerostat];
    public static readonly uint[] All = [.. trash, .. bonusAdds];

    public Actor? BossRoadripper;

    protected override void UpdateModule()
    {
        BossRoadripper ??= GetActor((uint)OID.VaultRoadripper);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossRoadripper);
        var m = this;
        Arena.Actors(m, trash);
        Arena.Actors(m, bonusAdds, Colors.Vulnerable);
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
