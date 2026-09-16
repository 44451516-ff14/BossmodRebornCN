namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class Quote(BossModule module) : BossComponent(module)
{
    public Actor? Source;
    public List<uint> PendingMechanics = [];
    public DateTime NextActivation;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var count = PendingMechanics.Count;
        if (count > 0)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < count; ++i)
            {
                var hint = PendingMechanics[i] switch
                {
                    (uint)AID.IronChariot => "Out",
                    (uint)AID.LunarDynamo => "In",
                    (uint)AID.ThermionicBeam => "Stack",
                    (uint)AID.RavenDive or (uint)AID.MeteorStream => "Spread",
                    (uint)AID.DalamudDive => "Tankbuster",
                    _ => "???"
                };

                if (sb.Length > 0)
                {
                    sb.Append(" > ");
                }
                sb.Append(hint);
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        List<uint> aids = id switch
        {
            6492 => [(uint)AID.LunarDynamo, (uint)AID.IronChariot],
            6493 => [(uint)AID.LunarDynamo, (uint)AID.ThermionicBeam],
            6494 => [(uint)AID.ThermionicBeam, (uint)AID.IronChariot],
            6495 => [(uint)AID.ThermionicBeam, (uint)AID.LunarDynamo],
            6496 => [(uint)AID.RavenDive, (uint)AID.IronChariot],
            6497 => [(uint)AID.RavenDive, (uint)AID.LunarDynamo],
            6500 => [(uint)AID.MeteorStream, (uint)AID.DalamudDive],
            6501 => [(uint)AID.DalamudDive, (uint)AID.ThermionicBeam],
            6502 => [(uint)AID.RavenDive, (uint)AID.LunarDynamo, (uint)AID.MeteorStream],
            6503 => [(uint)AID.LunarDynamo, (uint)AID.RavenDive, (uint)AID.MeteorStream],
            6504 => [(uint)AID.IronChariot, (uint)AID.ThermionicBeam, (uint)AID.RavenDive],
            6505 => [(uint)AID.IronChariot, (uint)AID.RavenDive, (uint)AID.ThermionicBeam],
            6506 => [(uint)AID.LunarDynamo, (uint)AID.RavenDive, (uint)AID.ThermionicBeam],
            6507 => [(uint)AID.LunarDynamo, (uint)AID.IronChariot, (uint)AID.RavenDive],
            _ => []
        };
        if (aids.Count > 0)
        {
            Source = actor;
            PendingMechanics = aids;
            NextActivation = WorldState.FutureTime(5.1d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (PendingMechanics.Count != 0 && spell.Action.ID == PendingMechanics[0])
        {
            PendingMechanics.RemoveAt(0);
            NextActivation = WorldState.FutureTime(3.1d);
        }
    }
}

sealed class QuoteIronChariotLunarDynamo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();

    private readonly AOEShapeCircle _shapeChariot = new(8.55f);
    private readonly AOEShapeDonut _shapeDynamo = new(6f, 22f); // TODO: verify inner radius
    private AOEInstance[] _aoe = [];
    private int lastmechCount;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void Update()
    {
        var count = _quote != null ? _quote.PendingMechanics.Count : -1;
        if (count > 0 && count != lastmechCount)
        {
            lastmechCount = count;
            AOEShape? shape = _quote!.PendingMechanics[0] switch
            {
                (uint)AID.IronChariot => _shapeChariot,
                (uint)AID.LunarDynamo => _shapeDynamo,
                _ => null
            };
            if (shape != null && _quote?.Source != null)
            {
                _aoe = [new(shape, _quote.Source.Position.Quantized(), default, _quote.NextActivation)];
                return;
            }
            _aoe = [];
        }
        if (count <= 0)
        {
            _aoe = [];
        }
    }
}

sealed class QuoteThermionicBeam(UCOB module) : Components.UniformStackSpread(module, 4f, default, 8, 8)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();
    private readonly Actor _nael = module.Nael()!;

    public override void Update()
    {
        var stackImminent = _quote != null && _quote.PendingMechanics.Count != 0 && _quote.PendingMechanics[0] == (uint)AID.ThermionicBeam;
        if (stackImminent && Stacks.Count == 0 && Raid.Player() is var target && target != null) // note: target is random
        {
            AddStack(target, _quote!.NextActivation);
        }
        else if (!stackImminent && Stacks.Count > 0)
        {
            Stacks.Clear();
        }
        base.Update();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Stacks.Count > 0)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(_nael.Position, 2f), Stacks.Ref(0).Activation);
        }
    }
}

sealed class QuoteRavenDive(UCOB module) : Components.UniformStackSpread(module, default, 3f)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();
    private readonly Actor _nael = module.Nael()!;
    private readonly Actor _bahamut = module.BahamutPrime()!;

    public override void Update()
    {
        var spreadImminent = _quote != null && _quote.PendingMechanics.Count != 0 && _quote.PendingMechanics[0] == (uint)AID.RavenDive;
        if (spreadImminent && Spreads.Count == 0)
        {
            AddSpreads(Raid.WithoutSlot(true, true, true), _quote!.NextActivation);
        }
        else if (!spreadImminent && Spreads.Count > 0)
        {
            Spreads.Clear();
        }
        base.Update();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        //base.AddAIHints(slot, actor, assignment, hints);

        if (IsSpreadTarget(actor))
        {
            var off = (int)assignment;

            WPos center;
            Angle north;

            if (_nael.IsTargetable) // p2: clock spots around nael
            {
                center = _nael.Position;
                north = 180f.Degrees();
            }
            else // p3 FRT: clock spots around arena center, relative north pointing towards bahamut so tanks naturally get tethers
            {
                center = Arena.Center;
                north = (_bahamut.Position - center).ToAngle();
            }

            hints.AddForbiddenZone(new SDPrecisePosition(center + (north.Deg - 45f * off).Degrees().ToDirection() * 5f, new(0f, 1f), 0.5f, actor.Position, 0.1f), Spreads.Ref(0).Activation);
        }
    }
}

sealed class QuoteMeteorStream(UCOB module) : Components.UniformStackSpread(module, default, 4f)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();
    private readonly Actor _bahamut = module.BahamutPrime()!;
    public bool Fixed;

    public override void Update()
    {
        var spreadImminent = _quote != null && _quote.PendingMechanics.Count > 0 && _quote.PendingMechanics[0] == (uint)AID.MeteorStream;
        if (spreadImminent && Spreads.Count == 0)
        {
            AddSpreads(Raid.WithoutSlot(true, true, true), _quote!.NextActivation);
        }
        else if (!spreadImminent && Spreads.Count > 0)
        {
            Spreads.Clear();
        }
        base.Update();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // fellruin: fixed spread spots, as usual
        if (Fixed && IsSpreadTarget(actor) && SpreadSpot(_bahamut, assignment) is var spot && spot != default)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(spot, 1f), Spreads.Ref(0).Activation);
            return;
        }

        // p2: normal spread
        base.AddAIHints(slot, actor, assignment, hints);
    }

    private WPos SpreadSpot(Actor bahamut, PartyRolesConfig.Assignment assignment)
    {
        var relN = (bahamut.Position - Arena.Center).ToAngle();
        var a45 = 45f.Degrees();
        return assignment switch
        {
            PartyRolesConfig.Assignment.MT => bahamut.Position + (relN + a45).ToDirection() * 5f,
            PartyRolesConfig.Assignment.OT => bahamut.Position + (relN - a45).ToDirection() * 5f,
            PartyRolesConfig.Assignment.M1 => bahamut.Position + (relN - a45).ToDirection() * -5f,
            PartyRolesConfig.Assignment.M2 => bahamut.Position + (relN + a45).ToDirection() * -5f,
            PartyRolesConfig.Assignment.H1 => bahamut.Position + (relN - a45).ToDirection() * -5f + relN.ToDirection() * -8f,
            PartyRolesConfig.Assignment.H2 => bahamut.Position + (relN + a45).ToDirection() * -5f + relN.ToDirection() * -8f,
            PartyRolesConfig.Assignment.R1 => bahamut.Position + (relN - a45).ToDirection() * -12f + relN.ToDirection() * -8f,
            PartyRolesConfig.Assignment.R2 => bahamut.Position + (relN + a45).ToDirection() * -12f + relN.ToDirection() * -8f,
            _ => default,
        };
    }
}

sealed class QuoteDalamudDive(UCOB module) : Components.GenericBaitAway(module, (uint)AID.DalamudDive, true, true)
{
    private readonly Quote? _quote = module.FindComponent<Quote>();
    private readonly Actor _nael = module.Nael()!;

    private readonly AOEShapeCircle _shape = new(5f);

    public override void Update()
    {
        var imminent = _quote != null && _quote.PendingMechanics.Count > 0 && _quote.PendingMechanics[0] == (uint)AID.DalamudDive;
        if (imminent && CurrentBaits.Count == 0 && WorldState.Actors.Find(_nael.TargetID) is var target && target != null)
        {
            CurrentBaits.Add(new(target, target, _shape));
        }
        else if (!imminent && CurrentBaits.Count > 0)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // tank should plant and let party dodge
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        var isBaitTarget = false;
        for (var i = 0; i < len; ++i)
        {
            if (baits[i].Target == actor)
            {
                isBaitTarget = true;
                break;
            }
        }
        if (!isBaitTarget)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }

        if (actor.Role is Role.Melee or Role.Tank)
        {
            for (var i = 0; i < len; ++i)
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(baits[i].Target.Position, 6f));
            }
        }
    }
}
