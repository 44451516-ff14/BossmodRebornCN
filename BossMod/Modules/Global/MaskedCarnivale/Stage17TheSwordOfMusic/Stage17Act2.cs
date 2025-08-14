namespace BossMod.Global.MaskedCarnivale.Stage17.Act2;

public enum OID : uint
{
    Boss = 0x2721, // R=2.5
    LeftClaw = 0x2722, //R=2.0
    RightClaw = 0x2723, //R=2.0
    MagitekRayVoidzone = 0x1E8D9B //R=0.5
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    GrandStrike = 15047, // Boss->self, 1.5s cast, range 75+R width 2 rect
    MagitekField = 15049, // Boss->self, 5.0s cast, single-target, buffs defenses, interruptible
    AutoAttack2 = 6499, // RightClaw/LeftClaw->player, no cast, single-target
    TheHand = 14760, // LeftClaw/RightClaw->self, 3.0s cast, range 6+R 120-degree cone
    Shred = 14759, // RightClaw/LeftClaw->self, 2.5s cast, range 4+R width 4 rect
    MagitekRay = 15048 // Boss->location, 3.0s cast, range 6 circle, voidzone, interruptible
}

sealed class GrandStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandStrike, new AOEShapeRect(77.5f, 2f));
sealed class MagitekField(BossModule module) : Components.CastInterruptHint(module, (uint)AID.MagitekField);
sealed class MagitekRay(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.MagitekRay, GetVoidzones, 1.1f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.MagitekRayVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}
sealed class TheHand(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheHand, new AOEShapeCone(8f, 60f.Degrees()));
sealed class Shred(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shred, new AOEShapeRect(6f, 2f));

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var clawsL = Module.Enemies((uint)OID.LeftClaw);
        var countL = clawsL.Count;
        if (countL != 0)
        {
            for (var i = 0; i < countL; ++i)
            {
                var clawL = clawsL[i];
                if (!clawL.IsDead)
                {
                    hints.Add($"{clawL.Name} counters magical damage!");
                    break;
                }
            }
        }
        var clawsR = Module.Enemies((uint)OID.RightClaw);
        var countR = clawsR.Count;
        if (countR != 0)
        {
            for (var i = 0; i < countR; ++i)
            {
                var clawR = clawsR[i];
                if (!clawR.IsDead)
                {
                    hints.Add($"{clawR.Name} counters physical damage!");
                    return;
                }
            }
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} is weak to lightning spells.\nDuring the fight he will spawn one of each claws as known from act 1.\nIf available use the Ram's Voice + Ultravibration combo for instant kill.");
    }
}

sealed class Stage17Act2States : StateMachineBuilder
{
    public Stage17Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekField>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<TheHand>()
            .ActivateOnEnter<GrandStrike>()
            .ActivateOnEnter<Shred>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 627, NameID = 8087, SortOrder = 2)]
public sealed class Stage17Act2 : BossModule
{
    public Stage17Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LeftClaw), Colors.Object);
        Arena.Actors(Enemies((uint)OID.RightClaw), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.LeftClaw or (uint)OID.RightClaw => 1, //TODO: ideally left claw should only be attacked with magical abilities and right claw should only be attacked with physical abilities
                _ => 0
            };
        }
    }
}
