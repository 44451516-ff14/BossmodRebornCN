namespace BossMod.Dawntrail.Ultimate.DMU;

class RevoltingRuinIII(BossModule module) : Components.GenericBaitAway(module, (uint)AID.RevoltingRuinIII, tankbuster: true,
    damageType: AIHints.PredictedDamageType.Tankbuster)
{

    private Actor? source; // If this ever becomes null, it means the mechanic has not started, or it has ended
    private Actor? target;
    private DateTime activation;
    private bool secondTB = false;

    public override void Update()
    {
        CurrentBaits.Clear();
        if (source == null)
        {
            return;
        }

        var target = secondTB ? RaidByEnmity(source, true).Skip(1).FirstOrDefault() : this.target;
        if (target != null)
        {
            CurrentBaits.Add(new(source, target, new AOEShapeCone(30f, 45f.Degrees()), activation));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RevoltingRuinIII)
        {
            source = caster;
            target = WorldState.Actors.Find(spell.TargetID);
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RevoltingRuinIII)
        {
            NumCasts++;
            secondTB = true;
            activation = WorldState.FutureTime(0.3f); // TODO random guess find the actual timing
        }

        if (spell.Action.ID == (uint)AID.RevoltingRuinIII1)
        {
            NumCasts++;
            source = null;
            CurrentBaits.Clear();
        }
    }
}

class GravenImage(BossModule module) : Components.GenericKnockback(module, (uint)AID.PulseWave)
{
    private readonly List<(ulong SourceID, int slot)> tethers = [];
    private readonly List<Knockback> knockbacks = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.GravenImageTether)
        {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return;
        }

        var slot = Raid.FindSlot(tether.Target);
        if (slot < 0)
        {
            return;
        }

        tethers.Add((source.InstanceID, slot));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PulseWave)
        {
            NumCasts++;
            tethers.Clear();
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        knockbacks.Clear();

        foreach (var target in tethers)
        {
            if (target.slot != slot)
            {
                continue;
            }

            var source = WorldState.Actors.Find(target.SourceID);
            if (source == null)
            {
                continue;
            }

            knockbacks.Add(new(source.Position, 14f, actorID: source.InstanceID));
            return CollectionsMarshal.AsSpan(knockbacks);
        }

        return CollectionsMarshal.AsSpan(knockbacks);
    }
}

class StackSpreadOrbs(BossModule module) : Components.UniformStackSpread(module, 6f, 5f, 4, 4)
{
    private bool? spread = null;
    private IconID? iconID = null;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.FlagrantFireIIIStack) or ((uint)AID.FlagrantFireIIISpread))
        {
            spread = null;
            iconID = null;
        }
    }

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();

        if (spread == null || iconID == null)
        {
            return;
        }

        // We do the opposite of whatever we are told
        if (iconID == IconID.FireRingQuestionMark)
        {
            if (spread == true)
            {
                var support = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsSupport());
                var dps = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsDD());

                if (support == null || dps == null)
                {
                    return;
                }

                AddStack(support);
                AddStack(dps);
            }

            if (spread == false)
            {
                AddSpreads(Raid.WithoutSlot(true, true, true));
            }
        }

        // We do what we are told
        if (iconID == IconID.FireRingBlueOrb)
        {
            if (spread == false)
            {
                var support = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsSupport());
                var dps = Raid.WithoutSlot(true, true, true).FirstOrDefault(p => p.Class.IsDD());

                if (support == null || dps == null)
                {
                    return;
                }

                AddStack(support);
                AddStack(dps);
            }

            if (spread == true)
            {
                AddSpreads(Raid.WithoutSlot(true, true, true));
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.spreadIcon)
        {
            spread = true;
        }

        if (iconID == (uint)IconID.stackIcon)
        {
            spread = false;
        }

        if (iconID == (uint)IconID.FireRingQuestionMark)
        {
            this.iconID = (IconID)iconID;
        }

        if (iconID == (uint)IconID.FireRingBlueOrb)
        {
            this.iconID = (IconID)iconID;
        }
    }
}

class BlizzardSafeSpots(BossModule module) : Components.GenericAOEs(module)
{
    private bool questionMark = false;
    private readonly List<(uint AID, AOEInstance AOE)> aoesAvailable = [];
    private readonly List<AOEInstance> aoes = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BlueRingQuestionMark)
        {
            questionMark = true;
        }

        if (iconID == (uint)IconID.BlueRingBlueOrb)
        {
            questionMark = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.BlizzardIIIBlowout) or
            ((uint)AID.BlizzardIIIBlowout1) or
            ((uint)AID.BlizzardIIIBlowout2))
        {
            aoesAvailable.Add((spell.Action.ID, new AOEInstance(new AOEShapeCone(40f, 45f.Degrees()), caster.Position, caster.Rotation, actorID: caster.InstanceID)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.BlizzardIIIBlowout) or
            ((uint)AID.BlizzardIIIBlowout1) or
            ((uint)AID.BlizzardIIIBlowout2))
        {
            NumCasts++;
            aoesAvailable.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        aoes.Clear();

        foreach (var currentAOE in aoesAvailable)
        {
            if (questionMark)
            {
                if (currentAOE.AID == (uint)AID.BlizzardIIIBlowout)
                {
                    aoes.Add(currentAOE.AOE);
                }
            }

            if (!questionMark)
            {
                if (currentAOE.AID is ((uint)AID.BlizzardIIIBlowout1) or ((uint)AID.BlizzardIIIBlowout2))
                {
                    aoes.Add(currentAOE.AOE);
                }
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

class WaveCannon(BossModule module) : Components.BaitAwayEveryone(module,
    module.Enemies((uint)OID.StatueBodyOrb).FirstOrDefault(), new AOEShapeRect(100f, 3f),
    (uint)AID.WaveCannon)
{

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WaveCannon)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

// TODO add priority system
class WaveCannonTowers(BossModule module) : Components.CastTowers(module, (uint)AID.TowerExplosion, 4)
{
    private readonly DateTime[] magicVulnerability = new DateTime[PartyState.MaxPartySize];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.MagicVulnerabilityUp)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                magicVulnerability[slot] = status.ExpireAt;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action.ID == WatchedAction)
        {
            for (var i = 0; i < Towers.Count; i++)
            {
                Towers.Ref(i).ForbiddenSoakers = Raid.WithSlot()
                    .WhereSlot(player => magicVulnerability[player] > Towers[i].Activation).Mask();
            }
        }
    }
}

class LightningSafeSpots(BossModule module) : Components.GenericAOEs(module)
{
    private bool questionMark = false;
    private readonly List<(uint AID, AOEInstance AOE)> aoesAvailable = [];
    private readonly List<AOEInstance> aoes = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.PurpleRingQuestionMark)
        {
            questionMark = true;
        }

        if (iconID == (uint)IconID.PurpleRingBlueOrb)
        {
            questionMark = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is ((uint)AID.ThrummingThunderIII) or
            ((uint)AID.ThrummingThunderIII1) or
            ((uint)AID.ThrummingThunderIII2))
        {
            aoesAvailable.Add((spell.Action.ID, new AOEInstance(new AOEShapeRect(40f, 5f), caster.Position, caster.Rotation, actorID: caster.InstanceID)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.ThrummingThunderIII) or
             ((uint)AID.ThrummingThunderIII1) or
             ((uint)AID.ThrummingThunderIII2))
        {
            NumCasts++;
            aoesAvailable.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        aoes.Clear();

        foreach (var currentAOE in aoesAvailable)
        {
            if (questionMark)
            {
                if (currentAOE.AID == (uint)AID.ThrummingThunderIII2)
                {
                    aoes.Add(currentAOE.AOE);
                }
            }

            if (!questionMark)
            {
                if (currentAOE.AID is ((uint)AID.ThrummingThunderIII) or ((uint)AID.ThrummingThunderIII1))
                {
                    aoes.Add(currentAOE.AOE);
                }
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

class DoubleTroubleTrapStacks(BossModule module) : Components.UniformStackSpread(module, 6f, 5f, 4, 4)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DoubleTroubleTrap)
        {
            AddStack(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DoubleTroubleTrap)
        {
            Stacks.RemoveAt(0);
        }
    }
}

class DoubleTroubleTrapKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.DoubleTroubleTrap1)
{
    private readonly List<Knockback> knockbacks = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        knockbacks.Clear();

        var stack = Module.FindComponent<DoubleTroubleTrapStacks>();
        if (stack == null)
        {
            return CollectionsMarshal.AsSpan(knockbacks);
        }

        foreach (var stackPoint in stack.Stacks)
        {
            if (actor.Position.InCircle(stackPoint.Target.Position, stackPoint.Radius))
            {
                knockbacks.Add(new(stackPoint.Target.Position, 14f, stackPoint.Activation, actorID: stackPoint.Target.InstanceID));
            }
        }

        return CollectionsMarshal.AsSpan(knockbacks);
    }
}

class HyperDrive(BossModule module) : Components.GenericBaitAway(module, (uint)AID.Hyperdrive, centerAtTarget: true, tankbuster: true,
    damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private DateTime? activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LightOfJudgment)
        {
            activation = Module.CastFinishAt(spell, 3.0f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;

            if (NumCasts >= 3)
            {
                activation = null;
            }
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (activation != null)
        {
            var target = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
            if (target != null)
            {
                CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeCircle(5), activation.Value));
            }
        }
    }
}

// TODO party config settings for display of green circles for stacks & spreads
class Gravitas(BossModule module) : Components.UniformStackSpread(module, 5, 5, 4, 4)
{
    private readonly List<Spread> spreadsIncoming = [];
    private int totalStacks = 0;
    public int NumCasts = 0;

    private enum TetherGroup { Support, DPS }
    private TetherGroup tetherGroup = TetherGroup.Support;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (Stacks.Count != 0 && totalStacks == 4)
        {
            Arena.AddCircle(Module.Center + new WDir(0, 19.5f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(Module.Center + new WDir(0, -19.5f), 1.0f, Colors.Safe, 2);
        }

        if (Stacks.Count != 0 && totalStacks == 8)
        {
            var puddles = Module.FindComponent<GravitasPuddles>();

            if (puddles == null)
            {
                return;
            }

            var puddleSources = puddles.puddles;

            var puddle1 = puddleSources.Where(p => p.actor.Position.Z > Module.Center.Z).Select(p => p.actor).MinBy(p => (p.Position - Module.Center).Length());
            if (puddle1 == null)
            {
                return;
            }

            var puddle2 = puddleSources.Where(p => p.actor.Position.Z < Module.Center.Z).Select(p => p.actor).MinBy(p => (p.Position - Module.Center).Length());
            if (puddle2 == null)
            {
                return;
            }

            Arena.AddCircle(puddle1.Position - (puddle1.Position - Module.Center).Normalized() * 5.5f, 1.0f, Colors.Safe, 2);
            Arena.AddCircle(puddle2.Position - (puddle2.Position - Module.Center).Normalized() * 5.5f, 1.0f, Colors.Safe, 2);
        }

        if (Spreads.Count != 0)
        {
            if (tetherGroup == TetherGroup.Support ? pc.Class.IsSupport() : tetherGroup == TetherGroup.DPS && pc.Class.IsDD())
            {
                // MT/M1 - [90.054, 0.000, 100.718, -178.033]
                Arena.AddCircle(new WPos(90.054f, 100.718f), 1.0f, Colors.Safe, 2);

                // H1/R1 - [87.971, 0.000, 86.119, 37.967]
                Arena.AddCircle(new WPos(87.971f, 86.119f), 1.0f, Colors.Safe, 2);

                // OT/M2 - [109.197, 0.000, 99.618, -87.793]
                Arena.AddCircle(new WPos(109.197f, 99.618f), 1.0f, Colors.Safe, 2);

                // H2/R2 - [113.566, 0.000, 112.415, -133.273]
                Arena.AddCircle(new WPos(113.566f, 112.415f), 1.0f, Colors.Safe, 2);
            }
            else
            {
                // Middle
                Arena.AddCircle(Module.Center, 1.0f, Colors.Safe, 2);
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.GravenImageTether)
        {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return;
        }

        var slot = Raid.FindSlot(tether.Target);
        if (slot < 0)
        {
            return;
        }

        if (source.Position.AlmostEqual(new(102.500f, 22.500f), 5))
        {
            AddStack(target, WorldState.FutureTime(6.5f));
            totalStacks++;
        }

        if (source.Position.AlmostEqual(new(126.000f, 41.500f), 5))
        {
            spreadsIncoming.Add(new(target, 5, WorldState.FutureTime(10.6f)));
            tetherGroup = target.Class.IsSupport() ? TetherGroup.Support : TetherGroup.DPS;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Gravitas && Stacks.Count > 0)
        {
            Stacks.RemoveAt(0);
            NumCasts++;
            Spreads.AddRange(spreadsIncoming);
            spreadsIncoming.Clear();
        }

        if (spell.Action.ID == (uint)AID.Vitrophyre && Spreads.Count > 0)
        {
            Spreads.RemoveAt(0);
        }
    }
}

class GravitasPuddles(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 5, _ => [], (uint)AID.Gravitas)
{
    public List<(Actor actor, uint colour)> puddles = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.PurplePuddles)
        {
            puddles.Add((actor, Colors.AOE));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.PurplePuddles && state == (uint)Animations.PuddleSoakReady)
        {
            var puddle = puddles.FindIndex(p => p.actor.InstanceID == actor.InstanceID);
            puddles[puddle] = (actor, Colors.SafeFromAOE);
        }

        if (actor.OID == (uint)OID.PurplePuddles && state == (uint)Animations.PuddleExplosion)
        {
            var puddle = puddles.FindIndex(p => p.actor.InstanceID == actor.InstanceID);
            if (puddle >= 0)
            {
                puddles.RemoveAt(puddle);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var puddle in puddles)
        {
            Shape.Draw(Arena, puddle.actor.Position, puddle.actor.Rotation, puddle.colour);
        }
    }
}

class GravitationalWave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EBFBD && state == (uint)Animations.PulseOrbStart)
        {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 20), Arena.Center, 90.Degrees(), actorID: actor.InstanceID));
        }

        if (actor.OID == 0x1EBFBC && state == (uint)Animations.PulseOrbStart)
        {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 20), Arena.Center, -90.Degrees(), actorID: actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.GravitationalWave) or ((uint)AID.IntemperateWill))
        {
            NumCasts++;
            aoes.Clear();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

/*
A1 MARK:
A - [93.696, 93.692]
1RIGHT - [94.219, 87.589]
ALEFT - [87.173, 93.588]
1 - [87.347, 87.392]

B2 MARK:
B - [106.246, 93.555]
BRIGHT - [112.405, 93.744]
2 - [112.704, 87.231]
2LEFT - [106.611, 87.215]

C3 MARK:
C - [106.397, 106.515]
3LEFT - [106.261, 112.634]
3 - [112.631, 112.776]
CRIGHT - [112.787, 106.504]

D4 MARK:
D - [93.761, 106.631]
DLEFT - [87.566, 106.333]
4 - [87.219, 112.880]
4RIGHT - [93.390, 112.900]
*/

// TODO make it so after the first one has gone off it removes the circle of the first one
// TODO make it so it leaves behind a puddle of the AOE
// TODO allow different configures
// TODO add support to allow different points of views for different players in the reply
//      Not needed as it only helps with the development and testing, just need to expand the variables to list and store all players debuffs instead
class TeleTrouncing(BossModule module) : BossComponent(module)
{
    public int NumCasts = 0;
    private (Direction direction, DateTime activation)? Debuff1;
    private (Direction direction, DateTime activation)? Debuff2;
    private enum Direction { UP, DOWN, LEFT, RIGHT }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TeleTrouncing1)
        {
            NumCasts++;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var player = Raid.FindSlot(actor.InstanceID);
        if (player is not (>= 0 and PartyState.PlayerSlot))
        {
            return;
        }

        Direction? dir = (SID)status.ID switch
        {
            SID.TelePortentUP or SID.TelePortentUP2 => Direction.UP,
            SID.TelePortentDOWN or SID.TelePortentDOWN2 => Direction.DOWN,
            SID.TelePortentLEFT or SID.TelePortentLEFT2 => Direction.LEFT,
            SID.TelePortentRIGHT or SID.TelePortentRIGHT2 => Direction.RIGHT,
            _ => null
        };

        if (dir == null)
        {
            return;
        }

        var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
        if (duration > 8)
        {
            Debuff2 = (dir.Value, status.ExpireAt);
        }
        else
        {
            Debuff1 = (dir.Value, status.ExpireAt);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts == 16)
        {
            return;
        }

        if (Debuff1 == null || Debuff2 == null)
        {
            return;
        }

        // Case 1: Both debuffs are in the same direction
        if (Debuff1.Value.direction == Debuff2.Value.direction)
        {
            if (Debuff1.Value.direction == Direction.UP)
            {
                Arena.AddCircle(new WPos(112.631f, 112.776f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(112.787f, 106.504f), 1.0f, Colors.AOE, 2);
            }

            if (Debuff1.Value.direction == Direction.DOWN)
            {
                Arena.AddCircle(new WPos(87.347f, 87.392f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(87.173f, 93.588f), 1.0f, Colors.AOE, 2);
            }

            if (Debuff1.Value.direction == Direction.LEFT)
            {
                Arena.AddCircle(new WPos(112.704f, 87.231f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(106.611f, 87.215f), 1.0f, Colors.AOE, 2);
            }

            if (Debuff1.Value.direction == Direction.RIGHT)
            {
                Arena.AddCircle(new WPos(87.219f, 112.880f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(93.390f, 112.900f), 1.0f, Colors.AOE, 2);
            }

            return;
        }

        // Case 2: Both debuffs are in different directions
        var debuff1First = Debuff1.Value.activation <= Debuff2.Value.activation;

        if ((Debuff1.Value.direction == Direction.UP || Debuff1.Value.direction == Direction.LEFT) &&
            (Debuff2.Value.direction == Direction.UP || Debuff2.Value.direction == Direction.LEFT))
        {

            var upFirst = Debuff1.Value.direction == Direction.UP ? debuff1First : !debuff1First;

            if (upFirst)
            {
                Arena.AddCircle(new WPos(93.696f, 93.692f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(94.219f, 87.589f), 1.0f, Colors.AOE, 2);
            }
            else
            {
                Arena.AddCircle(new WPos(94.219f, 87.589f), 1.0f, Colors.AOE, 2);
                Arena.AddCircle(new WPos(93.696f, 93.692f), 1.0f, Colors.Safe, 2);
            }
        }

        if ((Debuff1.Value.direction == Direction.UP || Debuff1.Value.direction == Direction.RIGHT) &&
            (Debuff2.Value.direction == Direction.UP || Debuff2.Value.direction == Direction.RIGHT))
        {

            var upFirst = Debuff1.Value.direction == Direction.UP ? debuff1First : !debuff1First;

            if (upFirst)
            {
                Arena.AddCircle(new WPos(106.246f, 93.555f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(112.405f, 93.744f), 1.0f, Colors.AOE, 2);
            }
            else
            {
                Arena.AddCircle(new WPos(112.405f, 93.744f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(106.246f, 93.555f), 1.0f, Colors.AOE, 2);
            }
        }

        if ((Debuff1.Value.direction == Direction.DOWN || Debuff1.Value.direction == Direction.LEFT) &&
            (Debuff2.Value.direction == Direction.DOWN || Debuff2.Value.direction == Direction.LEFT))
        {

            var downFirst = Debuff1.Value.direction == Direction.DOWN ? debuff1First : !debuff1First;

            if (downFirst)
            {
                Arena.AddCircle(new WPos(87.566f, 106.333f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(93.761f, 106.631f), 1.0f, Colors.AOE, 2);
            }
            else
            {
                Arena.AddCircle(new WPos(93.761f, 106.631f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(87.566f, 106.333f), 1.0f, Colors.AOE, 2);
            }
        }

        if ((Debuff1.Value.direction == Direction.DOWN || Debuff1.Value.direction == Direction.RIGHT) &&
            (Debuff2.Value.direction == Direction.DOWN || Debuff2.Value.direction == Direction.RIGHT))
        {

            var downFirst = Debuff1.Value.direction == Direction.DOWN ? debuff1First : !debuff1First;

            if (downFirst)
            {
                Arena.AddCircle(new WPos(106.397f, 106.515f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(106.261f, 112.634f), 1.0f, Colors.AOE, 2);
            }
            else
            {
                Arena.AddCircle(new WPos(106.261f, 112.634f), 1.0f, Colors.Safe, 2);
                Arena.AddCircle(new WPos(106.397f, 106.515f), 1.0f, Colors.AOE, 2);
            }
        }
    }
}

/*
Marker middle A - [89.393, 89.193]
Hitbox of A - [96.019, 96.796]

Marker middle B - [110.024, 89.869]
Hitbox of B - [103.378, 95.483]

Marker middle C - [109.500, 109.772]
Hitbox of C - [104.167, 103.239]

Markers middle D - [89.765, 110.050]
Hitbox of D - [96.912, 104.158]
 */
class GravenImage2(BossModule module) : Components.UniformStackSpread(module, 5, 5, 1, 1)
{
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.GravenImageTether)
        {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return;
        }

        if (source.Position.AlmostEqual(new(107.000f, 43.000f), 5))
        {
            AddSpread(target, WorldState.FutureTime(6.5f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.IdyllicWill)
        {
            Spreads.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (pc.Class.GetRole() == Role.Tank)
        {
            Arena.AddCircle(new WPos(96.019f, 89.193f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(new WPos(103.378f, 95.483f), 1.0f, Colors.Safe, 2);
        }

        if (pc.Class.GetRole() == Role.Healer)
        {
            Arena.AddCircle(new WPos(109.500f, 109.772f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(new WPos(89.765f, 110.050f), 1.0f, Colors.Safe, 2);
        }

        if (pc.Class.GetRole() == Role.Melee)
        {
            Arena.AddCircle(new WPos(104.167f, 103.239f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(new WPos(96.912f, 104.158f), 1.0f, Colors.Safe, 2);
        }

        if (pc.Class.GetRole() == Role.Ranged)
        {
            Arena.AddCircle(new WPos(89.393f, 89.193f), 1.0f, Colors.Safe, 2);
            Arena.AddCircle(new WPos(110.024f, 89.869f), 1.0f, Colors.Safe, 2);
        }
    }
}

class Gaze(BossModule module) : Components.GenericGaze(module)
{
    private Actor? eye;
    private bool inverted = false;
    private readonly List<Eye> eyeAoe = [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.StatuePurpleEye && state == (uint)Animations.EyeStart)
        {
            eye = actor;
            inverted = false;
        }

        if (actor.OID == (uint)OID.StatueYellowEye && state == (uint)Animations.EyeStart)
        {
            eye = actor;
            inverted = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is ((uint)AID.IndolentWill) or ((uint)AID.AveMaria))
        {
            NumCasts++;
            eye = null;
        }
    }

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        eyeAoe.Clear();

        if (eye == null)
        {
            return CollectionsMarshal.AsSpan(eyeAoe);
        }

        eyeAoe.Add(new Eye(eye.Position, inverted: inverted));
        return CollectionsMarshal.AsSpan(eyeAoe);
    }
}
