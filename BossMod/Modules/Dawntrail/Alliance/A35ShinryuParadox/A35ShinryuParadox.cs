namespace BossMod.Dawntrail.Alliance.A35ShinryuParadox;

public enum OID : uint
{
    ShinryuParadox = 0x4D92, // R25.000, x1
    UnkownActor = 0x4EB3, // R2.000, x5
    ArcaneSphere1 = 0x4D97, // R1.000, x1 : could be used to draw moving aoe rectangle if desired for atomic ray(?)
    ArcaneSphere2 = 0x4DCD, // R1.000, x1 : could be used to draw moving aoe rectangle if desired for atomic ray(?)
    ShinryuAutos = 0x4D9A, // R0.000, x3, Part type
    ShinryuGroin = 0x4D93, // R25.000, x1, Part type
    Helper = 0x233C, // R0.500, x24, Helper type
    GuloolJaJa = 0x4E53, // R5.000, x0 (spawn during fight)
    HollowKing = 0x4D96, // R25.000, x0 (spawn during fight)
    HollowKingAutos = 0x4D9B, // R0.000, x0 (spawn during fight), Part type

}

public enum AID : uint
{
    ShinryuAutoVisual = 49137, // ShinryuParadox->self, no cast, single-target
    ShinryuAuto = 49138, // ShinryuParadoxPart1->player, no cast, single-target

    CosmicBreathVisual1 = 49105, // ShinryuParadox->self, 6.0+1.0s cast, single-target
    CosmicBreathVisual2 = 49106, // ShinryuParadoxPart2->self, 6.0+1.0s cast, single-target
    CosmicBreath = 49107, // Helper->self, 7.0s cast, range 50 width 70 rect
    CosmicTailVisual1 = 49108, // Boss->self, 6.0+1.0s cast, single-target
    CosmicTailVisual2 = 49109, // ShinryusGroin->self, 6.0+1.0s cast, single-target
    CosmicTail = 49110, // Helper->self, 7.0s cast, range 50 width 70 rect
    CloakOfTwilight1 = 49111, // Boss->self, 3.0s cast, single-target
    CloakOfTwilight2 = 49112, // ShinryusGroin->self, 3.0s cast, single-target
    TwilightNebula1 = 49113, // Boss->self, 6.0s cast, single-target
    TwilightNebula2 = 49114, // ShinryusGroin->self, 6.0s cast, single-target
    TwilightRadiance = 49115, // Helper->self, no cast, range 60 circle
    TwilightShadow = 49116, // Helper->self, no cast, range 60 circle
    StarflareVisual1 = 49124, // Boss->self, 3.0s cast, single-target
    StarflareVisual2 = 49125, // ShinryusGroin->self, 3.0s cast, single-target
    StarflareP1Fast = 49126, // Helper->self, 5.0s cast, range 60 width 10 rect
    StarflareP1Slow = 49127, // Helper->self, 7.0s cast, range 60 width 10 rect
    CataclysmicVortexVisual1 = 49121, // Boss->self, 7.0s cast, single-target
    CataclysmicVortexVisual2 = 49122, // ShinryusGroin->self, 7.0s cast, single-target
    CataclysmicVortexFail = 49123, // Helper->player, no cast, single-target
    DarkNovaVisual1 = 49134, // Boss->self, 5.0s cast, single-target
    DarkNovaVisual2 = 49135, // ShinryusGroin->self, 5.0s cast, single-target
    DarkNova = 49136, // Helper->player, no cast, range 6 circle
    AtomicTailVisual1 = 49128, // Boss->self, 6.0+1.0s cast, single-target
    AtomicTailVisual2 = 49129, // ShinryusGroin->self, 6.0+1.0s cast, single-target
    AtomicTail = 49130, // Helper->self, 7.0s cast, range 50 width 70 rect
    GyreChargeVisual1 = 49131, // Boss->self, no cast, single-target
    GyreChargeVisual2 = 49132, // ShinryusGroin->self, no cast, single-target
    GyreCharge = 49133, // Helper->self, 0.5s cast, range 60 circle

    CelestialTrailVisual1 = 49139, // HollowKing->self, no cast, single-target
    CelestialTrailTower = 49140, // Helper->self, 8.0s cast, range 2 circle
    CelestialTrailVisual2 = 49141, // HollowKing->self, no cast, single-target
    CelestialTrailHPDown = 49142, // Helper->player/4D98, no cast, single-target
    CelestialTrailKnockback = 49143, // Helper->player/4D98, no cast, single-target
    CelestialTrailVisual3 = 49144, // HollowKing->self, no cast, single-target
    CelestialTrailExplosion = 49147, // Helper->self, 5.5s cast, range 60 circle
    HollowKingAutoVisual = 49180, // HollowKing->self, no cast, single-target
    HollowKingAuto = 49181, // HollowKingAutos->player, no cast, single-target
    EmptyProclamation = 49179, // HollowKing->self, 4.0s cast, range 60 circle
    RightSwordscrossVisual = 49151, // HollowKing->self, 8.0+1.0s cast, single-target
    LeftSwordscrossVisual = 49152, // HollowKing->self, 8.0+1.0s cast, single-target
    RightSwordscross1 = 49153, // Helper->self, 9.0s cast, range 60 width 30 rect
    LeftSwordscross1 = 49154, // Helper->self, 9.0s cast, range 60 width 30 rect
    RightSwordscross2 = 49155, // Helper->self, 9.0s cast, range 70 width 36 rect
    LeftSwordscross2 = 49156, // Helper->self, 9.0s cast, range 70 width 36 rect
    TwinBlazeVisual1 = 49157, // HollowKing->self, 5.0+1.0s cast, single-target
    TwinBlazeVisual2 = 49158, // HollowKing->self, 5.0+1.0s cast, single-target
    TwinBlazeIn = 49159, // Helper->self, 6.0s cast, range 20-60 donut
    TwinBlazeOut = 49160, // Helper->self, 6.0s cast, range 35 90-degree cone
    CataclysmicBladeVisual = 49161, // HollowKing->self, 7.0s cast, single-target
    CataclysmicBladeCone = 49162, // Helper->self, 7.0s cast, range 60 45-degree cone
    CataclysmicBladeFail = 49163, // Helper->player, no cast, single-target
    AtomicRayVisual = 49164, // HollowKing->self, 3.0s cast, single-target
    AtomicRay = 49165, // ArcaneSphere1/ArcaneSphere2->self, 1.5s cast, range 60 width 15 rect
    CosmicFlameVisual = 49166, // HollowKing->self, 5.0s cast, single-target
    CosmicFlameFirst = 49168, // Helper->self, 5.0s cast, range 6 circle
    CosmicFlameRest = 49169, // Helper->self, no cast, range 6 circle
    BurstVisual = 49170, // HollowKing->self, 3.0s cast, single-target
    Burst1 = 49171, // Helper->self, 5.0s cast, range 10 circle
    Burst2 = 49172, // Helper->self, 7.0s cast, range 10-20 donut
    Burst3 = 49173, // Helper->self, 9.0s cast, range 20-30 donut
    StarflareP2Cast = 49174, // HollowKing->self, 3.0s cast, single-target
    StarflareP2Fast = 49175, // Helper->self, 5.0s cast, range 60 width 10 rect
    StarflareP2Slow = 49176, // Helper->self, 7.0s cast, range 60 width 10 rect
    DarkNovaP2Visual = 49177, // HollowKing->self, 5.0s cast, single-target
    DarkNovaP2 = 49178, // Helper->players, no cast, range 6 circle
    SuperNovaVisual = 49182, // HollowKing->self, 5.0s cast, single-target
    SuperNova = 49183, // Helper->players, no cast, range 6 circle
}

public enum SID : uint
{
    Bleeding1 = 3077, // none->player, extra=0x0
    Bleeding2 = 3078, // none->player, extra=0x0
    CloakOfWaningLight = 5352, // none->player, extra=0x0
    CloakOfWaxingDark = 5353, // none->player, extra=0x0
    DownForTheCount = 1963, // Helper->player, extra=0xEC7
    Unk1 = 2202, // none->player, extra=0x0
    Unk3 = 2056, // none->_Gen_HollowKing/_Gen_ArcaneSphere1/_Gen_ArcaneSphere, extra=0x474/0x48E/0x497/0x496
    Unk4 = 2552, // none->player, extra=0x48F
    Clashing = 1271, // none->player, extra=0x317A/0x1836
    HPRecoveryDown = 2852, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    NoLook = 680, // player/Alxaal->self
    Look = 681, // player/Alxaal/Prishe->self
    NoMove = 682, // player/Prishe->self
    Move = 683, // player->self
    Checkmark = 136, // player/Alxaal/Prishe->self
    X = 137, // player->self
    Tankbuster = 344, // player->self
    Countdown = 720, // ArcaneSphere/ArcaneSphere1->self
    Stack = 305, // player->self
}

public enum TetherID : uint
{
    Tether_chn_fire001f = 5, // UnknownActor->HollowKing
}

// Puts AOE or Safezone over launchpad to avoid room wide aoe
abstract class FloorAOE(BossModule module, uint action) : Components.GenericAOEs(module, action)
{
    protected List<Actor> Casters = [];
    protected abstract int GetDangerFloor(int slot, Actor actor);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Casters)
        {
            var activation = Module.CastFinishAt(c.CastInfo);
            var danger = GetDangerFloor(slot, actor);
            var floor = Helpers.Level(actor);

            // By returning in each case directly instead of passing back a list
            // we update the AOE pattern as pc changes floors.
            if (danger == 1 && floor == 0)
                // stay away from launcher on lower floor to avoid danger on top floor.
                return new ReadOnlySpan<AOEInstance>([new AOEInstance(new AOEShapeCircle(2), Arena.Center - new WDir(0, 6), default, activation)]);
            else if (danger == 1 && floor == 1)
                // drop down to lower floor from top floor
                return new ReadOnlySpan<AOEInstance>([new AOEInstance(new AOEShapeDonut(6, 100), Arena.Center - new WDir(0, 6), default, activation)]);
            else if (danger == 0 && floor == 0)
                // hop up to top floor from lower floor
                return new ReadOnlySpan<AOEInstance>([new AOEInstance(new AOEShapeDonut(2, 100), Arena.Center - new WDir(0, 6), default, activation)]);
            else if (danger == 0 && floor == 1)
                // stay on top floor to avoid danger on lower floor
                return new ReadOnlySpan<AOEInstance>([new AOEInstance(new AOEShapeCircle(6), Arena.Center - new WDir(0, 6), default, activation)]);
        }
        return default;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            Casters.Remove(caster);
        }
    }
}

class CosmicBreath(BossModule module) : FloorAOE(module, (uint)AID.CosmicBreath)
{
    protected override int GetDangerFloor(int slot, Actor actor) => 1;
}

class CosmicTail(BossModule module) : FloorAOE(module, (uint)AID.CosmicTail)
{
    protected override int GetDangerFloor(int slot, Actor actor) => 0;
}

class AtomicTail(BossModule module) : FloorAOE(module, (uint)AID.AtomicTail)
{
    protected override int GetDangerFloor(int slot, Actor actor) => 0;
}

class AtomicTailArena(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00200010)
        {
            Shape[] arenaOutline = [new Rectangle(Arena.Center, 30f, 20f)];
            // Set a boundary to keep pc from jumping down into atomic tail.
            Shape[] circleOfDanger = [new Circle(Arena.Center - new WDir(0, 6), 6)];

            // Take the arena rectangle and give it a difference for the hole to prevent jumping down.
            ArenaBoundsCustom atomicTailArenaBounds = new(arenaOutline, circleOfDanger);
            Arena.Bounds = atomicTailArenaBounds;
        }

        if (index == 0x00 && state == 0x02000100)
            Arena.Bounds = new ArenaBoundsRect(30, 20);
    }
}

class TwilightNebula(BossModule module) : FloorAOE(module, (uint)AID.TwilightNebula1)
{
    readonly int[] colors = Utils.MakeArray(PartyState.MaxAllies, -1);

    protected override int GetDangerFloor(int slot, Actor actor)
    {
        var x = 1 - colors[slot];
        return x > 1 ? -1 : x;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.CloakOfWaningLight:
                var s1 = Raid.FindSlot(actor.InstanceID);
                if (s1 >= 0)
                    colors[s1] = 1;
                break;
            case SID.CloakOfWaxingDark:
                var s2 = Raid.FindSlot(actor.InstanceID);
                if (s2 >= 0)
                    colors[s2] = 0;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TwilightRadiance)
        {
            NumCasts++;
            Casters.Clear();
        }
    }
}

// Starflare: Two sets of crisscrossing line AoE telegraphs, hitting both levels at once.
// It's possible the pattern is the same on both floors. If so, we do not need to mess with floor checks.
// Works fine in my logs where the patterns match. Needs verification as to whether there is a non-matching pattern
// between floors.
class StarflareTimeGroups(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module,
    [(uint)AID.StarflareP1Fast, (uint)AID.StarflareP1Slow], new AOEShapeRect(60, 5), expectedNumCasters: 5);

// Icon to look at the boss
class VortexLook(BossModule module) : Components.GenericGaze(module)
{
    private DateTime _activation;
    private readonly List<Actor> _affected = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = _affected.Count;

        if (count == 0 || WorldState.CurrentTime < _activation.AddSeconds(-10d))
            return [];
        var eyes = new Eye[count];
        for (var i = 0; i < count; ++i)
            // inverted: true to look towards the eye.
            eyes[i] = new(_affected[i].Position, _activation, inverted: true);
        return eyes;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Look)
        {
            _activation = WorldState.FutureTime(7);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CataclysmicVortexVisual1 or AID.CataclysmicBladeVisual)
            _affected.Clear();
    }
}

// Icon to look away from the boss
class VortexNoLook(BossModule module) : Components.GenericGaze(module)
{
    private DateTime _activation;
    private readonly List<Actor> _affected = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = _affected.Count;

        if (count == 0 || WorldState.CurrentTime < _activation.AddSeconds(-10d))
            return [];
        var eyes = new Eye[count];
        for (var i = 0; i < count; ++i)
            eyes[i] = new(_affected[i].Position, _activation);
        return eyes;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.NoLook)
        {
            _activation = WorldState.FutureTime(7);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CataclysmicVortexVisual1 or AID.CataclysmicBladeVisual)
            _affected.Clear();
    }
}

class VortexStayMove(BossModule module) : Components.StayMove(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        Requirement r;
        switch ((IconID)iconID)
        {
            case IconID.NoMove:
                r = Requirement.Stay;
                break;
            case IconID.Move:
                r = Requirement.Move;
                break;
            default:
                return;
        }

        var p = Raid.FindSlot(actor.InstanceID);
        if (p >= 0)
        {
            var state = new PlayerState(r, WorldState.FutureTime(7));
            SetState(p, in state);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CataclysmicVortexVisual1 or AID.CataclysmicBladeVisual)
            Array.Fill(PlayerStates, default);
    }
}

class UpDownCounter(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.CosmicBreath, (uint)AID.CosmicTail]);

// There are more spells for DarkNova p2, I haven't seen them yet.  Will add if they are confirmed with a log.
class DarkNova(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Tankbuster, (uint)AID.DarkNova, centerAtTarget: true);

/*
 *  Spawns eight towers at the north. Tower will appear red (if empty) or blue (if taken).
 * One random player in each tower will be temporarily incapacitated with Clashing then take
 * heavy damage, receive a 20-second HP Recovery Down debuff, and be knocked back slightly.
 * Another set of eight towers will then spawn and should be taken by players without the debuff.
 */
class CelestialTrail(BossModule module) : Components.CastTowers(module, (uint)AID.CelestialTrailTower, 2, 1, 10)
{
    BitMask _forbidden;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.HPRecoveryDown)
            _forbidden.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            for (var i = 0; i < Towers.Count; i++)
                Towers.Ref(i).ForbiddenSoakers = _forbidden;
        }
    }
}

class EmptyProclamation(BossModule module) : Components.RaidwideCast(module, (uint)AID.EmptyProclamation);
class Swordscross1(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightSwordscross1, (uint)AID.LeftSwordscross1], new AOEShapeRect(60, 15));
class Swordscross2(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightSwordscross2, (uint)AID.LeftSwordscross2], new AOEShapeRect(70, 18));
class TwinBlaze1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwinBlazeIn, new AOEShapeDonutSector(20, 60, 45.Degrees()));
class TwinBlaze2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwinBlazeOut, new AOEShapeCone(35, 45.Degrees()));
class CataclysmicBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CataclysmicBladeCone, new AOEShapeCone(60, 22.5f.Degrees()));

class Burst(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Burst1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (uint)(AID)spell.Action.ID switch
        {
            (uint)AID.Burst1 => 0,
            (uint)AID.Burst2 => 1,
            (uint)AID.Burst3 => 2,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
    }
}

class CosmicFlame(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.CosmicFlameFirst)
            Lines.Add(new(caster.Position, caster.Rotation.ToDirection() * 8, Module.CastFinishAt(spell), 2.1f, 8, 3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CosmicFlameFirst or AID.CosmicFlameRest)
        {
            NumCasts++;
            var l = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (l >= 0)
                AdvanceLine(Lines[l], caster.Position);
        }
    }
}

// this is the one with two moving rectangles
// rectangles are not being animated here.  The ArcaneSphere1 and ArcaneSphere2 hang out the entire
// fight.  If we draw them they show up from the very start.
class AtomicRay(BossModule module) : Components.GenericAOEs(module, (uint)AID.AtomicRay)
{
    readonly List<(Actor Caster, WPos StartPos, WDir StartMove)> _recorded = [];
    readonly List<(Actor Caster, WPos Predicted, DateTime Activation)> _predicted = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _predicted.Count;
        if (count == 0)
        {
            return [];
        }

        for (var i = 0; i < count; i++)
        {
            var c = _predicted[i].Caster;
            var p = _predicted[i].Predicted;
            var d = _predicted[i].Activation;
            _aoes.Add(new AOEInstance(new AOEShapeRect(60, 7.5f), p, c.Rotation, d));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _predicted.RemoveAll(p => p.Caster == caster);
            var ix = _recorded.FindIndex(p => p.Caster == caster);
            if (ix >= 0)
            {
                var (p, s, m) = _recorded[ix];
                _recorded.RemoveAt(ix);
                ReportError($"{p} casting at {caster.Position}, starting was {s} going {m}");
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Countdown)
        {
            _recorded.Add((actor, actor.Position, actor.LastFrameMovement));
            var dt = WorldState.FutureTime(5.3f);

            if (actor.Position.AlmostEqual(new(800, -840), 2) && actor.LastFrameMovement.X > 0)
                _predicted.Add((actor, new(842.5f, -840), dt));
            else if (actor.Position.AlmostEqual(new(826, -840), 2) && actor.LastFrameMovement.X < 0)
                _predicted.Add((actor, new(812.5f, -840), dt));
            else if (actor.Position.AlmostEqual(new(850, -820), 2))
                _predicted.Add((actor, new(850, actor.LastFrameMovement.Z < 0 ? -806 : -834), dt));
            else if (actor.Position.AlmostEqual(new(850, -832), 2))
                _predicted.Add((actor, new(850, -820), dt));
            else if (actor.Position.AlmostEqual(new(814, -840), 2) && actor.LastFrameMovement.X > 0)
                _predicted.Add((actor, new(827.5f, -840), dt));
            else
                ReportError($"not sure what to predict for orb at {actor.Position} with movement {actor.LastFrameMovement}");
        }
    }
}

class AtomicRayCast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AtomicRay, new AOEShapeRect(60, 7.5f));

class GyreCharge(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.AtomicTailVisual1, (uint)AID.GyreCharge, 6.3f);

class SuperNova(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, (uint)AID.SuperNova, 6, 6.1f, minStackSize: 8)
{
    public int NumCasts { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {

        if (spell.Action.ID == StackAction)
        {
            NumCasts++;
            if (NumCasts >= 3)
            {
                Stacks.Clear();
                NumFinishedStacks++;
            }
        }
    }
}

class StarflareTimeGroupsP2(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.StarflareP2Fast, (uint)AID.StarflareP2Slow], new AOEShapeRect(60, 5), expectedNumCasters: 5);

[SkipLocalsInit]
sealed class ShinryuParadoxStates : StateMachineBuilder
{

    public ShinryuParadoxStates(A35ShinryuParadox module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CosmicBreath>()
            .ActivateOnEnter<CosmicTail>()
            .ActivateOnEnter<AtomicTail>()
            .ActivateOnEnter<AtomicTailArena>()
            .ActivateOnEnter<TwilightNebula>()
            .ActivateOnEnter<StarflareTimeGroups>()
            .ActivateOnEnter<VortexStayMove>()
            .ActivateOnEnter<VortexLook>()
            .ActivateOnEnter<VortexNoLook>()
            .ActivateOnEnter<UpDownCounter>()
            .ActivateOnEnter<DarkNova>()
            .ActivateOnEnter<CelestialTrail>()
            .ActivateOnEnter<EmptyProclamation>()
            .ActivateOnEnter<Swordscross1>()
            .ActivateOnEnter<Swordscross2>()
            .ActivateOnEnter<TwinBlaze1>()
            .ActivateOnEnter<TwinBlaze2>()
            .ActivateOnEnter<CataclysmicBlade>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<CosmicFlame>()
            .ActivateOnEnter<AtomicRay>()
            .ActivateOnEnter<GyreCharge>()
            .ActivateOnEnter<AtomicRayCast>()
            .ActivateOnEnter<SuperNova>()
            .ActivateOnEnter<StarflareTimeGroupsP2>()

            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies((uint)OID.HollowKing).All(k => k.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(ShinryuParadoxStates),
    ConfigType = null, // replace null with typeof(ShinryuParadoxConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.ShinryuParadox,
    Contributors = "Xan, ported by wen",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Alliance,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1117u,
    NameID = 14729u,
    SortOrder = 6,
    PlanLevel = 0)]

// Set up base logic for what level of arena and which phase boss pc is fighting.
public class A35ShinryuParadox(WorldState ws, Actor primary)
    : BossModule(ws, primary, new(820f, -820f), new ArenaBoundsRect(30f, 20f))
{
    Actor? Groin;
    Actor? _bossP2;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.HollowKing), Colors.Enemy);
    }

    protected override void UpdateModule()
    {
        Groin ??= Enemies((uint)OID.ShinryuGroin).FirstOrDefault();
        _bossP2 ??= Enemies((uint)OID.HollowKing).FirstOrDefault();
    }

    // If we are on the 0 level we fight the tail.
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment,
        AIHints hints)
    {
        var pBoss = 0;
        var pTail = AIHints.Enemy.PriorityInvincible;
        if (Helpers.Level(actor) == 0)
            (pTail, pBoss) = (pBoss, pTail);

        hints.SetPriority(PrimaryActor, pBoss);
        hints.SetPriority(Groin, pTail);
    }
}

// Helpers.Level is shorthand for which level of the arena pc is on.
// Arena itself is the same base shape with same center.  We do not have to
// change arena on radar during fight, just need to reference which level we are on
static class Helpers
{
    public static int Level(Actor pc) => Level(pc.PosRot);

    public static int Level(Vector4 p) => p.Y < -890 ? 0 : 1;
}
