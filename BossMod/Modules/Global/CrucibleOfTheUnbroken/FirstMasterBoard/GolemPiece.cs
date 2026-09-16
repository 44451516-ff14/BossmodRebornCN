namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.GolemPiece;

public enum OID : uint {
    GolemPiece = 0x4CCB,
    Helper = 0x233C,
    GolemPiece1 = 0x4CCC, // R4.000, x1
    GolemPieceHeart = 0x4D5A, // R2.200, x0 (spawn during fight), Part type
    SkyRock = 0x1EC0D2, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint {
    AutoAttackStone = 50930, // GolemPiece/4CCC->player, no cast, single-target
    EarthenRingBoss = 48752, // GolemPiece->self, 6.2+0.8s cast, single-target
    EarthenRing = 48753, // Helper->self, 7.0s cast, range 5-50 donut
    RockWall = 48748, // GolemPiece/4CCC->self, 3.0+1.0s cast, single-target
    Shockwave = 48750, // Helper->self, 4.0s cast, range 5 width 5 rect
    Rockslide = 48751, // Helper->self, no cast, range 30 width 10 rect
    PlaincrackerSwap = 48760, // GolemPiece->self, 6.0+1.0s cast, single-target
    Plaincracker1 = 48764, // 4CCC->self, no cast, single-target
    PlaincrackerShort = 48765, // Helper->self, 1.0s cast, range 20 circle
    PlaincrackerBoss = 48754, // 4CCC->self, 6.0+1.0s cast, single-target
    Plaincracker = 48755, // Helper->self, 7.0s cast, range 20 circle
    Obliterate = 50649, // 4CCC->self, 5.0s cast, range 60 circle
    OutcropBoss = 48767, // 4CCC/GolemPiece->self, 4.2+0.8s cast, single-target
    Outcrop = 48768, // Helper->self, 5.0s cast, range 40 60.000-degree cone
    Stoneshower = 48756, // 4CCC/GolemPiece->self, 3.0s cast, single-target
    StoneshowerCircle = 48757, // Helper->self, 1.0s cast, range 8 circle
    StoneshowerDonut = 48758, // Helper->self, 1.0s cast, range 3-11 donut
    SelfDestruct = 48766, // 4D5A->self, 20.0s cast, range 100 circle

    // Most likely to do with them swapping / turning into enrage heart
    GolemDeath = 48761, // Helper->4CCC/GolemPiece, no cast, single-target
    Unknown1 = 50687, // 4CCC/GolemPiece->self, no cast, single-target
}

public enum TetherID : uint {
    SwapTether = 431, // GolemPiece->4CCC
}

sealed class EarthenRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthenRing, new AOEShapeDonut(5.0f, 50.0f));
sealed class Plaincracker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plaincracker, 20.0f);
sealed class Obliterate(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Obliterate, 17.0f);
sealed class Outcrop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Outcrop, new AOEShapeCone(40.0f, 30.0f.Degrees()));
sealed class SelfDestruct(BossModule module) : Components.RaidwideCast(module, (uint)AID.SelfDestruct, "Enrage");

sealed class Shockwave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shockwave, new AOEShapeRect(5.0f, 2.5f)) {
    // Used to track if the mechanic started - needed as the boss can die while casting it, if the cast goes through and the boss dies, the mechanic
    // will still play out
    private bool active = false;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.GolemDeath && !active) {
            if (Casters.Count > 0) {
                Casters.Clear();
            }
        }
    }

    public override void OnMapEffect(byte index, uint state) {
        if (state == 2097168 || state == 131073) {
            active = true;
        }

        if (state == 4194308 || state == 524292) {
            Casters.Clear();
            active = false;
        }
    }
}

// TODO consider storing the index to figure why the best escape would be from the start? - 28 - 17 is bottom, 17-28 is top
sealed class Rockslide(BossModule module) : Components.GenericAOEs(module) {
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeRect shape = new(15.0f, 5.0f, 15.0f);

    // Used to track if the mechanic started - needed as the boss can die while casting it, if the cast goes through and the boss dies, the mechanic
    // will still play out
    private bool active = false;

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.GolemDeath && !active) {
            if (aoes.Count > 0) {
                aoes.Clear();
            }
        }
    }

    public override void OnMapEffect(byte index, uint state) {
        Service.Logger.Info("Map effect " + index);

        // Outside is bad - so we just display everything right away
        if (state == 2097168 && aoes.Count == 0) {
            aoes.Add(new(shape, new WPos(505.0f, 0.0f), 180.0f.Degrees()));
            aoes.Add(new(shape, new WPos(535.0f, 0.0f), 180.0f.Degrees()));
            active = true;
        }

        // Inside is bad
        if (state == 131073 && aoes.Count == 0) {
            aoes.Add(new(shape, new WPos(520.0f, 0.0f), 180.0f.Degrees()));
            active = true;
        }

        if ((state == 4194308 || state == 524292) && aoes.Count > 0) {
            aoes.Clear();
            active = false;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

// TODO add activation timer - giving tether + animation + 1.0 second cast
sealed class PlaincrackerSwap(BossModule module) : Components.GenericAOEs(module) {
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle shape = new(20.0f);
    private Actor? swapSource; // Used to track is the main source dies before the cast finishes

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.SwapTether) {
            var target = WorldState.Actors.Find(tether.Target);
            if (target == null) {
                return;
            }

            swapSource = source;
            aoes.Add(new(shape, target.Position, target.Rotation));
        }
    }

    // If the source of the cast dies before the cast finishes then the swapped boss will not cast PlainCracker
    public override void OnActorDeath(Actor actor) {
        if (swapSource == null) {
            return;
        }

        if (actor.InstanceID == swapSource.InstanceID) {
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.PlaincrackerShort) {
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

// TODO add activation time from enaim to cast
sealed class SkyRock(BossModule module) : Components.GenericAOEs(module) {
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle circle = new(8.0f);
    private readonly AOEShapeDonut donut = new(3.0f, 11.0f);

    public override void OnActorEAnim(Actor actor, uint state) {
        if (actor.OID == (uint)OID.SkyRock && state == 65538) {
            aoes.Add(new(circle, actor.Position, actor.Rotation));
        }

        if (actor.OID == (uint)OID.SkyRock && state == 4194432) {
            aoes.Add(new(donut, actor.Position, actor.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.StoneshowerCircle or (uint)AID.StoneshowerDonut) {
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

sealed class GolemPieceStates : StateMachineBuilder {
    public GolemPieceStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<EarthenRing>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<Rockslide>()
            .ActivateOnEnter<Plaincracker>()
            .ActivateOnEnter<Obliterate>()
            .ActivateOnEnter<Outcrop>()
            .ActivateOnEnter<PlaincrackerSwap>()
            .ActivateOnEnter<SkyRock>()
            .ActivateOnEnter<SelfDestruct>()
            .Raw.Update = () => AllDeadOrDestroyed(GolemPiece.Bosses);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.GolemPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14617u, SortOrder = 7)]
public sealed class GolemPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520f, 0f), new ArenaBoundsRect(20.0f, 15.0f)) {
    public static readonly uint[] Bosses = [(uint)OID.GolemPiece, (uint)OID.GolemPiece1, (uint)OID.GolemPieceHeart];

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actors(this, Bosses);
    }

    private readonly string[] _prePullHints = [
        "This boss has an enrage - When you have killed both golems, it will turn into a heart which you have to kill within 20.0 seconds",
    ];

    public override string[] PrePullHints => _prePullHints;
}
