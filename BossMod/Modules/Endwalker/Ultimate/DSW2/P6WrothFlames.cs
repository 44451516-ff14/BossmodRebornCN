namespace BossMod.Endwalker.Ultimate.DSW2;

class P6WrothFlames : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = []; // cauterize, then flame blasts
    private WPos _startingSpot;

    private static readonly AOEShapeRect _shapeCauterize = new(80f, 11f);
    private static readonly AOEShapeCross _shapeBlast = new(44f, 3f);

    public bool ShowStartingSpot => _startingSpot.X != 0 && _startingSpot.Z != 0 && NumCasts == 0;

    // assume it is activated when hraesvelgr is already in place; could rely on PATE 1E43 instead
    public P6WrothFlames(BossModule module) : base(module)
    {
        var cauterizeCasters = module.Enemies((uint)OID.HraesvelgrP6);
        var cauterizeCaster = cauterizeCasters.Count != 0 ? cauterizeCasters[0] : null;
        if (cauterizeCaster != null)
        {
            _aoes.Add(new(_shapeCauterize, WPos.ClampToGrid(cauterizeCaster.Position), cauterizeCaster.Rotation, WorldState.FutureTime(8.1d)));
            _startingSpot.X = cauterizeCaster.Position.X < 95f ? 120f : 80f; // assume nidhogg is at 78, prefer uptime if possible
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        int count = (NumCasts > 0) ? 3 : 4;
        return _aoes.Count > count ? _aoes.AsSpan()[..count] : CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (ShowStartingSpot)
            movementHints.Add(actor.Position, _startingSpot, Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (ShowStartingSpot)
            Arena.AddCircle(_startingSpot, 1, Colors.Safe);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ScarletPrice)
        {
            if (_aoes.Count == 4)
                _startingSpot.Z = actor.Position.Z < Arena.Center.Z ? 120 : 80;

            var delay = _aoes.Count switch
            {
                < 4 => 8.7f,
                < 7 => 9.7f,
                _ => 6.9f
            };
            _aoes.Add(new(_shapeBlast, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CauterizeH or (uint)AID.FlameBlast)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

class P6AkhMorn(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.AkhMornFirst, 6f, 8, 8)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { } // do not clear stacks on first cast

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhMornFirst or (uint)AID.AkhMornRest)
            if (++NumFinishedStacks >= 4)
                Stacks.Clear();
    }
}

class P6AkhMornVoidzone(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.VoidzoneAhkMorn);
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

class P6SpreadingEntangledFlames(BossModule module) : Components.UniformStackSpread(module, 4f, 5f, 2, alwaysShowSpreads: true)
{
    private readonly P6HotWingTail? _wingTail = module.FindComponent<P6HotWingTail>();
    private readonly bool _voidzonesNorth = module.Enemies((uint)OID.VoidzoneAhkMorn).Sum(z => z.Position.Z - module.Center.Z) < 0;

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var p in SafeSpots(actor))
            movementHints.Add(actor.Position, p, Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var p in SafeSpots(pc))
            Arena.AddCircle(p, 1, Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        // TODO: activation
        switch (status.ID)
        {
            case (uint)SID.SpreadingFlames:
                AddSpread(actor);
                break;
            case (uint)SID.EntangledFlames:
                AddStack(actor);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SpreadingFlames:
                Spreads.Clear();
                break;
            case (uint)AID.EntangledFlames:
                Stacks.Clear();
                break;
        }
    }

    // note: this assumes standard positions (spreads = black debuffs = under black dragon nidhogg, etc)
    // TODO: consider assigning concrete spots to each player
    private List<WPos> SafeSpots(Actor actor)
    {
        if (_wingTail == null)
            return [];
        var safespots = new List<WPos>();
        var x = Arena.Center.X;
        var z = Arena.Center.Z + (_wingTail.NumAOEs != 1 ? default : _voidzonesNorth ? 10f : -10f);
        if (IsSpreadTarget(actor))
        {
            safespots.Add(new(x - 18f, z));
            safespots.Add(new(x - 12f, z));
            safespots.Add(new(x - 6f, z));
            safespots.Add(new(x, z));
        }
        else
        {
            safespots.Add(new(x + 9f, z));
            safespots.Add(new(x + 18f, z));
        }
        return safespots;
    }
}
