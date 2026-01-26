namespace BossMod.Dawntrail.Savage.M10STheXtremes;

sealed class SickestTakeOff(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SickestTakeOff, new AOEShapeRect(50f, 7.5f));
sealed class SickSwell1(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SickSwell1, 10f, kind: Kind.DirForward);

sealed class AwesomeSplashSlab(BossModule module) : Components.GenericStackSpread(module, false)
{
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;
    private readonly FreakyPyrotation _freaky = module.FindComponent<FreakyPyrotation>()!;
    private readonly List<Stack> _delayFreaky = [];
    private enum Mechanic
    {
        Stack,
        Spread,
        None
    }
    private Mechanic _mech = Mechanic.None;
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_)
        {
            //var party = Raid.WithoutSlot(false, true, true);
            var party = _debuff.WaterPlayers.Any() ? _debuff.GetWaterActors() : Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            var act = WorldState.FutureTime(15.8d);
            switch (status.Extra)
            {
                // delay adding/hinting mechanic until Freaky Pyrorotation resolves to avoid clutter
                // Freaky Pyrorotation icons appear .15s before light party stack during arena split
                case 0x3ED:
                    // light party stack
                    for (var i = 0; i < len; i++)
                    {
                        if (party[i].Role == Role.Healer)
                        {
                            _mech = Mechanic.Stack;
                            Stack stack = new(party[i], 6f, 4, activation: act);
                            if (_freaky.ActiveStacks.Count == 0)
                            {
                                Stacks.Add(stack);
                            }
                            else
                            {
                                _delayFreaky.Add(stack);
                            }
                        }
                    }
                    break;
                case 0x3EE:
                    // spread
                    for (var i = 0; i < len; i++)
                    {
                        _mech = Mechanic.Spread;
                        Spreads.Add(new(party[i], 5f, activation: act));
                    }
                    break;
                case 0x3EF:
                    // watersnaking stack
                    for (var i = 0; i < len; i++)
                    {
                        if (party[i].Role == Role.Healer)
                        {
                            /*
                            if (party[i].FindStatus((uint)SID.Watersnaking) != null)
                            {
                                _mech = Mechanic.Stack;
                                Stacks.Add(new(party[i], 6f, 4, activation: act));
                            }
                            */
                            _mech = Mechanic.Stack;
                            Stacks.Add(new(party[i], 6f, 4, activation: act, forbiddenPlayers: _debuff.FirePlayers.Any() ? _debuff.FirePlayers : default));
                        }
                    }
                    break;
                case 0x3F0:
                    // watersnaking spread
                    for (var i = 0; i < len; i++)
                    {
                        /*
                        if (party[i].FindStatus((uint)SID.Watersnaking) != null)
                        {
                            _mech = Mechanic.Spread;
                            Spreads.Add(new(party[i], 5f, activation: act));
                        }
                        */
                        _mech = Mechanic.Spread;
                        Spreads.Add(new(party[i], 5f, activation: act));
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AwesomeSplash or (uint)AID.AwesomeSlab or (uint)AID.AwesomeSplashAerial or (uint)AID.AwesomeSlabAerial)
        {
            _mech = Mechanic.None;
            Spreads.Clear();
            Stacks.Clear();
        }

        if (spell.Action.ID == _freaky.StackAction)
        {
            Stacks.AddRange(_delayFreaky);
            _delayFreaky.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_mech == Mechanic.None)
            return;

        // TODO: only display hint if watersnaking
    }
}

sealed class AlleyOopWater(BossModule module) : Components.GenericBaitAway(module, alwaysDrawOtherBaits: true)
{
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopDoubleDipCast or (uint)AID.ReverseAlleyOopCast)
        {
            var targets = GetTargets();
            var len = targets.Length;

            for (var i = 0; i < len; i++)
            {
                CurrentBaits.Add(new(caster, targets[i], new AOEShapeCone(60f, 15f.Degrees()), Module.CastFinishAt(spell)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AlleyOopDoubleDip1 or (uint)AID.ReverseAlleyOop1)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }

    private Actor[] GetTargets()
    {
        var party = Raid.WithSlot(false, true, true);
        var waters = _debuff.WaterPlayers;

        if (waters.Any())
        {
            party = [.. party.IncludedInMask(waters)];
        }

        var len = party.Length;

        if (len == 0)
            return [];

        var actors = new Actor[len];
        for (var i = 0; i < len; i++)
        {
            actors[i] = party[i].Item2;
        }

        return actors;
    }
}

sealed class AlleyOopWaterAfter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly double _delay = 2.8f;
    private readonly Angle _offset = 22.5f.Degrees();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AlleyOopDoubleDip1)
        {
            _aoes.Add(new(new AOEShapeCone(60f, 15f.Degrees()), caster.Position, spell.Rotation, WorldState.FutureTime(_delay)));
        }

        // reverse does casts in both directions
        if (spell.Action.ID == (uint)AID.ReverseAlleyOop1)
        {
            _aoes.Add(new(new AOEShapeCone(60f, 7.5f.Degrees()), caster.Position, spell.Rotation + _offset, WorldState.FutureTime(_delay)));
            _aoes.Add(new(new AOEShapeCone(60f, 7.5f.Degrees()), caster.Position, spell.Rotation - _offset, WorldState.FutureTime(_delay)));
        }

        if (spell.Action.ID is (uint)AID.AlleyOopDoubleDip2 or (uint)AID.ReverseAlleyOop2)
        {
            NumCasts++;
            _aoes.Clear();
        }
    }
}

sealed class DeepImpact(BossModule module) : Components.GenericBaitProximity(module)
{
    // baited by blue debuff as well as furthest
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeepImpactCast)
        {
            var activation = Module.CastFinishAt(spell).AddSeconds(0.3d);
            Bait bait = new(caster, new AOEShapeCircle(6f), activation, 1, false, centerAtTarget: true, tankbuster: true, caster: (uint)OID.DeepBlue, forbidden: _debuff.FirePlayers, damageType: AIHints.PredictedDamageType.Tankbuster);
            CurrentBaits.Add(bait);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DeepImpact)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}
sealed class DeepImpactKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly DeepImpact _impact = module.FindComponent<DeepImpact>()!;
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_impact.ActiveBaits.Count == 0)
            return [];

        var baits = CollectionsMarshal.AsSpan(_impact.ActiveBaits);
        var bait = baits[0];

        if (!_impact.IsBaitTarget(ref bait, actor))
            return [];

        var direction = (actor.Position - bait.Position).ToAngle();
        return new Knockback[1] { new(actor.Position, 10f, bait.Activation, null, direction, Kind.DirForward) };
    }
}

sealed class DiversDareBlue(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiversDareBlue);
sealed class DeepVarial(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeepVarial, new AOEShapeCone(60f, 60f.Degrees()));
sealed class AwesomeSplashSlabAerial(BossModule module) : Components.GenericStackSpread(module)
{
    // activate this before watersnaking to get status on gain
    // otherwise find actors with watersnaking status on cast end, not sure if there's noticeable performance hit
    // or maybe have a separate component for only tracking water/fire snaking status to read from?
    // TODO: need to figure out what determines spread vs stack
    // wave has action timeline but 0x2487 but same for stack spread
    // map effect differents, 2(N) 4(S) 00800040 (stack) 08000400 (spread)
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is not 0x02 and not 0x04)
            return;

        if (state is not 0x00800040 and not 0x08000400)
            return;

        var party = _debuff.GetWaterActors();
        var len = party.Length;

        for (var i = 0; i < len; i++)
        {
            var player = party[i];

            if (state == 0x00800040)
            {
                // who does it target if healer is dead?
                if (player.Role == Role.Healer)
                {
                    Stacks.Add(new(player, 6f, 4, 4, WorldState.FutureTime(4.7d), _debuff.FirePlayers));
                }
            }
            if (state == 0x08000400)
            {
                Spreads.Add(new(player, 5f, WorldState.FutureTime(4.7d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AwesomeSplashAerial or (uint)AID.AwesomeSlabAerial)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }
}
