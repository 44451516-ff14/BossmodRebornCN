namespace BossMod.Endwalker.Dungeon.D01TheTowerOfZot.D013MagusSisters;

public enum OID : uint
{
    Cinduruva = 0x33F1, // R2.2
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
    ManusyaGlare = 25274, // Cinduruva->none, no cast, single-target
    ManusyaReflect = 25259, // Cinduruva->self, 4.2s cast, range 40 circle
    ManusyaThunder1 = 25284, // Minduruva->player, no cast, single-target
    ManusyaThunder2 = 25289, // Minduruva->player, 2.0s cast, single-target
    PraptiSiddhi = 25275, // Sanduruva->self, 2.0s cast, range 40 width 4 rect
    Samsara = 25273, // Cinduruva->self, 3.0s cast, range 40 circle
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
    Poison = 18 // Boss->player, extra=0x0
}

sealed class Dhrupad(BossModule module) : BossComponent(module)
{
    private int NumCasts;
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Dhrupad)
        {
            active = true;
        }
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
        {
            hints.Add("3 single target hits + DoTs");
        }
    }
}

sealed class ManusyaBio(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ManusyaBio, "Tankbuster + cleansable poison");

sealed class Poison(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Poison, "Poison", "poisoned");

sealed class IsitvaSiddhi(BossModule module) : Components.SingleTargetCast(module, (uint)AID.IsitvaSiddhi);
sealed class Samsara(BossModule module) : Components.RaidwideCast(module, (uint)AID.Samsara);
sealed class DeltaThunderIII1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaThunderIII1, 3f);
sealed class DeltaThunderIII2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaThunderIII2, 5f);
sealed class DeltaThunderIII3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaThunderIII3, new AOEShapeRect(40f, 5f));
sealed class DeltaThunderIII4(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DeltaThunderIII4, 5f, 4, 4);
sealed class DeltaBlizzardIII1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaBlizzardIII1, new AOEShapeCone(40.5f, 10f.Degrees()));
sealed class DeltaBlizzardIII2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaBlizzardIII2, new AOEShapeRect(44f, 2f));
sealed class DeltaBlizzardIII3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaBlizzardIII3, 15f);
sealed class DeltaFireIII1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaFireIII1, new AOEShapeDonut(5f, 40f));
sealed class DeltaFireIII2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaFireIII2, new AOEShapeRect(44f, 5f));
sealed class DeltaFireIII3(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DeltaFireIII3, 6f);
sealed class PraptiSiddhi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PraptiSiddhi, new AOEShapeRect(40f, 2f));

sealed class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BerserkerSphere)
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(7.3d)));
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

sealed class PrakamyaSiddhi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PrakamyaSiddhi, 5f);
sealed class ManusyaBlizzardIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManusyaBlizzardIII2, new AOEShapeCone(40.5f, 10.Degrees()));

sealed class D013MagusSistersStates : StateMachineBuilder
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
            .Raw.Update = () => AllDeadOrDestroyed(D013MagusSisters.Bosses);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", PrimaryActorOID = (uint)OID.Cinduruva, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 783u, NameID = 10265u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.Endwalker, SortOrder = 3)]
sealed class D013MagusSisters(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-27.5f, -49.5f), 19.59436f, 48)], [new Rectangle(new(-41.64214f, -35.35786f), 8f, 1.25f, -45f.Degrees())]);
    public static readonly uint[] Bosses = [(uint)OID.Cinduruva, (uint)OID.Sanduruva, (uint)OID.Minduruva];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Bosses);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Cinduruva => 2,
                (uint)OID.Minduruva => 1,
                _ => 0
            };
        }
    }
}
