namespace BossMod.Endwalker.Ultimate.DSW2;

class P5WrathOfTheHeavensSkywardLeap(BossModule module) : Components.UniformStackSpread(module, default, 24f, alwaysShowSpreads: true, raidwideOnResolve: false)
{
    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (IsSpreadTarget(actor) && SafeSpot() is var safespot && safespot != default)
            movementHints.Add(actor.Position, safespot, Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (IsSpreadTarget(pc) && SafeSpot() is var safespot && safespot != default)
            Arena.AddCircle(safespot, 1f, Colors.Safe);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SkywardLeapP5)
            AddSpread(actor, WorldState.FutureTime(6.4f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SkywardLeapP5)
            Spreads.Clear();
    }

    // note: this assumes LPDU strat
    private WPos SafeSpot()
    {
        var relNorth = Module.Enemies((uint)OID.Vedrfolnir).FirstOrDefault();
        if (relNorth == null)
            return default;
        var dirToNorth = Angle.FromDirection(relNorth.Position - Arena.Center);
        return Arena.Center + 20f * (dirToNorth + 60f.Degrees()).ToDirection();
    }
}

class P5WrathOfTheHeavensSpiralPierce(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50f, 8f), (uint)TetherID.SpiralPierce, (uint)AID.SpiralPierce)
{
    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (SafeSpot(actor) is var safespot && safespot != default)
            movementHints.Add(actor.Position, safespot, Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (SafeSpot(pc) is var safespot && safespot != default)
            Arena.AddCircle(safespot, 1, Colors.Safe);
    }

    private WPos SafeSpot(Actor actor)
    {
        // stay as close to twisting dive as possible, while stretching the tether through the center
        var bait = ActiveBaitsOn(actor).FirstOrDefault();
        if (bait.Source == null)
            return default;
        WDir toMidpoint = default;
        foreach (var b in CurrentBaits)
            toMidpoint += b.Source.Position - Arena.Center;
        var relSouthDir = Angle.FromDirection(-toMidpoint);
        var offset = toMidpoint.OrthoL().Dot(bait.Source.Position - Arena.Center) > 0f ? 20f.Degrees() : -20f.Degrees();
        return Arena.Center + 20f * (relSouthDir + offset).ToDirection();
    }
}

class P5WrathOfTheHeavensChainLightning(BossModule module) : Components.UniformStackSpread(module, default, 5f, alwaysShowSpreads: true)
{
    public BitMask Targets;

    public void ShowSpreads(float delay) => AddSpreads(Raid.WithSlot(true, true, true).IncludedInMask(Targets).Actors(), WorldState.FutureTime(delay));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.Count == 0 && Targets[slot])
            hints.Add("Prepare for lightning!", false);
        base.AddHints(slot, actor, hints);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    // note: this happens about a second before statuses appear
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChainLightning:
                foreach (var t in spell.Targets)
                    Targets.Set(Raid.FindSlot(t.ID));
                break;
            case (uint)AID.ChainLightningAOE:
                Targets.Reset();
                Spreads.Clear();
                break;
        }
    }
}

class P5WrathOfTheHeavensTwister(BossModule module) : Components.GenericAOEs(module, default, "GTFO from twister!")
{
    private readonly List<WPos> _predicted = GetPositions(module);
    private readonly List<Actor> _voidzones = module.Enemies((uint)OID.VoidzoneTwister);

    private static readonly AOEShapeCircle _shape = new(2f); // TODO: verify radius

    public bool Active => _voidzones.Count > 0;

    private static List<WPos> GetPositions(BossModule module)
    {
        var party = module.Raid.WithoutSlot(false, true, true);
        var len = party.Length;
        var pos = new List<WPos>(len);
        for (var i = 0; i < len; ++i)
            pos.Add(party[i].Position);
        return pos;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countP = _predicted.Count;
        var countV = _voidzones.Count;
        var aoes = new AOEInstance[countP + countV];
        for (var i = 0; i < countP; ++i)
            aoes[i] = new(_shape, _predicted[i]); // TODO: activation
        for (var i = 0; i < countV; ++i)
            aoes[i] = new(_shape, _voidzones[i].Position);
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.VoidzoneTwister)
            _predicted.Clear();
    }
}

// note: we're not really showing baits here, it's more misleading than helpful...
class P5WrathOfTheHeavensCauterizeBait(BossModule module) : BossComponent(module)
{
    private Actor? _target;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target != actor)
            return;
        hints.Add("Prepare for divebomb!", false);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_target != actor)
            return;
        movementHints.Add(actor.Position, SafeSpot(), Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target == pc)
            Arena.AddCircle(SafeSpot(), 1f, Colors.Safe);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Cauterize)
            _target = actor;
    }

    private WPos SafeSpot()
    {
        var charibert = Module.Enemies((uint)OID.SerCharibert).FirstOrDefault();
        if (charibert == null)
            return default;
        return Arena.Center + 20f * (charibert.Position - Arena.Center).Normalized();
    }
}

class P5WrathOfTheHeavensAscalonsMercyRevealed(BossModule module) : Components.BaitAwayEveryone(module, module.Enemies((uint)OID.BossP5).FirstOrDefault(), new AOEShapeCone(50f, 15f.Degrees()), (uint)AID.AscalonsMercyRevealedAOE);

// TODO: detect baiter
class P5WrathOfTheHeavensLiquidHeaven(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.LiquidHeaven, m => m.Enemies(OID.VoidzoneLiquidHeaven).Where(z => z.EventState != 7), 1.1f);

// TODO: detect baiter
class P5WrathOfTheHeavensAltarFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AltarFlareAOE, 8f);

class P5WrathOfTheHeavensEmptyDimension(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmptyDimension, new AOEShapeDonut(6f, 70f))
{
    private WPos _predicted;

    public bool KnowPosition => _predicted != default;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Casters.Count == 0 && KnowPosition)
            Arena.AddCircle(_predicted, 6f, Colors.Safe, 2f);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.SerGrinnaux && id == 0x1E43u)
            _predicted = actor.Position;
    }
}
