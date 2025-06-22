namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class Snowboulder(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance>[] _aoesPerPlayer = new List<AOEInstance>[PartyState.MaxPartySize];
    private readonly List<RectangleSE> rectangles = new(6);
    private readonly List<DateTime> activations = new(6);
    public BitMask Vulnerable;
    private bool isInit;
    private PolygonWithHolesDistanceFunction distance;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => isInit && slot is >= 0 and < 8 ? CollectionsMarshal.AsSpan(_aoesPerPlayer[slot]) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SnowBoulderVisual)
        {
            rectangles.Add(new(caster.Position, spell.LocXZ, 5f));
            activations.Add(Module.CastFinishAt(spell, 0.1f));
            if (rectangles.Count % 2 == 0)
            {
                ComputeNonOverlappingArea();
            }
        }
    }

    private void ComputeNonOverlappingArea()
    {
        var count = rectangles.Count;
        var center = Arena.Center;
        for (var i = 0; i < 8; ++i)
        {
            _aoesPerPlayer[i] = [];
        }
        var colorSafe1 = Colors.Safe;
        var colorSafe2 = Colors.SafeFromAOE;
        var clipper = new PolygonClipper();
        var unionOperand = new PolygonClipper.Operand();
        for (var i = 0; i < count; ++i)
        {
            List<RectangleSE> rects = [.. rectangles];
            rects.RemoveAt(i);
            var aoe = new AOEShapeCustom([rectangles[i]]);
            var aoeSafe = new AOEShapeCustom([rectangles[i]], rects);
            unionOperand.AddPolygon(aoeSafe.GetCombinedPolygon(center));
            for (var j = 0; j < 8; ++j)
            {
                _aoesPerPlayer[j].Add(new(Vulnerable[j] ? ref aoe : ref aoeSafe, Arena.Center, default, activations[i], Vulnerable[j] ? default : i < 2 ? colorSafe1 : colorSafe2));
            }
        }
        distance = new PolygonWithHolesDistanceFunction(center, clipper.Simplify(unionOperand));
        isInit = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SnowBoulder)
        {
            var targets = spell.Targets;
            var count = targets.Count;
            rectangles.RemoveAt(0);
            activations.RemoveAt(0);
            for (var i = 0; i < count; ++i)
            {
                var slot = Raid.FindSlot(targets[i].ID);
                if (slot is >= 0 and < 8)
                {
                    Vulnerable[slot] = true;
                }
            }
            if (++NumCasts % 2 == 0 && NumCasts < 6)
            {
                Array.Clear(_aoesPerPlayer);
                ComputeNonOverlappingArea();
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!isInit)
            return;
        var aoes = _aoesPerPlayer[slot];
        var count = aoes.Count;
        if (count == 0)
            return;
        if (!Vulnerable[slot])
        {
            var risky = true;
            for (var i = 0; i < count; ++i)
            {
                var aoe = aoes[i];
                if (aoe.Check(actor.Position))
                {
                    risky = false;
                    break;
                }
            }
            hints.Add("Share damage inside wild charge!", risky);
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Vulnerable[slot] && isInit)
        {
            hints.AddForbiddenZone(distance.InvertedDistance, activations[0]);
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (isInit)
        {
            hints.Add("Wild charges -> Tanks infront!");
        }
    }
}

sealed class SnowBoulderKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _kbs = new(6);
    private readonly Snowboulder _charge = module.FindComponent<Snowboulder>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var count = _kbs.Count;
        if (count == 0)
            return [];
        var kbs = CollectionsMarshal.AsSpan(_kbs);
        var max = count > 2 ? 2 : count;
        return kbs[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SnowBoulderVisual)
        {
            var dir = spell.LocXZ - caster.Position;
            _kbs.Add(new(caster.Position, 10f, Module.CastFinishAt(spell, 0.1f), new AOEShapeRect(dir.Length(), 5f), Angle.FromDirection(dir), Kind.DirForward));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SnowBoulder)
        {
            ++NumCasts;
            if (_kbs.Count != 0)
            {
                _kbs.RemoveAt(0);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_charge.Vulnerable[slot] && _kbs.Count != 0)
        {
            var act = _kbs[0].Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 20f), _kbs[0].Activation);
            }
        }
    }
}

sealed class AvalaunchTether(BossModule module) : Components.StretchTetherDuo(module, 58f, 8f, (uint)TetherID.AvalaunchBad);
