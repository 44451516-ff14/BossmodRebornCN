namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.CorpseFlowerPiece;

public enum OID : uint {
    CorpseFlowerPiece = 0x4CBD,
    Helper = 0x233C,
    SaplingPiece = 0x4CBE, // R0.750, x0 (spawn during fight)
    QueenHawkPiece = 0x4CBF, // R0.720, x0 (spawn during fight)
    ThornPuddle = 0x1EC0E2, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint {
    AutoAttack = 49681, // CorpseFlowerPiece->player, no cast, single-target
    AutoAttackQueenHawk = 48688, // 4CBF->player, no cast, single-target
    BuddingThorns = 48687, // CorpseFlowerPiece->self, 3.0s cast, single-target
    RottenStenchVisual = 48690, // CorpseFlowerPiece->self, 5.0+1.0s cast, single-target
    RottenStench = 48691, // CorpseFlowerPiece->self, no cast, range 45 width 12 rect
    FloralTrap = 48683, // CorpseFlowerPiece->self, 5.0s cast, range 80 circle
    FloralTrapPull = 48684, // CorpseFlowerPiece->self, no cast, range 45 ?-degree cone
    Devour = 48685, // CorpseFlowerPiece->self, no cast, range 8 ?-degree cone
    AcidRainBoss = 48692, // CorpseFlowerPiece->self, 3.0s cast, single-target
    AcidRainStart = 48693, // Helper->location, 2.0s cast, range 6 circle
    AcidRainRest = 48694, // Helper->location, no cast, range 6 circle
    FinalSting = 48689, // 4CBF->player, 5.0s cast, single-target
    Spit = 48686, // CorpseFlowerPiece->self, 4.0s cast, range 0 ???
}

public enum SID : uint {
    Bind = 2518, // CorpseFlowerPiece->player, extra=0x0
    Stun = 2656, // CorpseFlowerPiece->4CBF, extra=0x0
    Briar = 5176, // none->player, extra=0x32
    DamageUp = 2550, // none->CorpseFlowerPiece, extra=0x1
    Devoured = 421, // CorpseFlowerPiece->player, extra=0x0
}

public enum IconID : uint {
    FloralTrapLockOn = 703, // player->self
    RottenStenchWildCharge = 525, // CorpseFlowerPiece->player
    AcidRainLockOn = 197, // player->self
}

// TODO consider making components for growable circles since they are so common and annoying to work with, DateTime, SID etc
public class FloralTrap(BossModule module) : Components.CastCounter(module, (uint)AID.FloralTrap) {
    private readonly AOEShapeCircle shape = new(6.0f);
    private readonly Dictionary<Actor, DateTime> growPuddlesTimers = [];

    private DateTime InvertResolveAt;
    private int? ArenaProjectionLayer = null;
    private bool? RestrictToArenaProjectionLayer = false;

    private readonly Func<BossModule, IEnumerable<Actor>> Sources = GetVoidzones;
    private bool Inverted => InvertResolveAt != default;

    public override void OnActorEAnim(Actor actor, uint state) {
        if (actor.OID == (uint)OID.ThornPuddle && state == 0x00100020) {
            growPuddlesTimers[actor] = WorldState.CurrentTime;
        }
    }

    public override void OnActorEState(Actor actor, ushort state) {
        if (actor.OID == (uint)OID.ThornPuddle && state == 4) {
            growPuddlesTimers.Remove(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.FloralTrap) {
            InvertResolveAt = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Devour) {
            InvertResolveAt = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (!ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer)) {
            return;
        }

        var inVoidzone = false;
        foreach (var s in Sources(Module)) {
            if (ArenaProjectionLayerParticipantApplies(s, ArenaProjectionLayer, RestrictToArenaProjectionLayer) && ShapeSize(s).Check(actor.Position, s)) {
                inVoidzone = true;
                break;
            }
        }

        if (Inverted) {
            hints.Add(inVoidzone ? "Stay in voidzone" : "Go to voidzone!", !inVoidzone);
        } else if (inVoidzone) {
            hints.Add("GTFO from voidzone!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer)) {
            return;
        }

        var shapes = new List<ShapeDistance>();
        foreach (var source in Sources(Module)) {
            if (ArenaProjectionLayerParticipantApplies(source, ArenaProjectionLayer, RestrictToArenaProjectionLayer)) {
                var shape = ShapeSize(source).Distance(source.Position.Quantized(), source.Rotation);
                shapes.Add(shape);
            }
        }

        if (shapes.Count == 0) {
            return;
        }

        hints.AddForbiddenZone(Inverted ? new SDInvertedUnion([.. shapes]) : new SDUnion([.. shapes]), InvertResolveAt, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) {
        var color = Inverted ? Colors.SafeFromAOE : default;
        using (Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer)) {
            foreach (var s in Sources(Module)) {
                if (ArenaProjectionLayerParticipantApplies(s, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                    ShapeSize(s).Draw(Arena, s.Position, s.Rotation, color);
            }
        }
    }

    private static Actor[] GetVoidzones(BossModule module) {
        var enemies = module.Enemies((uint)OID.ThornPuddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i) {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }

    private AOEShapeCircle ShapeSize(Actor source) {
        if (!growPuddlesTimers.TryGetValue(source, out var growPuddle)) {
            return shape;
        }

        var duration = (WorldState.CurrentTime - growPuddle).TotalSeconds;
        // base + (max grow - base) * (time happened / max growth time to reach max size)
        return new((float)Math.Min(6.0f + (10.0f - 6.0f) * (duration / 10.0f), 10.0f));
    }
}

sealed class RottenStench(BossModule module) : Components.GenericWildCharge(module, 6.0f, (uint)AID.RottenStenchVisual) {
    private Actor? target;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (iconID == (uint)IconID.RottenStenchWildCharge) {
            var targetPlayer = WorldState.Actors.Find(targetID);
            if (targetPlayer == null) {
                return;
            }

            target = targetPlayer;
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == WatchedAction) {
            Source = caster;
            Activation = Module.CastFinishAt(spell);
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.RottenStench) {
            target = null;
            Source = null;
        }
    }

    private void InitIfReady() {
        if (target == null || Source == null) {
            return;
        }

        foreach (var (slot, player) in Raid.WithSlot(false, true, true)) {
            PlayerRoles[slot] = player.InstanceID == target.InstanceID ? PlayerRole.Target : PlayerRole.Share;
        }
    }
}

sealed class AcidRainBait(BossModule module) : Components.BaitAwayIcon(module, 6.0f, (uint)IconID.AcidRainLockOn) {
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.AcidRainStart) {
            CurrentBaits.Clear();
        }
    }
}

sealed class AcidRain(BossModule module) : Components.StandardChasingAOEs(module, 6.0f, (uint)AID.AcidRainStart, (uint)AID.AcidRainRest, 5.0f, 1.0d, 8,
    icon: (uint)IconID.AcidRainLockOn);

sealed class Devour(BossModule module) : Components.GenericBaitProximity(module) {
    private readonly AOEShapeCone shape = new(7.0f, 15.0f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.FloralTrap) {
            CurrentBaits.Add(new(Module.PrimaryActor, shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Devour) {
            if (CurrentBaits.Count > 0) {
                CurrentBaits.RemoveAt(0);
            }
        }
    }
}

sealed class CorpseFlowerPieceStates : StateMachineBuilder {
    public CorpseFlowerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FloralTrap>()
            .ActivateOnEnter<RottenStench>()
            .ActivateOnEnter<AcidRainBait>()
            .ActivateOnEnter<AcidRain>()
            .ActivateOnEnter<Devour>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.CorpseFlowerPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14603u, SortOrder = 3)]
public sealed class CorpseFlowerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f)) {
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i) {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch {
                (uint)OID.QueenHawkPiece => 2,
                (uint)OID.CorpseFlowerPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.QueenHawkPiece), Colors.Vulnerable);
    }

    private readonly string[] _prePullHints = [
        "Fight kill order priority: QueenHawkPiece (purple) -> Boss (red)",
        "You can use the FloralTrap pull-in to have the boss kill the QueenHawkPiece if you stand behind it during the mechanic"
    ];

    public override string[] PrePullHints => _prePullHints;
}
