namespace BossMod.Dawntrail.Savage.M01SBlackCat;

class ElevateAndEviscerateShockwave(BossModule module) : Components.GenericAOEs(module, default, "GTFO from shockwave!")
{
    private AOEInstance? aoe;
    private static readonly AOEShapeCross cross = new(60f, 5f);
    private readonly ElevateAndEviscerate _kb = module.FindComponent<ElevateAndEviscerate>()!;
    public static readonly AOEShapeRect Rect = new(5f, 5f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>();
        if (_kb.CurrentTarget != null && _kb.CurrentTarget != actor)
        {
            aoes.Add(new(cross, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.CurrentTarget.Position)), default, _kb.CurrentDeadline.AddSeconds(3.2d)));
            if (_kb.CurrentKnockbackDistance == 0)
                aoes.Add(new(Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.CurrentTarget.Position)), default, _kb.CurrentDeadline.AddSeconds(2d)));
            else if (_kb.CurrentKnockbackDistance == 10 && Module.InBounds(_kb.Cache))
                aoes.Add(new(Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, _kb.CurrentDeadline.AddSeconds(4.2d)));
        }
        if (aoe != null && _kb.LastTarget != actor)
            aoes.Add(aoe.Value);
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ElevateAndEviscerateHitAOE or (uint)AID.ElevateAndEviscerateKnockbackAOE && Module.InBounds(_kb.Cache))
            aoe = new(Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, WorldState.FutureTime(_kb.CurrentKnockbackDistance == 0f ? 1.4d : 3.6d));
        else if (spell.Action.ID is (uint)AID.ElevateAndEviscerateImpactHit or (uint)AID.ElevateAndEviscerateImpactKnockback)
            aoe = null;
    }

    public override void Update()
    {
        if (aoe != null && _kb.CurrentTarget != null && _kb.CurrentTarget.IsDead) // impact doesn't happen if player dies between ElevateAndEviscerate and Impact
            aoe = null;
    }
}

class ElevateAndEviscerate(BossModule module) : Components.GenericKnockback(module, ignoreImmunes: true, stopAfterWall: true)
{
    private Actor? _nextTarget; // target selection icon appears before cast start
    public Actor? CurrentTarget; // for current mechanic
    public Actor? LastTarget; // for current mechanic
    public DateTime CurrentDeadline; // for current target - expected time when stun starts, which is deadline for positioning
    public float CurrentKnockbackDistance;
    public WPos Cache;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => player == CurrentTarget ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (CurrentTarget != null && actor == CurrentTarget && CurrentKnockbackDistance > 0)
            return new Knockback[1] { new(Arena.Center, CurrentKnockbackDistance, CurrentDeadline, Direction: actor.Rotation, Kind: Kind.DirForward) };
        return [];
    }

    public override void Update()
    {
        if (CurrentTarget != null && ActiveKnockbacks(0, CurrentTarget).Length != 0)
            Cache = CalculateMovements(0, CurrentTarget)[0].to;
        else if (CurrentTarget != null)
            Cache = CurrentTarget.Position;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID != (uint)IconID.ElevateAndEviscerate)
            return;
        _nextTarget = actor;
        InitIfReady();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (CurrentTarget == actor)
            hints.Add($"{(CurrentKnockbackDistance > 0 ? "Knockback" : "Hit")} in {Math.Max(0, (CurrentDeadline - WorldState.CurrentTime).TotalSeconds):f1}s", false);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ElevateAndEviscerateKnockback or (uint)AID.ElevateAndEviscerateHit)
        {
            CurrentDeadline = Module.CastFinishAt(spell, 1.8f);
            CurrentKnockbackDistance = spell.Action.ID == (uint)AID.ElevateAndEviscerateKnockback ? 10f : 0f;
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (_nextTarget != null && CurrentDeadline != default)
        {
            CurrentTarget = _nextTarget;
            _nextTarget = null;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ElevateAndEviscerateHitAOE or (uint)AID.ElevateAndEviscerateKnockbackAOE)
            LastTarget = CurrentTarget;
        else if (spell.Action.ID == (uint)AID.ElevateAndEviscerateShockwave)
        {
            ++NumCasts;
            CurrentTarget = null;
            CurrentDeadline = default;
            CurrentKnockbackDistance = 0;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var comp = Module.FindComponent<ElevateAndEviscerateHint>();
        if (comp != null)
        {
            var aoes = comp.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].Check(pos))
                    return true;
            }
        }
        return !Module.InBounds(pos);
    }
}

class ElevateAndEviscerateHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ElevateAndEviscerate _kb = module.FindComponent<ElevateAndEviscerate>()!;
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var comp = _kb.CurrentTarget;
        if (comp != null && actor == comp)
        {
            var damagedCells = _arena.DamagedCells.SetBits();
            var tiles = ArenaChanges.Tiles;
            var aoes = new List<AOEInstance>();
            var len = damagedCells.Length;
            for (var i = 0; i < len; ++i)
            {
                var tile = tiles[damagedCells[i]];
                aoes.Add(new(ElevateAndEviscerateShockwave.Rect, tile.Center, Color: Colors.FutureVulnerable, Risky: false));
            }
            return CollectionsMarshal.AsSpan(aoes);
        }
        return [];
    }
}
