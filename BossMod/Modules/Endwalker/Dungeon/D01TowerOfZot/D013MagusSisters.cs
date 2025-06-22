namespace BossMod.Endwalker.Dungeon.D01TheTowerOfZot.D013MagusSisters;

public enum OID : uint
{
    Boss = 0x33F1, // R2.2
    Sanduruva = 0x33F2, // R=2.5
    Minduruva = 0x33F3, // R=2.04
    BerserkerSphere = 0x33F0, // R=1.5-2.5
    Helper2 = 0x3610,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 871, // Sanduruva->player, no cast, single-target
    Teleport = 25254, // Sanduruva->location, no cast, single-target

    DeltaAttack = 25260, // Minduruva->Boss, 5.0s cast, single-target
    DeltaAttack1 = 25261, // Minduruva->Boss, 5.0s cast, single-target
    DeltaAttack2 = 25262, // Minduruva->Boss, 5.0s cast, single-target
    DeltaBlizzardIII1 = 25266, // Helper->self, 3.0s cast, range 40+R 20-degree cone
    DeltaBlizzardIII2 = 25267, // Helper->self, 3.0s cast, range 44 width 4 rect
    DeltaBlizzardIII3 = 25268, // Helper->location, 5.0s cast, range 40 circle
    DeltaFireIII1 = 25263, // Helper->self, 4.0s cast, range 5-40 donut
    DeltaFireIII2 = 25264, // Helper->self, 3.0s cast, range 44 width 10 rect
    DeltaFireIII3 = 25265, // Helper->player, 5.0s cast, range 6 circle, spread
    DeltaThunderIII1 = 25269, // Helper->location, 3.0s cast, range 3 circle
    DeltaThunderIII2 = 25270, // Helper->location, 3.0s cast, range 5 circle
    DeltaThunderIII3 = 25271, // Helper->self, 3.0s cast, range 40 width 10 rect
    DeltaThunderIII4 = 25272, // Helper->player, 5.0s cast, range 5 circle, stack
    Dhrupad = 25281, // Minduruva->self, 4.0s cast, single-target, after this each of the non-tank players get hit once by a single-target spell (ManusyaBlizzard1, ManusyaFire1, ManusyaThunder1)
    IsitvaSiddhi = 25280, // Sanduruva->player, 4.0s cast, single-target, tankbuster
    ManusyaBlizzard1 = 25283, // Minduruva->player, no cast, single-target
    ManusyaBlizzard2 = 25288, // Minduruva->player, 2.0s cast, single-target
    ManusyaFaith = 25258, // Sanduruva->Minduruva, 4.0s cast, single-target
    ManusyaFire1 = 25282, // Minduruva->player, no cast, single-target
    ManusyaFire2 = 25287, // Minduruva->player, 2.0s cast, single-target
    ManusyaGlare = 25274, // Boss->none, no cast, single-target
    ManusyaReflect = 25259, // Boss->self, 4.2s cast, range 40 circle
    ManusyaThunder1 = 25284, // Minduruva->player, no cast, single-target
    ManusyaThunder2 = 25289, // Minduruva->player, 2.0s cast, single-target
    PraptiSiddhi = 25275, // Sanduruva->self, 2.0s cast, range 40 width 4 rect
    Samsara = 25273, // Boss->self, 3.0s cast, range 40 circle
    ManusyaBio = 25290, // Minduruva->player, 4.0s cast, single-target
    ManusyaBerserk = 25276, // Sanduruva->self, 3.0s cast, single-target
    ExplosiveForce = 25277, // Sanduruva->self, 2.0s cast, single-target
    SphereShatter = 25279, // BerserkerSphere->self, 1.5s cast, range 15 circle
    PrakamyaSiddhi = 25278, // Sanduruva->self, 4.0s cast, range 5 circle
    ManusyaBlizzardIII = 25285, // Minduruva->self, 4.0s cast, single-target
    ManusyaBlizzardIII2 = 25286 // Helper->self, 4.0s cast, range 40+R 20-degree cone
}

public enum SID : uint
{
    Poison = 18, // Boss->player, extra=0x0
    Burns = 2082, // Minduruva->player, extra=0x0
    Frostbite = 2083, // Minduruva->player, extra=0x0
    Electrocution = 2086 // Minduruva->player, extra=0x0
}

class Dhrupad(BossModule module) : BossComponent(module)
{
    private int NumCasts;
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Dhrupad)
            active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ManusyaFire1 or (uint)AID.ManusyaBlizzard1 or (uint)AID.ManusyaThunder1)
        {
            ++NumCasts;
            if (NumCasts == 3)
            {
                NumCasts = 0;
                active = false;
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (active)
            hints.Add("3 single target hits + DoTs");
    }
}

class ManusyaBio(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ManusyaBio, "Tankbuster + cleansable poison");

class Poison(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Poison, "Poison", "poisoned");

class IsitvaSiddhi(BossModule module) : Components.SingleTargetCast(module, (uint)AID.IsitvaSiddhi);
class Samsara(BossModule module) : Components.RaidwideCast(module, (uint)AID.Samsara);
class DeltaThunderIII1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaThunderIII1, 3f);
class DeltaThunderIII2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaThunderIII2, 5f);
class DeltaThunderIII3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaThunderIII3, new AOEShapeRect(40f, 5f));
class DeltaThunderIII4(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DeltaThunderIII4, 5f, 4, 4);
class DeltaBlizzardIII1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaBlizzardIII1, new AOEShapeCone(40.5f, 10f.Degrees()));
class DeltaBlizzardIII2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaBlizzardIII2, new AOEShapeRect(44f, 2f));
class DeltaBlizzardIII3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaBlizzardIII3, 15f);
class DeltaFireIII1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaFireIII1, new AOEShapeDonut(5f, 40f));
class DeltaFireIII2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaFireIII2, new AOEShapeRect(44f, 5f));
class DeltaFireIII3(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DeltaFireIII3, 6f);
class PraptiSiddhi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PraptiSiddhi, new AOEShapeRect(40f, 2f));

class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BerserkerSphere)
            _aoes.Add(new(circle, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(7.3d)));
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

class PrakamyaSiddhi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PrakamyaSiddhi, 5f);
class ManusyaBlizzardIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManusyaBlizzardIII2, new AOEShapeCone(40.5f, 10.Degrees()));

class D013MagusSistersStates : StateMachineBuilder
{
    public D013MagusSistersStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IsitvaSiddhi>()
            .ActivateOnEnter<ManusyaBio>()
            .ActivateOnEnter<Poison>()
            .ActivateOnEnter<Samsara>()
            .ActivateOnEnter<ManusyaBlizzardIII>()
            .ActivateOnEnter<PrakamyaSiddhi>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<PraptiSiddhi>()
            .ActivateOnEnter<DeltaFireIII1>()
            .ActivateOnEnter<DeltaFireIII2>()
            .ActivateOnEnter<DeltaFireIII3>()
            .ActivateOnEnter<DeltaThunderIII1>()
            .ActivateOnEnter<DeltaThunderIII2>()
            .ActivateOnEnter<DeltaThunderIII3>()
            .ActivateOnEnter<DeltaThunderIII4>()
            .ActivateOnEnter<Dhrupad>()
            .ActivateOnEnter<DeltaBlizzardIII1>()
            .ActivateOnEnter<DeltaBlizzardIII2>()
            .ActivateOnEnter<DeltaBlizzardIII3>()
            .Raw.Update = () => module.Enemies(D013MagusSisters.Bosses).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 783, NameID = 10265)]
class D013MagusSisters(WorldState ws, Actor primary) : BossModule(ws, primary, new(-27.5f, -49.5f), new ArenaBoundsCircle(20f))
{
    public static readonly uint[] Bosses = [(uint)OID.Boss, (uint)OID.Sanduruva, (uint)OID.Minduruva];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(Bosses));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 2,
                (uint)OID.Minduruva => 1,
                _ => 0
            };
        }
    }
}
