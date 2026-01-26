namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class AssaultEvolvedSword(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;
    private WPos _dashPos;
    private Angle _crossRotation;
    private readonly List<Actor> _healers = [];
    private readonly List<AOEInstance> _aoes = [];

    // Reborn-native shapes
    private static readonly AOEShapeCross Cross = new(40f, 5f);
    private static readonly AOEShapeRect HealerCone = new(60f, 2f);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AssaultEvolved_SwordDash)
            Arm(spell.TargetXZ);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo cast)
    {
        // Helper resolves the mechanic
        if (_active && (AID)cast.Action.ID == AID.AssaultEvolved_SwordCross)
            Clear();
    }

    private void Arm(WPos dashPos)
    {
        _active = true;
        _aoes.Clear();
        _healers.Clear();
        _dashPos = dashPos;
        var boss = Module.PrimaryActor;
        _crossRotation = Angle.FromDirection(_dashPos - boss.Position); // cross is rotated 45 degrees from dash direction

        // Cross centered on boss
        _aoes.Add(new AOEInstance(
            Cross,
            _dashPos,
            _crossRotation
        ));

        // Cache healers for cones
        foreach (var p in WorldState.Party.WithoutSlot())
        {
            if (!p.IsDead && p.Role == Role.Healer)
                _healers.Add(p);
        }
    }

    private void Clear()
    {
        _active = false;
        _aoes.Clear();
        _healers.Clear();
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active)
            return default;

        // Rebuild healer cones dynamically so they track healer movement
        var bossPos = Module.PrimaryActor.Position;

        _aoes.RemoveAll(a => a.Shape == HealerCone);

        foreach (var h in _healers)
        {
            var dir = Angle.FromDirection(h.Position - bossPos);
            _aoes.Add(new AOEInstance(HealerCone, bossPos, dir));
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_active)
            return;

        if (actor.Role == Role.Healer)
            hints.Add("Stack in cone");
        else
            hints.Add("Avoid cross");
    }
}

sealed class AssaultEvolvedAxeStack(BossModule module) : BossComponent(module)
{
    private Actor? _target;
    private const float Radius = 6f;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AssaultEvolved_AxeDash:
                SelectHealer();
                break;

            case AID.AssaultEvolved_HeavyWeight:
                _target = null; // clear on resolution
                break;
        }
    }

    private void SelectHealer()
    {
        _target = null;

        foreach (var p in WorldState.Party.WithoutSlot())
        {
            if (!p.IsDead && p.Role == Role.Healer)
            {
                _target = p;
                break;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == null)
            return;

        if (actor == _target)
            hints.Add("Party stack");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target != null)
            Arena.AddCircle(_target.Position, Radius, Colors.Safe);
    }
}

sealed class AssaultEvolved_AxeAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AssaultEvolved_AxeAOE, new AOEShapeCircle(8f));
sealed class AssaultEvolvedScythe(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;
    private WPos _dashPos;
    private readonly List<Actor> _players = [];
    private readonly List<AOEInstance> _aoes = [];

    // Reborn-native shapes
    private static readonly AOEShapeDonut Donut =
        new(innerRadius: 5f, outerRadius: 60f);

    private static readonly AOEShapeCone PlayerCone =
        new(radius: 60f, halfAngle: 10.Degrees()); // very narrow cones

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AssaultEvolved_ScytheDash)
            Arm(spell.TargetXZ);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo cast)
    {
        // Helper resolves the donut → clear everything
        if (_active && (AID)cast.Action.ID == AID.AssaultEvolved_ScytheDonut)
            Clear();
    }

    private void Arm(WPos dashPos)
    {
        _active = true;
        _dashPos = dashPos;
        _aoes.Clear();
        _players.Clear();

        var boss = Module.PrimaryActor;

        // Donut centered on boss destination
        _aoes.Add(new AOEInstance(
            Donut,
            _dashPos,
            boss.Rotation
        ));

        // Cache alive party members for cones
        foreach (var p in WorldState.Party.WithoutSlot())
        {
            if (!p.IsDead)
                _players.Add(p);
        }
    }

    private void Clear()
    {
        _active = false;
        _aoes.Clear();
        _players.Clear();
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active)
            return default;

        var bossPos = Module.PrimaryActor.Position;

        // Remove previously generated cones (keep donut)
        _aoes.RemoveAll(a => a.Shape == PlayerCone);

        // Rebuild cones dynamically so they track player movement
        foreach (var p in _players)
        {
            var dir = Angle.FromDirection(p.Position - bossPos);
            _aoes.Add(new AOEInstance(PlayerCone, bossPos, dir));
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_active)
            return;

        hints.Add("Spread cones, stay in donut");
    }
}
