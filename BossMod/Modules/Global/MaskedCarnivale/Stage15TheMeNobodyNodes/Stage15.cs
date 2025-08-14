namespace BossMod.Global.MaskedCarnivale.Stage15;

public enum OID : uint
{
    Boss = 0x26F9, // R=2.3
    Shabti = 0x26FA, //R=1.1
    Serpent = 0x26FB, //R=1.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Shabti/Serpent->player, no cast, single-target

    HighVoltage = 14890, // Boss->self, 7.0s cast, range 50+R circle, paralysis + summon add
    Summon = 14897, // Boss->self, no cast, range 50 circle

    BallastVisual1 = 14893, // Boss->self, 1.0s cast, single-target
    BallastVisual2 = 14955, // Boss->self, no cast, single-target
    Ballast1 = 14894, // Helper->self, 3.0s cast, range 5+R 270-degree cone, knockback dist 15
    Ballast2 = 14895, // Helper->self, 3.0s cast, range 10+R 270-degree cone, knockback dist 15
    Ballast3 = 14896, // Helper->self, 3.0s cast, range 15+R 270-degree cone, knockback dist 15

    PiercingLaser = 14891, // Boss->self, 3.0s cast, range 30+R width 8 rect
    RepellingCannons = 14892, // Boss->self, 3.0s cast, range 10+R circle
    Spellsword = 14968, // Shabti->self, 3.5s cast, range 6+R 120-degree cone
    Superstorm = 14971, // Boss->self, 3.5s cast, single-target
    Superstorm2 = 14970, // Helper->self, 3.5s cast, range 8-20 donut
    Disseminate = 14899 // Serpent->self, 2.0s cast, range 6+R circle, casts on death of serpents
}

sealed class HighVoltage(BossModule module) : Components.CastInterruptHint(module, (uint)AID.HighVoltage);

sealed class Ballast(BossModule module) : Components.ConcentricAOEs(module, _shapes, true)
{
    private static readonly Angle a135 = 135f.Degrees();
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(5.5f, a135), new AOEShapeDonutSector(5.5f, 10.5f, a135), new AOEShapeDonutSector(10.5f, 15.5f, a135)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BallastVisual1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell, 3.6d), spell.Rotation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.Ballast1 => 0,
                (uint)AID.Ballast2 => 1,
                (uint)AID.Ballast3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(0.6d), spell.Rotation);
        }
    }
}

sealed class PiercingLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PiercingLaser, new AOEShapeRect(32.3f, 4f));
sealed class RepellingCannons(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RepellingCannons, 12.3f);
sealed class Superstorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Superstorm2, new AOEShapeDonut(8f, 20f));
sealed class Spellsword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spellsword, new AOEShapeCone(7.1f, 60f.Degrees()));
sealed class Disseminate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Disseminate, 7.2f);

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("For this stage Flying Sardine and Acorn Bomb are highly recommended.\nUse Flying Sardine to interrupt High Voltage.\nUse Acorn Bomb to put Shabtis to sleep until their buff runs out.");
    }
}

sealed class Stage15States : StateMachineBuilder
{
    public Stage15States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HighVoltage>()
            .ActivateOnEnter<Ballast>()
            .ActivateOnEnter<PiercingLaser>()
            .ActivateOnEnter<RepellingCannons>()
            .ActivateOnEnter<Superstorm>()
            .ActivateOnEnter<Spellsword>()
            .ActivateOnEnter<Disseminate>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 625, NameID = 8109)]
public sealed class Stage15 : BossModule
{
    public Stage15(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Shabti), Colors.Object);
        Arena.Actors(Enemies((uint)OID.Serpent), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Shabti => 2, // TODO: ideally AI would use Acorn Bomb to put it to sleep until buff runs out instead of attacking them directly
                (uint)OID.Serpent => 1,
                _ => 0
            };
        }
    }
}
