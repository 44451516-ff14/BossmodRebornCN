namespace BossMod.Modules.Global.CrucibleOfTheUnbroken.FirstMasterBoard.GargoylePiece;

public enum OID : uint
{
    GargoylePiece = 0x4CC2,
    Malady = 0x4CC3, // R1.000, x10
    PurpleVoidZone = 0x1E963D, // R0.500, x0 (spawn during fight), EventObj type
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50396, // GargoylePiece->player, no cast, single-target
    Teleport = 48732, // GargoylePiece->location, no cast, single-target
    DesolationBoss = 48726, // GargoylePiece->self, 2.2+1.3s cast, single-target
    Desolation = 48727, // Helper->self, 3.5s cast, range 60 width 7 rect
    GrimFate = 48730, // GargoylePiece->player, 5.0s cast, single-target
    GrimFate1 = 48731, // Helper->player, no cast, single-target
    SeaOfPitchBoss = 48728, // GargoylePiece->self, 3.0s cast, single-target
    SeaOfPitch = 48729, // Helper->location, 3.0s cast, range 6 circle
    RipplingEvisceration = 48719, // GargoylePiece->self, 5.4+0.6s cast, single-target
    RipplingEviscerationOuter = 48720, // Helper->self, 6.0s cast, range 13 circle
    RipplingEviscerationInner = 48721, // Helper->self, 8.0s cast, range 13-30 donut

    SweepingEviscerationBoss = 48717, // GargoylePiece->self, 7.9s cast, single-target
    SweepingEviscerationTeleport = 50932, // GargoylePiece->location, no cast, single-target
    SweepingEvisceration1 = 50933, // Helper->player, no cast, single-target
    SweepingEvisceration = 48718, // Helper->self, no cast, range 60 90-degree cone

    MaladyBoss = 48713, // GargoylePiece->self, 4.0+1.0s cast, single-target
    MaladyActor = 48714, // 4CC3->GargoylePiece, 4.7s cast, single-target
    Malady = 48715, // Helper->location, 5.0s cast, range 6 circle
    Burst = 48716, // 4CC3->self, no cast, range 6 circle

    FivefoldFalloutBoss = 50695, // GargoylePiece->self, 2.5+0.5s cast, single-target
    FivefoldFalloutLong = 50696, // Helper->self, 3.0s cast, range 60 circle
    FivefoldFalloutBoss1 = 48722, // GargoylePiece->self, no cast, single-target
    FivefoldFalloutShort = 48723, // Helper->self, 0.5s cast, range 60 circle
    FivefoldFalloutBossKnockback = 48724, // GargoylePiece->self, 2.5+0.5s cast, single-target
    FivefoldFalloutKnockback = 48725, // Helper->self, 3.0s cast, range 60 circle
}

public enum SID : uint
{
    DamageUp = 2550, // none->GargoylePiece, extra=0xA/0x9/0x8/0x7/0x6/0x5/0x4/0x3/0x2/0x1
    GrowingDread = 5178, // 4CC3->player, extra=0x1/0x2/0x3/0x4
    Hysteria = 4167, // 4CC3->player, extra=0x0
}

public enum IconID : uint
{
    TankBuster = 218, // player->self
}

public enum TetherID : uint
{
    Tether = 57, // GargoylePiece->player
    TetherStretched = 1, // GargoylePiece->player
    OrbTether = 426, // 4CC3->GargoylePiece
}

sealed class Desolation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Desolation, new AOEShapeRect(60.0f, 3.5f));
sealed class GrimFate(BossModule module) : Components.SingleTargetCast(module, (uint)AID.GrimFate);
sealed class SeaOfPitch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeaOfPitch, 6.0f);

sealed class RipplingEvisceration(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle circle = new(13.0f);
    private readonly AOEShapeDonut donut = new(13.0f, 30.0f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RipplingEviscerationOuter)
        {
            aoes.Add(new(circle, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            SortHelpers.SortAOEByActivation(aoes);
        }

        if (spell.Action.ID == (uint)AID.RipplingEviscerationInner)
        {
            aoes.Add(new(donut, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            SortHelpers.SortAOEByActivation(aoes);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RipplingEviscerationOuter or (uint)AID.RipplingEviscerationInner)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var nextAOEs = CollectionsMarshal.AsSpan(aoes);

        for (var i = 0; i < nextAOEs.Length; i++)
        {
            ref var aoe = ref nextAOEs[i];
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = i == 0;
        }

        return nextAOEs;
    }
}

sealed class SweepingEviscerationTether(BossModule module) : Components.StretchTetherDuo(module, 21.0f, 8.1f);
sealed class SweepingEvisceration(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCone shape = new(60.0f, 90.0f.Degrees());
    private Actor? tetherTarget;
    private Actor? tetherSource;
    private DateTime activation;
    private bool baitLocked = false;
    private readonly List<AOEInstance> aoes = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is not (uint)TetherID.Tether and not (uint)TetherID.TetherStretched)
        {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return;
        }

        tetherSource = source;
        tetherTarget = target;
        activation = WorldState.FutureTime(11.0f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SweepingEvisceration)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);

                if (aoes.Count == 0)
                {
                    tetherTarget = null;
                    tetherSource = null;
                    activation = default;
                    baitLocked = false;
                }
            }
        }

        if (spell.Action.ID == (uint)AID.SweepingEviscerationTeleport)
        {
            if (tetherTarget == null || tetherSource == null || activation == default)
            {
                return;
            }

            baitLocked = true;
            aoes.Clear();
            aoes.Add(new(shape, spell.TargetXZ, spell.Rotation, WorldState.FutureTime(2.8f)));
            aoes.Add(new(shape, spell.TargetXZ, spell.Rotation - 180.0f.Degrees(), WorldState.FutureTime(4.8f)));
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (baitLocked)
        {
            return CollectionsMarshal.AsSpan(aoes);
        }

        if (tetherTarget == null || tetherSource == null || activation == default) {
            return [];
        }

        var offset = Angle.FromDirection(tetherTarget.Position - tetherSource.Position);
        return new AOEInstance[] { new(shape, tetherTarget.Position, offset, risky: actor != tetherTarget) };
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) { }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (tetherTarget == null || tetherSource == null || activation == default) {
            return;
        }

        if (baitLocked)
        {
            var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
            for (var i = 0; i < incomingAOEs.Length; i++) {
                ref var aoe = ref incomingAOEs[i];
                shape.Draw(Arena, aoe.Origin, aoe.Rotation, i == 0 ? Colors.Danger : Colors.AOE);
            }
            return;
        }

        var offset = Angle.FromDirection(tetherTarget.Position - tetherSource.Position);
        shape.Outline(Arena, tetherTarget.Position, offset);
    }
}

sealed class Malady(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Malady, 6.0f);
sealed class MaladyOrbs : Components.PersistentInvertibleVoidzone
{
    private static List<Actor> orbs = [];

    public MaladyOrbs(BossModule module) : base(module, 2.0f, GetVoidzones)
    {
        InvertResolveAt = WorldState.CurrentTime;
        orbs = []; // To ensure no orbs exist upon loading the module
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.OrbTether)
        {
            orbs.Add(source);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
        {
            if (orbs.Count > 0)
            {
                orbs.Remove(caster);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.GrowingDread && status.Extra == 0x4)
        {
            InvertResolveAt = default;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.GrowingDread)
        {
            InvertResolveAt = WorldState.CurrentTime;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (orbs.Count == 0 )
        {
            return;
        }

        if (Inverted)
        {
            hints.Add("Soak orbs until you reach 4 stacks!", false);
        }

        if (!Inverted)
        {
            hints.Add("Don't soak any orbs!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var color = Inverted ? Colors.SafeFromAOE : default;
        using (Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            foreach (var orb in orbs)
            {
                if (ArenaProjectionLayerParticipantApplies(orb, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                    Shape.Draw(Arena, orb.Position, orb.Rotation, color);
            }
        }
    }

    private static List<Actor> GetVoidzones(BossModule module) => orbs;
}

sealed class PurpleVoidZone(BossModule module) : Components.Voidzone(module, 6.0f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.PurpleVoidZone);
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

sealed class FivefoldFallout(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> knockbacks = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FivefoldFalloutLong)
        {
            knockbacks.Add(new(spell.LocXZ, 20.0f, WorldState.FutureTime(11.8d), kind: Kind.AwayFromOrigin));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FivefoldFalloutLong or (uint)AID.FivefoldFalloutShort)
        {
            NumCasts++;
        }

        if (spell.Action.ID == (uint)AID.FivefoldFalloutKnockback)
        {
            if (knockbacks.Count > 0)
            {
                knockbacks.RemoveAt(0);
                NumCasts = 0;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (knockbacks.Count == 0 || NumCasts == 4)
        {
            return;
        }

        base.AddHints(slot, actor, hints);
        hints.Add("4x Raidwide!");
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (knockbacks.Count == 0)
        {
            return [];
        }

        var knockback = knockbacks[0];
        if ((knockback.Activation - WorldState.CurrentTime).TotalSeconds < 6.0d)
        {
            return CollectionsMarshal.AsSpan(knockbacks);
        }

        return [];
    }
}

sealed class GargoylePieceStates : StateMachineBuilder
{
    public GargoylePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<RipplingEvisceration>()
            .ActivateOnEnter<SweepingEviscerationTether>()
            .ActivateOnEnter<SweepingEvisceration>()
            .ActivateOnEnter<Malady>()
            .ActivateOnEnter<MaladyOrbs>()
            .ActivateOnEnter<GrimFate>()
            .ActivateOnEnter<SeaOfPitch>()
            .ActivateOnEnter<PurpleVoidZone>()
            .ActivateOnEnter<FivefoldFallout>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.GargoylePiece, Contributors = "Equilius",
    GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14608u, SortOrder = 5)]
public sealed class GargoylePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsSquare(20f)) {
    private readonly string[] _prePullHints = [
        "During this fight 10 orbs will spawn, collecting an orb will give a stack of GrowingDread, reaching 5 stacks will turn it into Hysteria. So, " +
        "you will need to soak 4 orbs then wait for the GrowingDread debuff to fall off then continue soaking the orbs.",
        "Each orb on the map will grant the boss a stack of damage up"
    ];

    public override string[] PrePullHints => _prePullHints;
}
