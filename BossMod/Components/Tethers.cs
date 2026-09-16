namespace BossMod.Components;

// generic component for tankbuster at tethered targets; tanks are supposed to intercept tethers and gtfo from the raid
public class TankbusterTether(BossModule module, uint aid, uint tetherID, AOEShape shape, double activationDelay = default, bool centerAtTarget = false, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false) : CastCounter(module, aid)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    public TankbusterTether(BossModule module, uint aid, uint tetherID, float radius, double activationDelay = default, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false) : this(module, aid, tetherID, new AOEShapeCircle(radius), activationDelay, true, arenaProjectionLayer, restrictToArenaProjectionLayer) { }
    public readonly uint TID = tetherID;
    public readonly AOEShape Shape = shape;
    protected readonly List<(Actor Player, Actor Enemy)> _tethers = [];
    protected BitMask _tetheredPlayers;
    private BitMask _inAnyAOE; // players hit by aoe, excluding selves
    protected DateTime activation;

    public bool Active => _tetheredPlayers != default && (RestrictToArenaProjectionLayer != true || _tethers.Exists(t => ArenaProjectionLayerParticipantApplies(t.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer)));

    public override void Update()
    {
        _inAnyAOE = default;

        var count = _tethers.Count;
        if (count == 0)
        {
            return;
        }

        var party = Raid.WithSlot();
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var p = ref party[i];
            for (var j = 0; j < count; ++j)
            {
                var t = _tethers[j];
                if (t.Player == p.Item2 || !ArenaProjectionLayerParticipantApplies(t.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer) || !ArenaProjectionLayerParticipantApplies(p.Item2, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    continue;
                }

                var playerPos = p.Item2.Position;
                var tetherPos = t.Player.Position;
                var enemyPos = t.Enemy.Position;
                if (Shape.Check(playerPos, centerAtTarget ? tetherPos : enemyPos, centerAtTarget ? default : Angle.FromDirection(tetherPos - enemyPos)))
                {
                    _inAnyAOE[p.Item1] = true;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        if (!Active)
        {
            return;
        }

        if (actor.Role == Role.Tank)
        {
            if (!_tetheredPlayers[slot])
            {
                hints.Add("Grab the tether!");
                return;
            }
            var party = Raid.WithoutSlot();
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p == actor || !ArenaProjectionLayerParticipantApplies(p, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    continue;
                }

                var count = _tethers.Count;
                for (var j = 0; j < count; ++j)
                {
                    var t = _tethers[j];
                    if (t.Player == actor || !ArenaProjectionLayerParticipantApplies(t.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                    {
                        continue;
                    }

                    var playerPos = p.Position;
                    var enemyPos = t.Enemy.Position;
                    if (Shape.Check(playerPos, centerAtTarget ? playerPos : enemyPos, centerAtTarget ? default : Angle.FromDirection(playerPos - enemyPos)))
                    {
                        hints.Add("GTFO from raid!");
                        return;
                    }
                }
            }
        }
        else
        {
            if (_tetheredPlayers[slot])
            {
                hints.Add("Hit by tankbuster");
            }
            if (_inAnyAOE[slot])
            {
                hints.Add("GTFO from tankbuster!");
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (!ArenaProjectionLayerApplies(pc, ArenaProjectionLayer, RestrictToArenaProjectionLayer) || !ArenaProjectionLayerParticipantApplies(player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return PlayerPriority.Irrelevant;
        }
        if (_tetheredPlayers[playerSlot])
        {
            return PlayerPriority.Danger;
        }

        // for tanks, other players are interesting, since tank should not clip them
        if (pc.Role == Role.Tank)
        {
            return _inAnyAOE[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;
        }

        // for non-tanks, other players are irrelevant
        return PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        using var projection = Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer);
        // show tethered targets with circles
        var count = _tethers.Count;
        for (var i = 0; i < count; ++i)
        {
            var side = _tethers[i];
            if (!ArenaProjectionLayerParticipantApplies(side.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }
            var playerPos = side.Player.Position;
            var enemyPos = side.Enemy.Position;
            Arena.AddLine(enemyPos, playerPos, side.Player.Role == Role.Tank ? Colors.Safe : default);
            if (side.Player != pc)
            {
                continue;
            }

            Shape.Outline(Arena, centerAtTarget ? playerPos : enemyPos, centerAtTarget ? default : Angle.FromDirection(playerPos - enemyPos));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        using var projection = Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer);
        // show tethered targets with circles
        var count = _tethers.Count;
        for (var i = 0; i < count; ++i)
        {
            var side = _tethers[i];
            if (!ArenaProjectionLayerParticipantApplies(side.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }
            if (side.Player == pc)
            {
                continue;
            }

            var playerPos = side.Player.Position;
            var enemyPos = side.Enemy.Position;
            Shape.Draw(Arena, centerAtTarget ? playerPos : enemyPos, centerAtTarget ? default : Angle.FromDirection(playerPos - enemyPos));
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides is (int, Actor, Actor) side)
        {
            _tethers.Add((side.Player, side.Enemy));
            _tetheredPlayers[side.PlayerSlot] = true;
            if (activation == default)
            {
                activation = WorldState.FutureTime(activationDelay);
            }
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides is (int, Actor, Actor) side)
        {
            _tethers.Remove((side.Player, side.Enemy));
            _tetheredPlayers[side.PlayerSlot] = false;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            activation = default;
        }
    }

    // we support both player->enemy and enemy->player tethers
    private (int PlayerSlot, Actor Player, Actor Enemy)? DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
        {
            return null;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return null;
        }

        var (player, enemy) = Raid.WithoutSlot().Contains(source) ? (source, target) : (target, source);
        var playerSlot = Raid.FindSlot(player.InstanceID);
        return (playerSlot, player, enemy);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        if (_tetheredPlayers == default)
        {
            return;
        }

        var count = _tethers.Count;
        var party = Raid.WithoutSlot();
        var len = party.Length;
        for (var i = 0; i < count; ++i)
        {
            var t = _tethers[i];
            if (!ArenaProjectionLayerParticipantApplies(t.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }
            var playerPos = t.Player.Position;
            var enemyPos = t.Enemy.Position;
            if (t.Player != actor)
            {
                hints.AddForbiddenZone(Shape, centerAtTarget ? playerPos : enemyPos, centerAtTarget ? default : Angle.FromDirection(playerPos - enemyPos), activation, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
            }
            else if (t.Player.Role == Role.Tank) // avoid non tanks trying to dodge tanks...
            {
                for (var j = 0; j < len; ++j)
                {
                    var p = party[j];
                    if (p == t.Player || !ArenaProjectionLayerParticipantApplies(p, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                    {
                        continue;
                    }
                    var pos = p.Position;
                    switch (Shape)
                    {
                        case AOEShapeDonut:
                        case AOEShapeCircle:
                            hints.AddForbiddenZone(Shape, pos, default, activation, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
                            break;
                        case AOEShapeCone cone:
                            hints.AddForbiddenZone(new SDCone(enemyPos, 100f, Angle.FromDirection(pos - enemyPos), cone.HalfAngle), activation, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
                            break;
                        case AOEShapeRect rect:
                            hints.AddForbiddenZone(new SDCone(enemyPos, 100f, Angle.FromDirection(pos - enemyPos), Angle.Asin(rect.HalfWidth / (pos - enemyPos).Length())), activation, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
                            break;
                    }
                }
            }
        }
        // TODO: add logic for AI to grab tethers
    }
}

// generic component for AOE at tethered targets; players are supposed to intercept tethers and gtfo from the raid
public class InterceptTetherAOE(BossModule module, uint aid, uint tetherID, float radius, uint[]? excludedAllies = null, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false) : CastCounter(module, aid)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    public readonly uint[]? ExcludedAllies = excludedAllies;
    public readonly uint TID = tetherID;
    public readonly float Radius = radius;
    public readonly List<(Actor Player, Actor Enemy)> Tethers = [];
    private BitMask _tetheredPlayers;
    private BitMask _inAnyAOE; // players hit by aoe, excluding selves
    public DateTime Activation;

    public bool Active => Tethers.Count != 0 && (RestrictToArenaProjectionLayer != true || Tethers.Exists(t => ArenaProjectionLayerParticipantApplies(t.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer)));

    public override void Update()
    {
        _inAnyAOE = default;
        var raid = Raid.WithSlot();
        var len = raid.Length;
        for (var j = 0; j < len; ++j)
        {
            if (_tetheredPlayers[j])
            {
                var target = Raid[j];
                if (target != null && ArenaProjectionLayerParticipantApplies(target, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    for (var i = 0; i < len; ++i)
                    {
                        var p = raid[i];
                        var actor = p.Item2;
                        if (target == actor)
                        {
                            continue;
                        }
                        if (actor.Position.InCircle(target.Position, Radius))
                        {
                            if (ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                            {
                                _inAnyAOE.Set(p.Item1);
                            }
                        }
                    }
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        if (!Active)
        {
            return;
        }

        if (!_tetheredPlayers[slot])
        {
            hints.Add("Grab the tether!");
            return;
        }
        var party = Raid.WithoutSlot();
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            if (p == actor || !ArenaProjectionLayerParticipantApplies(p, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }

            if (p.Position.InCircle(actor.Position, Radius))
            {
                hints.Add("GTFO from raid!");
                break;
            }
        }

        if (_tetheredPlayers[slot])
        {
            hints.Add("Hit by baited AOE");
        }
        if (_inAnyAOE[slot])
        {
            hints.Add("GTFO from baited AOE!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        var count = Tethers.Count;
        if (count == 0)
        {
            return;
        }

        var raid = Raid.WithoutSlot();
        for (var i = 0; i < count; ++i)
        {
            var tether = Tethers[i];
            if (!ArenaProjectionLayerParticipantApplies(tether.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }
            if (tether.Player != actor)
            {
                hints.AddForbiddenZone(new SDCircle(tether.Player.Position, Radius), Activation, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
            }
            else
            {
                for (var j = 0; j < raid.Length; ++j)
                {
                    ref var member = ref raid[j];
                    if (member != actor && ArenaProjectionLayerParticipantApplies(member, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                    {
                        hints.AddForbiddenZone(new SDCircle(member.Position, Radius), Activation, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
                    }
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        using var projection = Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer);
        // show tethered targets with circles
        var count = Tethers.Count;
        if (count == 0)
        {
            return;
        }

        var len = ExcludedAllies?.Length;
        var exclude = new List<Actor>(len ?? 0);
        if (ExcludedAllies != null)
        {
            for (var i = 0; i < len; ++i)
            {
                exclude.AddRange(Module.Enemies(ExcludedAllies[i]));
            }
        }

        for (var i = 0; i < count; ++i)
        {
            var side = Tethers[i];
            if (!ArenaProjectionLayerParticipantApplies(side.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }
            var inParty = false;
            foreach (var a in Raid.WithoutSlot())
            {
                if (a == side.Player && !exclude.Contains(a))
                {
                    inParty = true;
                    break;
                }
            }

            Arena.AddLine(side.Enemy.Position, side.Player.Position, inParty ? Colors.Safe : default);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            Tethers.Add((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Set(sides.Value.PlayerSlot);
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            Tethers.Remove((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Clear(sides.Value.PlayerSlot);
        }
    }

    // we support both player->enemy and enemy->player tethers
    private (int PlayerSlot, Actor Player, Actor Enemy)? DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
        {
            return null;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return null;
        }

        var (player, enemy) = Raid.WithoutSlot().Contains(source) ? (source, target) : (target, source);
        var playerSlot = Raid.FindSlot(player.InstanceID);
        return (playerSlot, player, enemy);
    }
}

// generic component for tethers that need to be intercepted eg. to prevent a boss from gaining buffs
public class InterceptTether(BossModule module, uint aid, uint tetherIDBad = 84u, uint tetherIDGood = 17u, uint[]? excludedAllies = null, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false) : CastCounter(module, aid)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    public readonly uint TIDGood = tetherIDGood;
    public readonly uint TIDBad = tetherIDBad;
    public readonly uint[]? ExcludedAllies = excludedAllies;
    protected readonly List<(Actor Player, Actor Enemy)> _tethers = [];
    protected BitMask _tetheredPlayers;
    protected const string hint = "Grab the tether!";
    public bool Active => _tethers.Count != 0 && (RestrictToArenaProjectionLayer != true || _tethers.Exists(t => ArenaProjectionLayerParticipantApplies(t.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer)));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        if (!Active)
        {
            return;
        }

        if (!_tetheredPlayers[slot])
        {
            hints.Add(hint);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        using var projection = Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer);
        if (!Active)
        {
            return;
        }

        var len = ExcludedAllies?.Length;
        var exclude = new List<Actor>(len ?? 0);
        if (ExcludedAllies != null)
        {
            for (var i = 0; i < len; ++i)
            {
                exclude.AddRange(Module.Enemies(ExcludedAllies[i]));
            }
        }

        var count = _tethers.Count;
        for (var i = 0; i < count; ++i)
        {
            var side = _tethers[i];
            if (!ArenaProjectionLayerParticipantApplies(side.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }
            var inParty2 = false;
            foreach (var a in Raid.WithoutSlot())
            {
                if (a == side.Player && !exclude.Contains(a))
                {
                    inParty2 = true;
                    break;
                }
            }

            Arena.AddLine(side.Enemy.Position, side.Player.Position, inParty2 ? Colors.Safe : default);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            _tethers.Add((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Set(sides.Value.PlayerSlot);
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            _tethers.Remove((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Clear(sides.Value.PlayerSlot);
        }
    }

    public virtual (int PlayerSlot, Actor Player, Actor Enemy)? DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TIDGood && tether.ID != TIDBad)
        {
            return null;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return null;
        }

        var (player, enemy) = Raid.WithoutSlot().Contains(source) ? (source, target) : (target, source);
        var playerSlot = Raid.FindSlot(player.InstanceID);
        return (playerSlot, player, enemy);
    }
}

// generic component for tethers that need to be stretched and switch between a "good" and "bad" tether
// at the end of the mechanic various things are possible, eg. single target dmg, knockback/pull, AOE etc.
public class StretchTetherDuo(BossModule module, float minimumDistance, double activationDelay, uint tetherIDBad = 57u, uint tetherIDGood = 1u, AOEShape? shape = null, uint aid = default, uint enemyOID = default, bool knockbackImmunity = false, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : GenericBaitAway(module, aid, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    public readonly AOEShape? Shape = shape;
    public readonly uint TIDGood = tetherIDGood;
    public readonly uint TIDBad = tetherIDBad;
    public readonly float MinimumDistance = minimumDistance;
    private readonly float minSq = minimumDistance * minimumDistance;
    public readonly bool KnockbackImmunity = knockbackImmunity;
    public readonly List<Actor> _enemies = module.Enemies(enemyOID);
    public readonly List<(Actor, uint)> TetherOnActor = [];
    public readonly List<(Actor, DateTime)> ActivationDelayOnActor = [];
    public readonly double ActivationDelay = activationDelay;
    public const string HintGood = "Tether is stretched!";
    public const string HintBad = "Stretch tether further!";
    public const string HintKnockbackImmmunityGood = "Immune against tether mechanic!";
    public const string HintKnockbackImmmunityBad = "Tether can be ignored with knockback immunity!";

    protected struct PlayerImmuneState
    {
        public DateTime RoleBuffExpire; // 0 if not active
        public DateTime JobBuffExpire; // 0 if not active
        public DateTime DutyBuffExpire; // 0 if not active

        public readonly bool ImmuneAt(DateTime time) => RoleBuffExpire > time || JobBuffExpire > time || DutyBuffExpire > time;
    }

    protected PlayerImmuneState[] PlayerImmunes = new PlayerImmuneState[PartyState.MaxAllies];

    public bool IsImmune(int slot, DateTime time) => KnockbackImmunity && PlayerImmunes[slot].ImmuneAt(time);

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
        {
            switch (status.ID)
            {
                case 3054u: //Guard in PVP
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    PlayerImmunes[slot].RoleBuffExpire = status.ExpireAt;
                    break;
                case 1722u: //Bluemage Diamondback
                case (uint)WAR.SID.InnerStrength:
                    PlayerImmunes[slot].JobBuffExpire = status.ExpireAt;
                    break;
                case 2345u: //Lost Manawall in Bozja
                    PlayerImmunes[slot].DutyBuffExpire = status.ExpireAt;
                    break;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
        {
            switch (status.ID)
            {
                case 3054u: //Guard in PVP
                case (uint)WHM.SID.Surecast:
                case (uint)WAR.SID.ArmsLength:
                    PlayerImmunes[slot].RoleBuffExpire = default;
                    break;
                case 1722u: //Bluemage Diamondback
                case (uint)WAR.SID.InnerStrength:
                    PlayerImmunes[slot].JobBuffExpire = default;
                    break;
                case 2345u: //Lost Manawall in Bozja
                    PlayerImmunes[slot].DutyBuffExpire = default;
                    break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var baits = ActiveBaitsOn(pc);

        if (baits.Count == 0)
        {
            return;
        }

        if (!IsImmune(pcSlot, baits.Ref(0).Activation))
        {
            if (IsTether(pc, TIDBad))
            {
                DrawTetherLines(pc);
            }
            else if (IsTether(pc, TIDGood))
            {
                DrawTetherLines(pc, Colors.Safe);
            }
        }
    }

    protected bool IsTether(Actor actor, uint tetherID) => TetherOnActor.Contains((actor, tetherID));

    private void DrawTetherLines(Actor target, uint color = default)
    {
        var count = CurrentBaits.Count;
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var bait = ref baits[i];
            if (bait.Target == target && BaitParticipantAppliesToArenaProjectionLayer(target, bait))
            {
                using (Arena.WorldProjectionLayer(bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer))
                {
                    Arena.AddLine(bait.Source.Position, bait.Target.Position, color);
                }
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null && (enemyOID == default || _enemies.Contains(source)))
        {
            var hasPlayer = false;
            for (var ai = 0; ai < ActivationDelayOnActor.Count; ++ai)
            {
                if (ActivationDelayOnActor[ai].Item1 == player)
                {
                    hasPlayer = true;
                    break;
                }
            }

            if (!hasPlayer)
            {
                ActivationDelayOnActor.Add((player, WorldState.FutureTime(ActivationDelay)));
            }

            DateTime playerActivation = default;
            for (var ai = 0; ai < ActivationDelayOnActor.Count; ++ai)
            {
                if (ActivationDelayOnActor[ai].Item1 == player)
                {
                    playerActivation = ActivationDelayOnActor[ai].Item2;
                    break;
                }
            }

            CurrentBaits.Add(new(enemy, player, Shape ?? new AOEShapeCircle(default), playerActivation, arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
            TetherOnActor.Add((player, tether.ID));
        }
    }

    public override void Update()
    {
        var count = ActivationDelayOnActor.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            var a = ActivationDelayOnActor[i];
            if (a.Item2.AddSeconds(1d) <= WorldState.CurrentTime)
            {
                ActivationDelayOnActor.RemoveAt(i);
            }
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            var removing = CurrentBaits.RemoveAll(b => b.Source == enemy && b.Target == player);
            var removed = TetherOnActor.Remove((WorldState.Actors.Find(tether.Target)!, tether.ID));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var baits = ActiveBaitsOn(actor);
        if (baits.Count == 0)
        {
            return;
        }
        ref var bait0 = ref baits.Ref(0);
        var immunity = IsImmune(slot, bait0.Activation);
        var dist = (bait0.Source.Position - actor.Position).LengthSq();
        if (immunity)
        {
            hints.Add(HintKnockbackImmmunityGood, false);
        }
        else if (dist < minSq && TetherOnActor.Contains((actor, TIDBad)))
        {
            hints.Add(HintBad);
        }
        else if (dist >= minSq || TetherOnActor.Contains((actor, TIDGood)))
        {
            hints.Add(HintGood, false);
        }
        if (KnockbackImmunity && !immunity)
        {
            hints.Add(HintKnockbackImmmunityBad);
        }
    }

    public (Actor? player, Actor? enemy) DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TIDGood && tether.ID != TIDBad)
        {
            return (null, null);
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return (null, null);
        }

        var (player, enemy) = Raid.WithoutSlot().Contains(source) ? (source, target) : (target, source);
        return (player, enemy);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaits.Count == 0)
        {
            return;
        }

        DateTime actorBaitActivation = default;
        for (var bi = 0; bi < ActiveBaits.Count; ++bi)
        {
            if (ActiveBaits[bi].Target == actor && BaitParticipantAppliesToArenaProjectionLayer(actor, ActiveBaits[bi]))
            {
                actorBaitActivation = ActiveBaits[bi].Activation;
                break;
            }
        }

        var immunity = IsImmune(slot, actorBaitActivation);
        var isImmune = immunity && KnockbackImmunity;
        var couldBeImmune = !immunity && KnockbackImmunity;
        var actorHasTimedBait = false;
        for (var ai = 0; ai < ActivationDelayOnActor.Count; ++ai)
        {
            if (ActivationDelayOnActor[ai].Item1 == actor && ActivationDelayOnActor[ai].Item2.AddSeconds(-6d) <= WorldState.CurrentTime)
            {
                actorHasTimedBait = true;
                break;
            }
        }

        if (couldBeImmune && actorHasTimedBait && IsBaitTarget(actor))
        {
            hints.ActionsToExecute.Push(ActionDefinitions.Armslength, actor, ActionQueue.Priority.High);
            hints.ActionsToExecute.Push(ActionDefinitions.Surecast, actor, ActionQueue.Priority.High);
        }
        if (Shape != null)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }

        if (!isImmune)
        {
            for (var bi = 0; bi < ActiveBaits.Count; ++bi)
            {
                var b = ActiveBaits[bi];
                if (b.Target == actor && BaitParticipantAppliesToArenaProjectionLayer(actor, b))
                {
                    hints.AddForbiddenZone(new SDCircle(b.Source.Position, MinimumDistance), b.Activation, arenaProjectionLayer: ArenaProjectionLayerForAI(b.ResolveArenaProjectionLayer(Module), b.RestrictToArenaProjectionLayer));
                }
            }
        }
    }
}

// generic component for tethers that need to be stretched
public class StretchTetherSingle(BossModule module, uint tetherID, float minimumDistance, AOEShape? shape = null, uint aid = default, uint enemyOID = default, double activationDelay = default, bool knockbackImmunity = false, bool needToKite = false, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) :
StretchTetherDuo(module, minimumDistance, activationDelay, tetherID, tetherID, shape, aid, enemyOID, knockbackImmunity, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaitsOn(actor).Count == 0)
        {
            return;
        }

        if (needToKite && TetherOnActor.Contains((actor, TIDBad)))
        {
            hints.Add("Kite the add!");
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (needToKite)
        {
            var baits = ActiveBaitsOn(pc);
            if (baits.Count != 0 && IsTether(pc, TIDBad))
            {
                Arena.Actor(baits.Ref(0).Source, Colors.Object, true);
            }
        }
    }
}

//generic component for Tethers that must be avoided if you have a status and intercepted if you don't
public class InterceptTetherStatus(BossModule module, uint aid, uint tetherID, uint sid, float radius = 0f, uint[]? excludedAllies = null, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false) : CastCounter(module, aid)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    public readonly uint[]? ExcludedAllies = excludedAllies;
    public readonly uint TID = tetherID;
    public readonly uint statusid = sid;
    public readonly float Radius = radius;
    public readonly List<(Actor Player, Actor Enemy)> Tethers = [];
    private BitMask _tetheredPlayers;
    private BitMask _inAnyAOE; // players hit by aoe, excluding selves
    private BitMask _hasStatus;
    public DateTime Activation;

    public bool Active => Tethers.Count != 0 && (RestrictToArenaProjectionLayer != true || Tethers.Exists(t => ArenaProjectionLayerParticipantApplies(t.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer)));

    public override void Update()
    {
        _inAnyAOE = default;
        var raid = Raid.WithSlot();
        var len = raid.Length;
        for (var j = 0; j < len; ++j)
        {
            if (_tetheredPlayers[j])
            {
                var target = Raid[j];
                if (target != null && ArenaProjectionLayerParticipantApplies(target, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    for (var i = 0; i < len; ++i)
                    {
                        var p = raid[i];
                        var actor = p.Item2;
                        if (target == actor)
                        {
                            continue;
                        }
                        if (actor.Position.InCircle(target.Position, Radius))
                        {
                            if (ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                            {
                                _inAnyAOE.Set(p.Item1);
                            }
                        }
                    }
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        if (!Active)
        {
            return;
        }

        if (!_tetheredPlayers[slot] && !_hasStatus[slot])
        {
            hints.Add("Grab the tether!");
            return;
        }
        var party = Raid.WithoutSlot();
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            if (p == actor || !ArenaProjectionLayerParticipantApplies(p, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }

            if (p.Position.InCircle(actor.Position, Radius))
            {
                hints.Add("GTFO from raid!");
                break;
            }
        }

        if (_tetheredPlayers[slot] && !_hasStatus[slot])
        {
            hints.Add("Hit by baited AOE");
        }

        if (_tetheredPlayers[slot] && _hasStatus[slot])
        {
            hints.Add("Give tether away!");
        }

        if (_inAnyAOE[slot])
        {
            hints.Add("GTFO from baited AOE!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        var count = Tethers.Count;
        if (count == 0)
        {
            return;
        }

        var raid = Raid.WithoutSlot();
        for (var i = 0; i < count; ++i)
        {
            var tether = Tethers[i];
            if (!ArenaProjectionLayerParticipantApplies(tether.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }
            if (tether.Player != actor)
            {
                hints.AddForbiddenZone(new SDCircle(tether.Player.Position, Radius), Activation, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
            }
            else
            {
                for (var j = 0; j < raid.Length; ++j)
                {
                    ref var member = ref raid[j];
                    if (member != actor && ArenaProjectionLayerParticipantApplies(member, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                    {
                        hints.AddForbiddenZone(new SDCircle(member.Position, Radius), Activation, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
                    }
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        using var projection = Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer);
        // show tethered targets with circles
        var count = Tethers.Count;
        if (count == 0)
        {
            return;
        }

        var len = ExcludedAllies?.Length;
        var exclude = new List<Actor>(len ?? 0);
        if (ExcludedAllies != null)
        {
            for (var i = 0; i < len; ++i)
            {
                exclude.AddRange(Module.Enemies(ExcludedAllies[i]));
            }
        }

        for (var i = 0; i < count; ++i)
        {
            var side = Tethers[i];
            if (!ArenaProjectionLayerParticipantApplies(side.Player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }
            Arena.AddLine(side.Enemy.Position, side.Player.Position, _hasStatus[Raid.FindSlot(side.Player.InstanceID)] ? Colors.Danger : Colors.Safe);
            Arena.ZoneCircleOutline(side.Player.Position, Radius);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            Tethers.Add((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Set(sides.Value.PlayerSlot);
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        var sides = DetermineTetherSides(source, tether);
        if (sides != null)
        {
            Tethers.Remove((sides.Value.Player, sides.Value.Enemy));
            _tetheredPlayers.Clear(sides.Value.PlayerSlot);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == statusid)
        {
            _hasStatus.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == statusid)
        {
            _hasStatus.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    // we support both player->enemy and enemy->player tethers
    private (int PlayerSlot, Actor Player, Actor Enemy)? DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
        {
            return null;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return null;
        }

        var (player, enemy) = Raid.WithoutSlot().Contains(source) ? (source, target) : (target, source);
        var playerSlot = Raid.FindSlot(player.InstanceID);
        return (playerSlot, player, enemy);
    }
    //TODO: AI logic for getting tethers -- some sort of priority for non-statused folks?
}
