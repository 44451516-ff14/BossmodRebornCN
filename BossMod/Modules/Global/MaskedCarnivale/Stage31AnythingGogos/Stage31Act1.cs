namespace BossMod.Global.MaskedCarnivale.Stage31.Act1;

public enum OID : uint
{
    Boss = 0x30F5, //R=2.0
    Imitation = 0x30F6, // R=2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    Mimic = 23097, // Boss->self, 5.0s cast, single-target, stop everything that does dmg
    MimickedFlameThrower = 23116, // Boss->self, no cast, range 8 90-degree cone, unavoidable
    MimickedSap0 = 23104, // Boss->self, 3.5s cast, single-target
    MimickedSap1 = 23101, // Helper->location, 4.0s cast, range 8 circle
    MimickedSap2 = 23105, // Boss->self, 1.5s cast, single-target
    MimickedSap3 = 23106, // Helper->location, 2.0s cast, range 8 circle
    MimickedDoomImpending = 23113, // Boss->self, 8.0s cast, range 80 circle, applies doom
    MimickedBunshin = 23107, // Boss->self, 3.0s cast, single-target, summons Imitation
    MimickedFireBlast = 23109, // Boss->self, 2.0s cast, single-target
    MimickedFireBlast2 = 23110, // Helper->self, 2.0s cast, range 70+R width 4 rect
    MimickedProteanWave = 23111, // Imitation->self, 2.0s cast, single-target
    MimickedProteanWave2 = 23112, // Helper->self, 2.0s cast, range 50 30-degree cone
    MimickedRawInstinct = 23115, // Boss->self, 3.0s cast, single-target, buffs self with critical strikes, can be removed
    MimickedImpSong = 23114, // Boss->self, 6.0s cast, range 40 circle, interruptible, turns player into imp
    MimickedFlare = 23098, // Boss->player, 3.0s cast, range 80 circle, possible punishment for ignoring Mimic
    MimickedHoly = 23100, // Boss->player, 3.0s cast, range 6 circle, possible punishment for ignoring Mimic
    MimickedPowerfulHit = 23103, // Boss->player, 3.0s cast, single-target, possible punishment for ignoring Mimic
    MimickedCriticalHit = 23102 // Boss->player, 3.0s cast, single-target, possible punishment for ignoring Mimic
}

public enum SID : uint
{
    Mimicry = 2450, // none->Boss, extra=0x0
    CriticalStrikes = 1797 // Boss->Boss, extra=0x0
}

sealed class Mimic(BossModule module) : Components.CastHint(module, (uint)AID.Mimic, "Stop attacking when cast ends");
sealed class MimickedSap(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MimickedSap1, (uint)AID.MimickedSap2], 8f);
sealed class MimickedDoomImpending(BossModule module) : Components.CastHint(module, (uint)AID.MimickedDoomImpending, "Heal to full before cast ends!");
sealed class MimickedProteanWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MimickedProteanWave2, new AOEShapeCone(50f, 15f.Degrees()));
sealed class MimickedFireBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MimickedFireBlast2, new AOEShapeRect(70.5f, 2f));
sealed class MimickedImpSong(BossModule module) : Components.CastInterruptHint(module, (uint)AID.MimickedImpSong);
sealed class MimickedRawInstinct(BossModule module) : Components.CastHint(module, (uint)AID.MimickedRawInstinct, "Applies buff, dispel it");

sealed class DiamondBackHint(BossModule module) : Components.CastHints(module, [(uint)AID.MimickedFlare, (uint)AID.MimickedHoly, (uint)AID.MimickedCriticalHit, (uint)AID.MimickedPowerfulHit], "Use Diamondback!");

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.FindStatus((uint)SID.Mimicry) != null)
        {
            hints.Add($"Do no damage!");
        }
        if (Module.PrimaryActor.FindStatus((uint)SID.CriticalStrikes) != null)
        {
            hints.Add("Dispel buff!");
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"For this fight Diamondback, Exuviation, Flying Sardine and a healing\nability (preferably Pom Cure with healer mimicry) are mandatory.\nEerie Soundwave is also recommended.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirements for achievement: Take no optional damage and finish faster\nthan ideal time.", false);
    }
}

sealed class Stage31Act1States : StateMachineBuilder
{
    public Stage31Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Mimic>()
            .ActivateOnEnter<MimickedSap>()
            .ActivateOnEnter<MimickedDoomImpending>()
            .ActivateOnEnter<MimickedProteanWave>()
            .ActivateOnEnter<MimickedFireBlast>()
            .ActivateOnEnter<MimickedImpSong>()
            .ActivateOnEnter<MimickedFireBlast>()
            .ActivateOnEnter<MimickedRawInstinct>()
            .ActivateOnEnter<DiamondBackHint>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 754, NameID = 9908, SortOrder = 1)]
public sealed class Stage31Act1 : BossModule
{
    public Stage31Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }
}
