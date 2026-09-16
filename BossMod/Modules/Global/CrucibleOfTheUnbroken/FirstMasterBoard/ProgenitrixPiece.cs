namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.ProgenitrixPiece;

public enum OID : uint {
    ProgenitrixPiece = 0x4CD2,
    Helper = 0x233C,
    BombPiece = 0x4CD4, // R0.900, x0 (spawn during fight)
    SnollPiece = 0x4CD5, // R0.900, x0 (spawn during fight)
    PyrobolusPiece = 0x4CD6, // R1.050, x0 (spawn during fight)
    GrenadePiece = 0x4CD3, // R2.800, x0 (spawn during fight)
    FirePuddle = 0x1E8D9B, // R0.500, x0 (spawn during fight), EventObj type
    Unknown = 0x4CD7, // R2.000, x9
}

public enum AID : uint {
    AutoAttack = 50786, // ProgenitrixPiece->player, no cast, single-target
    Teleport = 48797, // ProgenitrixPiece->location, no cast, single-target
    ScaldingScoldingBoss = 48798, // ProgenitrixPiece->self, 5.0s cast, single-target
    ScaldingScolding = 48799, // Helper->self, 6.0s cast, range 40 180.000-degree cone
    MassiveExplosionBoss = 48802, // ProgenitrixPiece->self, 7.0s cast, range 50 circle
    MassiveExplosion = 48790, // 4CD3->self, 4.0s cast, range 50 circle
    MeltdownBoss = 48800, // ProgenitrixPiece->player, no cast, single-target
    Meltdown = 48801, // Helper->self, no cast, range 40 width 12 rect
    FieryFuryTeleportStart = 48803, // ProgenitrixPiece->location, 8.0+0.9s cast, single-target
    FieryFuryTeleportRest = 48804, // ProgenitrixPiece->location, no cast, single-target
    FieryFuryAOE = 48827, // Helper->self, 7.9s cast, range 6 circle
    FieryFuryKnockback = 48805, // Helper->self, 7.9s cast, range 40 circle

    // BombPiece
    AutoAttackFire = 48622, // 4CD4->player, no cast, single-target
    SelfDestruct = 48792, // 4CD4->self, no cast, range 6 circle
    FireII = 48791, // 4CD4->location, 4.0s cast, range 5 circle

    // SnollPiece
    AutoAttackBlizzard = 48621, // 4CD5->player, no cast, single-target
    HypothermalCombustion = 48794, // 4CD5->self, no cast, range 10 circle
    IceSpikes = 48793, // 4CD5->self, 3.0s cast, single-target

    // PyrobolusPiece
    AutoAttackPyrobolusPiece = 49682, // 4CD6->player, no cast, single-target
    ToxicFumesActor = 48795, // 4CD6->self, 3.0s cast, single-target
    ToxicFumes = 48796, // Helper->self, 4.0s cast, range 40 20.000-degree cone
}

public enum SID : uint {
    Invincibility = 4410, // none->ProgenitrixPiece, extra=0x0
    Swelling = 5182, // none->4CD3, extra=0x1/0x2/0x3/0x5/0x6/0x7/0x8/0x9/0xA/0xB/0xC/0xD/0x4
    Bind = 2518, // ProgenitrixPiece->player, extra=0x0
    IceSpikes = 2528, // 4CD5->4CD5, extra=0x64
    Slow = 9, // 4CD5->player, extra=0x0
    Burns = 3065, // none->player, extra=0x0
    Burns1 = 3066, // none->player, extra=0x0
    Bleeding = 3077, // none->player, extra=0x0
    Bleeding1 = 3078, // none->player, extra=0x0
}

public enum IconID : uint {
    MeltdownTankBuster = 412, // player->self
}

public enum TetherID : uint {
    InvincibilityTether = 5, // 4CD3->ProgenitrixPiece
}

sealed class ScaldingScolding(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScaldingScolding, new AOEShapeCone(40.0f, 90.0f.Degrees()));
sealed class MassiveExplosion(BossModule module) : Components.RaidwideCast(module, (uint)AID.MassiveExplosion);
sealed class ToxicFumes(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ToxicFumes, new AOEShapeCone(40.0f, 10.0f.Degrees()));
sealed class FireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireII, 6.0f);

sealed class Meltdown(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(40.0f, 6.0f), (uint)IconID.MeltdownTankBuster, (uint)AID.Meltdown,
    activationDelay: 7.9d, tankbuster: true);
sealed class MeltdownKnockback(BossModule module) : Components.SimpleKnockbacks(module, default, 15.0f, kind: Kind.AwayFromOrigin) {
    private Actor? target;
    private DateTime activation = default;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (iconID == (uint)IconID.MeltdownTankBuster) {
            var targetPlayer = WorldState.Actors.Find(targetID);
            if (targetPlayer == null) {
                return;
            }

            target = targetPlayer;
            activation = WorldState.FutureTime(7.9f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Meltdown) {
            target = null;
            activation = default;
        }
    }

    public override void Update() {
        Casters.Clear();

        if (target == null) {
            return;
        }

        var minDist = KnockbackKind == Kind.TowardsOrigin ? (MinDistance + (MinDistanceBetweenHitboxes ? Raid.Player()!.HitboxRadius + Module.PrimaryActor.HitboxRadius : default)) : MinDistance;
        Casters.Add(new(Module.PrimaryActor.Position, Distance, activation, Shape, default, KnockbackKind, minDist, [], default, IgnoreImmunes,
            ResolveArenaProjectionLayer(target.Position.Z), RestrictToArenaProjectionLayer));
    }
}

sealed class FirePuddles(BossModule module) : Components.Voidzone(module, 6.0f, GetVoidzones) {
    private static Actor[] GetVoidzones(BossModule module) {
        var enemies = module.Enemies((uint)OID.FirePuddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class FieryFuryKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FieryFuryKnockback, 15.0f, maxCasts: 2);
sealed class FieryFuryAOE : Components.SimpleAOEs {
    public FieryFuryAOE(BossModule module) : base(module, (uint)AID.FieryFuryAOE, 6.0f, maxCasts: 2) {
        MaxDangerColor = 1;
    }
}

sealed class ProgenitrixPieceStates : StateMachineBuilder {
    public ProgenitrixPieceStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<ScaldingScolding>()
            .ActivateOnEnter<MassiveExplosion>()
            .ActivateOnEnter<ToxicFumes>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<FirePuddles>()
            .ActivateOnEnter<Meltdown>()
            .ActivateOnEnter<MeltdownKnockback>()
            .ActivateOnEnter<FieryFuryAOE>()
            .ActivateOnEnter<FieryFuryKnockback>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.ProgenitrixPiece, Contributors = "Equilius",
    GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14623u, SortOrder = 9)]
public sealed class ProgenitrixPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f)) {
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i) {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch {
                (uint)OID.GrenadePiece => 5,
                (uint)OID.PyrobolusPiece => 4,
                (uint)OID.BombPiece => 3,
                (uint)OID.SnollPiece => 2,
                (uint)OID.ProgenitrixPiece => e.Actor.FindStatus((uint)SID.Invincibility) != null ? AIHints.Enemy.PriorityForbidden : 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GrenadePiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.PyrobolusPiece));
        Arena.Actors(Enemies((uint)OID.BombPiece));
        Arena.Actors(Enemies((uint)OID.SnollPiece));
    }

    private readonly string[] _prePullHints = [
        "Fight kill order priority: GrenadePiece (purple) -> Any other add -> Boss",
        "Killing ice bombs near fire puddles will get rid of them",
    ];

    public override string[] PrePullHints => _prePullHints;
}
