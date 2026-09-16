namespace BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.YoungerTablitaurPiece;

public enum OID : uint
{
    YoungerTablitaurPiece = 0x4C60,
    ElderTablitaurPiece = 0x4C5F, // R3.600, x1
    Gen = 0x4E00, // R1.000, x2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50216, // 4C5F/YoungerTablitaurPiece->player, no cast, single-target
    Teleport = 48188, // YoungerTablitaurPiece/4C5F->location, no cast, single-target

    TonzeSwipe1000Visual = 48189, // 4C5F->self, 6.5+0.5s cast, single-target
    TonzeSwipe1000 = 48190, // Helper->self, 7.0s cast, range 60 width 60 rect
    TonzeSwipe1000VisualLong = 48191, // YoungerTablitaurPiece->self, 9.5+0.5s cast, single-target
    TonzeSwipe1000Long = 48192, // Helper->self, 10.0s cast, range 60 width 60 rect
    TonzeSwing1111Visual = 48193, // 4C5F->self, 7.0+1.0s cast, single-target
    TonzeSwing1111 = 48194, // Helper->self, 8.0s cast, range 23 circle
    TonzeSwing1111VisualLong = 48195, // YoungerTablitaurPiece->self, 13.0+1.0s cast, single-target
    TonzeSwing1111Long = 48196, // Helper->self, 14.0s cast, range 23 circle

    TonzeStomp10Teleport = 48197, // YoungerTablitaurPiece->location, 6.3+0.7s cast, single-target
    Shockwave = 48199, // Helper->self, 7.0s cast, range ?-60 donut
    TonzeStomp10 = 48198, // Helper->self, 7.0s cast, range 5 circle

    TonzeStomp10TeleportLong = 48200, // 4C5F->location, 9.8+0.7s cast, single-target
    ShockwaveLong = 48202, // Helper->self, 10.5s cast, range ?-60 donut
    TonzeStomp10Long = 48201, // Helper->self, 10.5s cast, range 5 circle

    // TODO when does this end?
    EndlessSwingBoss = 48205, // 4C5F->self, 5.0+1.0s cast, single-target
    EndlessSwing = 48206, // Helper->4C5F, 6.0s cast, range 8 circle
    EndlessSwing1 = 48207, // Helper->4C5F, no cast, range 8 circle

    EndlessSwipesYounger = 48208, // YoungerTablitaurPiece->self, 5.0s cast, single-target
    EndlessSwipesElder = 48209, // ElderTablitaurPiece->self, 5.0s cast, single-target
    EndlessSwipes1 = 48210, // Helper->self, 5.2s cast, range 40 60.000-degree cone
    EndlessSwipes2 = 48211, // YoungerTablitaurPiece->self, no cast, single-target
    EndlessSwipes3 = 48212, // Helper->self, 0.5s cast, range 40 60-degree cone

    RallyingCheer = 48203, // YoungerTablitaurPiece->ElderTablitaurPiece, 7.0s cast, single-target
    TonzeSlash100 = 48204, // ElderTablitaurPiece->self/player, 9.0s cast, range 65 width 8 rect

    // Enrage - if one dies before the other too early
    DisorientingGroan = 48213, // YoungerTablitaurPiece->self, no cast, single-target
    EndlessSlashes = 48214, // YoungerTablitaurPiece->self, 9.0s cast, single-target
    EndlessSlashes1 = 48216, // Helper->self, no cast, range 80 width 70 rect
    EndlessSlashes2 = 48215, // YoungerTablitaurPiece->self, no cast, single-target
}

public enum IconID : uint
{
    TankBuster = 412, // player->self
    TurnRight = 167, // YoungerTablitaurPiece->self
    TurnLeft = 168, // ElderTablitaurPiece->self
}

public enum TetherID : uint
{
    KnockbackTether = 54, // YoungerTablitaurPiece/4C5F->4E00
    TankBusterTether = 260, // YoungerTablitaurPiece->ElderTablitaurPiece
    UnknownTether = 17, // 4C5F->player - most likely the target tether for the spin attack that follows the player around
    _Gen_Tether_chn_dark001f = 1, // ElderTablitaurPiece->player

}

sealed class Hint(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        hints.Add("Kill both at the same time!");
    }
}

sealed class TonzeSwipe1000 : Components.SimpleAOEGroups
{
    public TonzeSwipe1000(BossModule module) : base(module, [(uint)AID.TonzeSwipe1000, (uint)AID.TonzeSwipe1000Long], new AOEShapeRect(60.0f, 30.0f),
        expectedNumCasters: 2)
    {
        MaxDangerColor = 1;
        MaxRisky = 1;
    }
}

sealed class TonzeSwing1111 : Components.SimpleAOEGroups
{
    public TonzeSwing1111(BossModule module) : base(module, [(uint)AID.TonzeSwing1111, (uint)AID.TonzeSwing1111Long], 23.0f, expectedNumCasters: 2)
    {
        MaxDangerColor = 1;
        MaxRisky = 1;
    }
}

sealed class Shockwave(BossModule module) : Components.SimpleKnockbackGroups(module, [(uint)AID.Shockwave, (uint)AID.ShockwaveLong], 20.0f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        SortHelpers.SortKnockbacksByActivation(Casters);
    }
}

sealed class TonzeStomp10 : Components.SimpleAOEGroups
{
    public TonzeStomp10(BossModule module) : base(module, [(uint)AID.TonzeStomp10, (uint)AID.TonzeStomp10Long], 5.0f, expectedNumCasters: 2)
    {
        MaxDangerColor = 1;
        MaxRisky = 1;
    }
}

// TODO change shape to arc cap
sealed class EndlessSwing(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle shape = new(8.0f);
    private Actor? spellSource;
    private readonly EndlessSwipes? endlessSwipes = module.FindComponent<EndlessSwipes>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EndlessSwingBoss)
        {
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation));
            spellSource = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.EndlessSwipes1 or (uint)AID.EndlessSwipes3)
        {
            NumCasts++;

            if (NumCasts == 13)
            {
                aoes.Clear();
            }
        }
    }

    public override void Update()
    {
        // If the caster of swipes dies, then we have to clear any left over aoes otherwise they will not get removed - it can be either boss
        if (endlessSwipes != null && endlessSwipes.isCasterDead)
        {
            aoes.Clear();
            return;
        }

        var count = aoes.Count;
        if (count == 0 || spellSource == null)
        {
            return;
        }

        var nextAOEs = CollectionsMarshal.AsSpan(aoes);
        ref var aoe = ref nextAOEs[0];
        aoe.Origin = spellSource.Position;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

sealed class EndlessSwipes(BossModule module) : Components.GenericRotatingAOE(module)
{
    private ActorCastInfo? spellInfo;
    private Actor? source;
    private Angle increment = default;
    private readonly AOEShapeCone shape = new(40f, 30f.Degrees());
    public bool isCasterDead = false;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        increment = iconID switch
        {
            (uint)IconID.TurnLeft => 30.0f.Degrees(),
            (uint)IconID.TurnRight => -30.0f.Degrees(),
            _ => default
        };

        InitIfReady();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // The spell for the rotation and starting locXZ
        if (spell.Action.ID == (uint)AID.EndlessSwipes1)
        {
            spellInfo = spell;
            InitIfReady();
        }

        // The spell for which boss is actually performing the spell - it can be either one
        if (spell.Action.ID is (uint)AID.EndlessSwipesYounger or (uint)AID.EndlessSwipesElder)
        {
            source = caster;
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (spellInfo != null && increment != default && source != null)
        {
            Sequences.Add(new(shape, spellInfo.LocXZ, spellInfo.Rotation, increment, Module.CastFinishAt(spellInfo), 1.5d, 13, 3, actorID: source.InstanceID));
            spellInfo = null;
            increment = default;
            source = null;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.EndlessSwipes1 or (uint)AID.EndlessSwipes3)
        {
            if (Sequences.Count > 0)
            {
                AdvanceSequence(0, WorldState.CurrentTime);
            }
        }
    }

    // If the caster of swipes dies, then we have to clear any left over aoes otherwise they will not get removed - it can be either boss
    public override void Update()
    {
        base.Update();

        if (Sequences.Count == 0)
        {
            return;
        }

        var target = WorldState.Actors.Find(Sequences[0].ActorID);
        if (target == null || target.IsDead)
        {
            Sequences.Clear();
            isCasterDead = true;
        }
    }
}

sealed class TonzeSlash100(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(65.0f, 4.0f), (uint)IconID.TankBuster, (uint)AID.TonzeSlash100,
    9.1d, source: module.Enemies((uint)OID.ElderTablitaurPiece)[0], tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class YoungerTablitaurPieceStates : StateMachineBuilder
{
    public YoungerTablitaurPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TonzeSwipe1000>()
            .ActivateOnEnter<TonzeSwing1111>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<TonzeStomp10>()
            .ActivateOnEnter<EndlessSwipes>()
            .ActivateOnEnter<EndlessSwing>()
            .ActivateOnEnter<TonzeSlash100>()
            .Raw.Update = () => AllDeadOrDestroyed(YoungerTablitaurPiece.Bosses);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.YoungerTablitaurPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1089u, NameID = 14556u, SortOrder = 4)]
public sealed class YoungerTablitaurPiece : BossModule
{
    public static readonly uint[] Bosses = [(uint)OID.YoungerTablitaurPiece, (uint)OID.ElderTablitaurPiece];

    public YoungerTablitaurPiece(WorldState ws, Actor primary) : base(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f))
    {
        ActivateComponent<Hint>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Bosses);
    }
}
