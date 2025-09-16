namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.Paeonia;

public enum OID : uint
{
    Paeonia = 0x48AE, // R1.8
    PaeoniaSeedling = 0x48AF, // R0.84

    VaultOnion = 0x48B9, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    VaultEggplant = 0x48BA, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    VaultGarlic = 0x48BB, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    VaultTomato = 0x48BC, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    VaultQueen = 0x48BD, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Paeonia/PaeoniaSeedling->player, no cast, single-target
    Teleport = 43790, // Paeonia->location, no cast, single-target
    Visual = 43601, // Paeonia->self, no cast, single-target

    HypnotizeVisual = 43700, // Paeonia->self, 3.0s cast, single-target
    Hypnotize1 = 43701, // Helper->self, 5.0s cast, range 50 circle, gaze
    Hypnotize2 = 43702, // Helper->self, 8.0s cast, range 50 circle

    LightningBoltVisual = 43703, // Paeonia->self, 2.8+0,2s cast, single-target
    LightningBolt = 43704, // Helper->location, 3.0s cast, range 6 circle

    LightningStorm = 43714, // Helper->player, 5.0s cast, range 6 circle, spread

    LightningCrossingVisual = 43705, // Paeonia->self, 4.8+0,2s cast, single-target
    LightningCrossing1 = 43706, // Helper->self, 5.0s cast, range 40 45-degree cone
    LightningCrossing2 = 43707, // Helper->self, 7.0s cast, range 40 45-degree cone

    Mandrastorm = 43715, // Paeonia->self, 5.0s cast, range 60 circle, raidwide
    Ram = 43716, // Paeonia->player, 5.0s cast, single-target, tankbuster
    SaibaiMandragora = 43717, // Paeonia->self, 3.0s cast, single-target

    EpicenterShockVisual = 43708, // Paeonia->self, 4.8+0,2s cast, single-target
    EpicenterShock1 = 43710, // Helper->self, 5.0s cast, range 10 circle
    LevinrootRing2 = 43712, // Helper->self, 7.0s cast, range 10-30 donut
    LevinrootRingVisual = 43711, // Paeonia->self, 4.8+0,2s cast, single-target
    LevinrootRing1 = 43713, // Helper->self, 5.0s cast, range 10-30 donut
    EpicenterShock2 = 43709, // Helper->self, 7.0s cast, range 10 circle

    TearyTwirl = 32301, // VaultOnion->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // VaultEggplant->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // VaultGarlic->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // VaultTomato->self, 3.5s cast, range 7 circle
    Pollen = 32305, // VaultQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class Hypnotize(BossModule module) : Components.CastGazes(module, [(uint)AID.Hypnotize1, (uint)AID.Hypnotize2]);
sealed class LightningStorm(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.LightningStorm, 6f);
sealed class LightningCrossing(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LightningCrossing1, (uint)AID.LightningCrossing2], new AOEShapeCone(40f, 22.5f.Degrees()), 4, 8);
sealed class Mandrastorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.Mandrastorm);
sealed class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, 6f);
sealed class Ram(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Ram);

sealed class EpicenterShock(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EpicenterShock1)
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
                (uint)AID.EpicenterShock1 => 0,
                (uint)AID.LevinrootRing2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class LevinrootRing(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(10f, 30f), new AOEShapeCircle(10f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LevinrootRing1)
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
                (uint)AID.LevinrootRing1 => 0,
                (uint)AID.EpicenterShock2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

sealed class PaeoniaStates : StateMachineBuilder
{
    public PaeoniaStates(Paeonia module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hypnotize>()
            .ActivateOnEnter<LightningStorm>()
            .ActivateOnEnter<LightningCrossing>()
            .ActivateOnEnter<Mandrastorm>()
            .ActivateOnEnter<Ram>()
            .ActivateOnEnter<EpicenterShock>()
            .ActivateOnEnter<LevinrootRing>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(Paeonia.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Paeonia, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 14005u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 5)]
public sealed class Paeonia : SharedBoundsBoss
{
    public Paeonia(WorldState ws, Actor primary) : base(ws, primary)
    {
        seedlings = Enemies((uint)OID.PaeoniaSeedling);
    }

    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultGarlic, (uint)OID.VaultEggplant, (uint)OID.VaultQueen];
    public static readonly uint[] All = [(uint)OID.Paeonia, (uint)OID.PaeoniaSeedling, .. bonusAdds];
    private readonly List<Actor> seedlings;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(seedlings);
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
                (uint)OID.VaultQueen => 2,
                (uint)OID.PaeoniaSeedling => 1,
                _ => 0
            };
        }
    }
}
