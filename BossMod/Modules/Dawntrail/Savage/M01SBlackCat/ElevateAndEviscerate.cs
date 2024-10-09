namespace BossMod.Dawntrail.Savage.M01SBlackCat;

class ElevateAndEviscerateShockwave(BossModule module) : Components.GenericAOEs(module, default, "GTFO from shockwave!")
{
    private AOEInstance? aoe;
    private static readonly AOEShapeCross cross = new(60, 5);
    private readonly ElevateAndEviscerate _kb = module.FindComponent<ElevateAndEviscerate>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_kb.CurrentTarget != null && _kb.CurrentTarget != actor)
        {
            yield return new(cross, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.CurrentTarget.Position)), default, _kb.CurrentDeadline.AddSeconds(3.2f));
            if (_kb.CurrentKnockbackDistance == 0)
                yield return new(Mouser.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.CurrentTarget.Position)), default, _kb.CurrentDeadline.AddSeconds(2));
            else if (_kb.CurrentKnockbackDistance == 10 && Module.InBounds(_kb.Cache))
                yield return new(Mouser.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, _kb.CurrentDeadline.AddSeconds(4.2f));
        }
        if (aoe != null && _kb.LastTarget != actor)
            yield return aoe.Value;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ElevateAndEviscerateHitAOE or AID.ElevateAndEviscerateKnockbackAOE && Module.InBounds(_kb.Cache))
            aoe = new(Mouser.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, WorldState.FutureTime(_kb.CurrentKnockbackDistance == 0 ? 1.4f : 3.6f));
        else if ((AID)spell.Action.ID is AID.ElevateAndEviscerateImpactHit or AID.ElevateAndEviscerateImpactKnockback)
            aoe = null;
    }

    public override void Update()
    {
        if (aoe != null && _kb.CurrentTarget != null && _kb.CurrentTarget.IsDead) // impact doesn't happen if player dies between ElevateAndEviscerate and Impact
            aoe = null;
    }
}

class ElevateAndEviscerate(BossModule module) : Components.Knockback(module, ignoreImmunes: true, stopAfterWall: true)
{
    private Actor? _nextTarget; // target selection icon appears before cast start
    public Actor? CurrentTarget; // for current mechanic
    public Actor? LastTarget; // for current mechanic
    public DateTime CurrentDeadline; // for current target - expected time when stun starts, which is deadline for positioning
    public int CurrentKnockbackDistance;
    public WPos Cache;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => player == CurrentTarget ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (CurrentTarget != null && actor == CurrentTarget && CurrentKnockbackDistance > 0)
            yield return new(Arena.Center, CurrentKnockbackDistance, CurrentDeadline, Direction: actor.Rotation, Kind: Kind.DirForward);
    }

    public override void Update()
    {
        if (CurrentTarget != null && Sources(0, CurrentTarget).Any())
            Cache = CalculateMovements(0, CurrentTarget).First().to;
        else if (CurrentTarget != null)
            Cache = CurrentTarget.Position;
    }

    public override void OnEventIcon(Actor actor, uint iconID)
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
        if ((AID)spell.Action.ID is AID.ElevateAndEviscerateKnockback or AID.ElevateAndEviscerateHit)
        {
            CurrentDeadline = Module.CastFinishAt(spell, 1.8f);
            CurrentKnockbackDistance = (AID)spell.Action.ID == AID.ElevateAndEviscerateKnockback ? 10 : 0;
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
        if ((AID)spell.Action.ID is AID.ElevateAndEviscerateHitAOE or AID.ElevateAndEviscerateKnockbackAOE)
            LastTarget = CurrentTarget;
        else if ((AID)spell.Action.ID == AID.ElevateAndEviscerateShockwave)
        {
            ++NumCasts;
            CurrentTarget = null;
            CurrentDeadline = default;
            CurrentKnockbackDistance = 0;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<ElevateAndEviscerateHint>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}

class ElevateAndEviscerateHint(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var comp = Module.FindComponent<ElevateAndEviscerate>()!.CurrentTarget;
        if (comp != default && actor == comp)
        {
            var damagedCells = Module.FindComponent<ArenaChanges>()!.DamagedCells;
            var tiles = ArenaChanges.Tiles;

            foreach (var index in damagedCells.SetBits())
            {
                var tile = tiles[index];
                yield return new(Mouser.Rect, tile.Center, Color: Colors.FutureVulnerable, Risky: false);
            }
        }
    }
}
