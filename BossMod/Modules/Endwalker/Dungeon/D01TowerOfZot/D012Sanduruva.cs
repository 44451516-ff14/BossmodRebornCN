namespace BossMod.Endwalker.Dungeon.D01TheTowerOifZot.D012Sanduruva;

public enum OID : uint
{
    Sanduruva = 0x33EF, // R=2.5
    BerserkerSphere = 0x33F0 // R=1.5-2.5
}

public enum AID : uint
{
    AutoAttack = 871, // Sanduruva->player, no cast, single-target
    Teleport = 25254,  // Sanduruva->location, no cast, single-target

    ExplosiveForce = 25250, //Sanduruva->self, 3.0s cast, single-target
    IsitvaSiddhi = 25257, // Sanduruva->player, 4.0s cast, single-target
    ManusyaBerserk = 25249, // Sanduruva->self, 3.0s cast, single-target
    ManusyaConfuse = 25253, // Sanduruva->self, 3.0s cast, range 40 circle
    ManusyaStop = 25255, // Sanduruva->self, 3.0s cast, range 40 circle
    PrakamyaSiddhi = 25251, // Sanduruva->self, 4.0s cast, range 5 circle
    PraptiSiddhi = 25256, //Sanduruva->self, 2.0s cast, range 40 width 4 rect
    SphereShatter = 25252 // BerserkerSphere->self, 2.0s cast, range 15 circle
}

sealed class IsitvaSiddhi(BossModule module) : Components.SingleTargetCast(module, (uint)AID.IsitvaSiddhi);

sealed class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(15f);
    private bool needsUpdate;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        if (!needsUpdate)
        {
            return;
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        if (aoe0.Activation.AddSeconds(-7d) < WorldState.CurrentTime)
        {
            var count = _aoes.Count;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                aoes[i].Risky = true;
            }
            needsUpdate = false;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BerserkerSphere)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, NumCasts == 0 ? WorldState.FutureTime(10.8d) : WorldState.FutureTime(20d), risky: false));
            needsUpdate = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SphereShatter)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}

sealed class PraptiSiddhi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PraptiSiddhi, new AOEShapeRect(40f, 2f));
sealed class PrakamyaSiddhi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PrakamyaSiddhi, 5f);
sealed class ManusyaConfuse(BossModule module) : Components.CastHint(module, (uint)AID.ManusyaConfuse, "Applies Manyusa Confusion");
sealed class ManusyaStop(BossModule module) : Components.CastHint(module, (uint)AID.ManusyaStop, "Applies Manyusa Stop");

sealed class D012SanduruvaStates : StateMachineBuilder
{
    public D012SanduruvaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ManusyaConfuse>()
            .ActivateOnEnter<IsitvaSiddhi>()
            .ActivateOnEnter<ManusyaStop>()
            .ActivateOnEnter<PrakamyaSiddhi>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<PraptiSiddhi>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", PrimaryActorOID = (uint)OID.Sanduruva, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 783u, NameID = 10257u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.Endwalker, SortOrder = 2)]
public sealed class D012Sanduruva(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-258f, -25.95682f), 19.5f, 48)], [new Rectangle(new(-258f, -46f), 8f, 1.25f),
    new Rectangle(new(-258f, -6f), 8f, 1.25f)]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.BerserkerSphere => AIHints.Enemy.PriorityPointless,
                _ => 0
            };
        }
    }
}
