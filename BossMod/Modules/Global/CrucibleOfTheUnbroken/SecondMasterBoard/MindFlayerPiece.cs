namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.MindFlayerPiece;

public enum OID : uint {
    MindflayerPiece = 0x4CDD,
    Helper = 0x233C,
    MyconidPiece = 0x4CDE, // R0.600, x0 (spawn during fight)
    WaterPuddle = 0x1E9998, // R0.500, x0 (spawn during fight), EventObj type
    ShroombedPuddle = 0x4CDF, // R6.000, x0 (spawn during fight)
    ArcaneSphere = 0x4CE1, // R1.000-1.860, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttackThunder = 48623, // MindflayerPiece->player, no cast, single-target
    VoidWaterIIIBoss = 49199, // MindflayerPiece->self, 5.0s cast, single-target
    VoidWaterIII = 49200, // Helper->players, no cast, range 8 circle
    VoidThunderIIIBoss = 49205, // MindflayerPiece->self, 5.0s cast, single-target
    VoidThunderIIITB = 49206, // Helper->player, no cast, range 6 circle
    VoidThunderIIICross = 49207, // Helper->self, 4.0s cast, range 50 width 10 cross
    VoidThunderIIICross1 = 50939, // Helper->self, 4.0s cast, range 50 width 10 cross
    ArcaneUtterance = 49201, // MindflayerPiece->self, 5.0s cast, single-target
    ArcaneEnhancement = 49202, // MindflayerPiece->self, 5.0s cast, single-target
    DarkCurrentSmall = 49203, // 4CE1->self, 2.0s cast, range 100 width 4 rect
    DarkCurrentBig = 49204, // 4CE1->self, 2.0s cast, range 100 width 10 rect
    VoidParalyzeIII = 49208, // MindflayerPiece->self, 7.0s cast, range 60 circle

    // MyconidPiece
    AutoAttackMyconidPiece = 49682, // 4CDE->player, no cast, single-target
    SporeSpill = 49196, // Helper->self, 1.0s cast, range 6 circle

    Unknown = 49197, // 4CDF->self, no cast, range 6 circle - most likely spawning the puddle
}

public enum SID : uint {
    WaterResistanceDown = 5021, // Helper->player, extra=0x1/0x2
    LightningResistanceDownII = 4456, // none->player, extra=0x0
    SustainedDamage = 3795, // none->4CE1, extra=0x1
}

public enum IconID : uint {
    VoidWaterIIIIcon = 135, // player/4A04/4A15->self
    VoidThunderIIITankBuster = 344, // player->self
}

public enum TetherID : uint {
    ArcaneEnhancementTether = 426, // 4CE1->MindflayerPiece
}

sealed class VoidWaterIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VoidWaterIIIIcon, (uint)AID.VoidWaterIII, 8.0f, 5.1f);
sealed class VoidThunderIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VoidThunderIIITankBuster, (uint)AID.VoidThunderIIITB, 6.0f, 5.1f);
sealed class VoidThunderIIICross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidThunderIIICross, new AOEShapeCross(50.0f, 5.0f));
sealed class VoidParalyzeIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.VoidParalyzeIII);

sealed class WaterPuddles : Components.PersistentInvertibleVoidzone {

    public WaterPuddles(BossModule module) : base(module, 8.0f, GetVoidzones) {
        InvertResolveAt = WorldState.CurrentTime;
    }

    public static Actor[] GetVoidzones(BossModule module) {
        var enemies = module.Enemies((uint)OID.WaterPuddle);
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
}

sealed class ShroombedPuddles(BossModule module) : Components.Voidzone(module, 6.0f, GetVoidzones) {
    private static Actor[] GetVoidzones(BossModule module) {
        var enemies = module.Enemies((uint)OID.ShroombedPuddle);
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
}

sealed class ArcaneEnhancement(BossModule module) : Components.GenericAOEs(module) {
    private readonly List<(Actor actor, bool isTethered)> spheres = [];
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeRect smallRect = new(100.0f, 2.0f, 100.0f);
    private readonly AOEShapeRect bigRect = new(100.0f, 5.0f, 100.0f);
    private bool active = false; // Used to wait showing the aoes until the tethers are sent out

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.ArcaneSphere) {
            spheres.Add((actor, false));
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.ArcaneEnhancementTether) {
            var sphereIndex = spheres.FindIndex(sphere => sphere.actor == source);
            if (sphereIndex < 0) {
                return;
            }

            spheres[sphereIndex] = (spheres[sphereIndex].actor, true);
            active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.DarkCurrentSmall or (uint)AID.DarkCurrentBig) {
            if (spheres.Count > 0) {
                var sphereIndex = spheres.FindIndex(sphere => sphere.actor == caster);
                if (sphereIndex < 0) {
                    return;
                }

                spheres.RemoveAt(sphereIndex);

                if (spheres.Count == 0) {
                    active = false;
                }
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (!active) {
            return [];
        }

        aoes.Clear();

        var count = spheres.Count;
        if (count == 0) {
            return [];
        }

        foreach (var sphere in spheres) {
            if (sphere.isTethered) {
                aoes.Add(new(bigRect, sphere.actor.Position, sphere.actor.Rotation, actorID: sphere.actor.InstanceID));
            }

            if (!sphere.isTethered) {
                aoes.Add(new(smallRect, sphere.actor.Position, sphere.actor.Rotation, actorID: sphere.actor.InstanceID));
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class MindflayerPieceStates : StateMachineBuilder {
    public MindflayerPieceStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<VoidWaterIII>()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<VoidThunderIIICross>()
            .ActivateOnEnter<VoidParalyzeIII>()
            .ActivateOnEnter<WaterPuddles>()
            .ActivateOnEnter<ShroombedPuddles>()
            .ActivateOnEnter<ArcaneEnhancement>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy, PrimaryActorOID = (uint)OID.MindflayerPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14633u, SortOrder = 2)]
public sealed class MindflayerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f)) {
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var puddles = WaterPuddles.GetVoidzones(this);

        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i) {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch {
                (uint)OID.MyconidPiece => insidePuddle(e.Actor, puddles) ? 2 : AIHints.Enemy.PriorityForbidden,
                (uint)OID.MindflayerPiece => 1,
                _ => 0
            };
        }
    }

    private static bool insidePuddle(Actor actor, Actor[] puddles) {
        foreach (var puddle in puddles) {
            if (actor.Position.InCircle(puddle.Position, 6.0f)) {
                return true;
            }
        }

        return false;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.MyconidPiece), Colors.Vulnerable);
    }

    private readonly string[] _prePullHints = [
        "Place the water puddles apart, but close together.",
        "Kill the MyconidPiece inside the water puddles to solve the mechanic correctly"
    ];

    public override string[] PrePullHints => _prePullHints;
}
