namespace BossMod.Global.MaskedCarnivale.Stage24.Act2;

public enum OID : uint
{
    Boss = 0x2736, //R=2.0
    ArenaViking = 0x2737, //R=1.0
    ArenaMagus = 0x2738 //R=1.0
}

public enum AID : uint
{
    AutoAttack = 6499, // 2736->player, no cast, single-target
    AutoAttack2 = 6497, // 2737->player, no cast, single-target
    CondensedLibra = 15319, // 2736->player, 5.0s cast, single-target
    TripleHit = 15320, // 2736->players, 3.0s cast, single-target
    Mechanogravity = 15322, // 2736->location, 3.0s cast, range 6 circle
    Fire = 14266, // 2738->player, 1.0s cast, single-target
    Starstorm = 15317, // 2738->location, 3.0s cast, range 5 circle
    RagingAxe = 15316, // 2737->self, 3.0s cast, range 4+R 90-degree cone
    Silence = 15321 // 2736->player, 5.0s cast, single-target
}

class Starstorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Starstorm, 5f);
class Mechanogravity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mechanogravity, 6f);
class RagingAxe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RagingAxe, new AOEShapeCone(5f, 45f.Degrees()));
class CondensedLibra(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CondensedLibra, "Use Diamondback!");
class TripleHit(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TripleHit, "Use Diamondback!");
class Silence(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Silence);

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var magi = Module.Enemies((uint)OID.ArenaMagus);
        var countM = magi.Count;
        if (countM != 0)
            for (var i = 0; i < countM; ++i)
            {
                var magus = magi[i];
                if (!magus.IsDead)
                {
                    hints.Add($"{magus.Name} is immune to magical damage!");
                    break;
                }
            }
        var vikings = Module.Enemies((uint)OID.ArenaMagus);
        var countV = vikings.Count;
        if (countV != 0)
            for (var i = 0; i < countV; ++i)
            {
                var viking = vikings[i];
                if (!viking.IsDead)
                {
                    hints.Add($"{viking.Name} is immune to physical damage!");
                    return;
                }
            }
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"The {Module.PrimaryActor.Name} casts Silence which should be interrupted.\nCondensed Libra puts a debuff on you. Use Diamondback to survive the\nfollowing attack. Alternatively you can cleanse the debuff with Exuviation.");
    }
}

class Stage24Act2States : StateMachineBuilder
{
    public Stage24Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Starstorm>()
            .ActivateOnEnter<RagingAxe>()
            .ActivateOnEnter<Silence>()
            .ActivateOnEnter<Mechanogravity>()
            .ActivateOnEnter<CondensedLibra>()
            .ActivateOnEnter<TripleHit>()
            .ActivateOnEnter<Hints2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 634, NameID = 8128, SortOrder = 2)]
public class Stage24Act2 : BossModule
{
    public Stage24Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ArenaViking));
        Arena.Actors(Enemies((uint)OID.ArenaMagus));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ArenaMagus or (uint)OID.ArenaViking => 1, //TODO: ideally Viking should only be attacked with magical abilities and Magus should only be attacked with physical abilities
                _ => 0
            };
        }
    }
}
