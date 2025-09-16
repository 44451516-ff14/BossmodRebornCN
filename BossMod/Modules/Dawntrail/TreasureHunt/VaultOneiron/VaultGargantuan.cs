namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.VaultGargantuan;

public enum OID : uint
{
    VaultCrab = 0x4898, // R1.4
    VaultScorpion = 0x4899, // R2.4
    VaultGargantuan = 0x489A, // R2.28

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
    AutoAttack1 = 872, // VaultCrab/VaultScorpion/VaultGargantuan->player, no cast, single-target

    SmallClaw = 43611, // VaultCrab->player, no cast, single-target
    MoltenSilk1 = 43612, // VaultScorpion->self, 3.0s cast, range 9 270-degree cone
    MoltenSilk2 = 43613, // VaultScorpion->self, 3.0s cast, range 9 270-degree cone
    FlyingPress = 43614, // VaultScorpion->location, 3.0s cast, range 6 circle
    Scoop = 43618, // VaultGargantuan->self, 4.0s cast, range 13 120-degree cone
    Brushfire = 43616, // VaultGargantuan->self, 5.0s cast, range 60 circle
    Inhale = 43617, // VaultGargantuan->self, 0.5s cast, range 20 120-degree cone, pull 25 between hitboxes
    Spin = 43619, // VaultGargantuan->self, 3.5s cast, range 11 circle
    Mash = 43615, // VaultGargantuan->player, 5.0s cast, single-target, tankbuster

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

sealed class MoltenSilk(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MoltenSilk1, (uint)AID.MoltenSilk2], new AOEShapeCone(9f, 135f.Degrees()));
sealed class FlyingPress(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlyingPress, 6f);
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));
sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Mash(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Mash);
sealed class Brushfire(BossModule module) : Components.RaidwideCast(module, (uint)AID.Brushfire);

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);
sealed class Thunderlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderlance, new AOEShapeRect(20f, 1.5f));
sealed class LanceSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanceSwing, 8f);

sealed class VaultGargantuanStates : StateMachineBuilder
{
    public VaultGargantuanStates(VaultGargantuan module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MoltenSilk>()
            .ActivateOnEnter<FlyingPress>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<Brushfire>()
            .ActivateOnEnter<Thunderlance>()
            .ActivateOnEnter<LanceSwing>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDestroyed(VaultGargantuan.All) && (module.BossGargantuan?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.VaultCrab, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 13986u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 1)]
public sealed class VaultGargantuan(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultGarlic, (uint)OID.VaultEggplant, (uint)OID.VaultQueen, (uint)OID.GoldyCat, (uint)OID.Vaultkeeper];
    private static readonly uint[] trash = [(uint)OID.VaultCrab, (uint)OID.VaultScorpion];
    public static readonly uint[] All = [.. trash, .. bonusAdds];

    public Actor? BossGargantuan;

    protected override void UpdateModule()
    {
        BossGargantuan ??= GetActor((uint)OID.VaultGargantuan);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossGargantuan);
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
