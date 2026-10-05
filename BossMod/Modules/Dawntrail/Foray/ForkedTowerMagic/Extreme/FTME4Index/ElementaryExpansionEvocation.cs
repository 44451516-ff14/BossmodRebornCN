namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

sealed class ElementaryExpansionEvocation(BossModule module) : Components.GenericAOEs(module)
{
    private readonly OmniElementPanels _panels = module.FindComponent<OmniElementPanels>()!;
    private readonly ElementIII _element3 = module.FindComponent<ElementIII>()!;
    private readonly List<AOEInstance> _aoes = [with(12)];
    private readonly AOEShapeCone _cone = new(30f, 30f.Degrees());
    private DateTime _lastCastEvent = default;
    private int _expRingCount = 0;
    private int _evoOrbCount = 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        if (_element3.ActiveAOEs(slot, actor).Length != 0)
        {
            return [];
        }

        // evocation only happens with expansion but all orbs spawn before any rings
        // wait until a ring appears so it doesn't display all evocation spots first
        if (_evoOrbCount > 0 && _expRingCount == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var limit = _evoOrbCount > 0 ? 4 : 2;
        var max = count > limit ? limit : count;
        return aoes[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is uint oid && oid is >= (uint)OID.ExpansionFire and <= (uint)OID.ExpansionThunder or >= (uint)OID.SwirlingOrb and <= (uint)OID.BallOfLevin)
        {
            var panelId = oid switch
            {
                (uint)OID.ExpansionFire => (uint)OID.OmniElementFire,
                (uint)OID.ExpansionIce => (uint)OID.OmniElementIce,
                (uint)OID.ExpansionThunder => (uint)OID.OmniElementThunder,
                (uint)OID.SwirlingOrb => (uint)OID.OmniElementIce,
                (uint)OID.BallOfFire => (uint)OID.OmniElementFire,
                (uint)OID.BallOfLevin => (uint)OID.OmniElementThunder,
                _ => default
            };

            if (panelId == default)
            {
                return;
            }

            var panels = CollectionsMarshal.AsSpan(_panels.Actors);
            var panelcount = panels.Length;

            // Elementary Expansion
            if (oid is (uint)OID.ExpansionFire or (uint)OID.ExpansionIce or (uint)OID.ExpansionThunder)
            {
                for (var i = 0; i < panelcount; ++i)
                {
                    ref var panel = ref panels[i];
                    if (panel.OID == panelId)
                    {
                        ++_expRingCount;
                        var act = WorldState.FutureTime(6.75d);
                        var rotation = panel.Rotation;
                        // use actorID to store OID for potential filtering
                        _aoes.Add(new(_cone, Module.PrimaryActor.Position, rotation, act, actorID: panelId));
                        _aoes.Add(new(_cone, Module.PrimaryActor.Position, rotation + 180f.Degrees(), act, actorID: panelId));
                        // sort AOEs as evocation orbs spawn before any expansion rings and orbs don't necessarily spawn in order unlike rings
                        SortHelpers.SortAOEByActivation(_aoes);
                        break;
                    }
                }
            }
            // Elementary Evocation
            else if (oid is (uint)OID.BallOfFire or (uint)OID.SwirlingOrb or (uint)OID.BallOfLevin)
            {
                var ballRotation = actor.Rotation;
                Actor? targetPanel = null;
                for (var i = 0; i < panelcount; ++i)
                {
                    ref var panel = ref panels[i];
                    if (panel.OID == panelId)
                    {
                        targetPanel = panel;
                        break;
                    }
                }

                if (targetPanel == null)
                {
                    return;
                }

                var panelRotation = targetPanel.Rotation;
                var distance = ballRotation.DistanceToAngle(panelRotation);
                var degrees = distance.Deg;
                if (degrees < 0f)
                {
                    _evoOrbCount += 2;
                    // -30, -90, -150
                    var delay = distance.AlmostEqual(-30f.Degrees(), 0.1f) ? 0 : distance.AlmostEqual(-90f.Degrees(), 0.1f) ? 1 : 2;
                    var activation = WorldState.FutureTime(8.58d + 4.35d * delay);
                    // use actorID to store OID for potential filtering
                    _aoes.Add(new(_cone, Module.PrimaryActor.Position, panelRotation, activation, actorID: panelId));
                    _aoes.Add(new(_cone, Module.PrimaryActor.Position, panelRotation + 180f.Degrees(), activation, actorID: panelId));
                }
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.ExpansionFire or (uint)OID.ExpansionIce or (uint)OID.ExpansionThunder && _expRingCount > 0)
        {
            _expRingCount -= 1;
        }
        else if (actor.OID is (uint)OID.BallOfFire or (uint)OID.SwirlingOrb or (uint)OID.BallOfLevin && _evoOrbCount > 0)
        {
            _evoOrbCount -= 1;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        /*
        - omni-element order:
        - Evocation: Fire -> Lightning -> Ice
        - Expansion: Ice  -> Fire      -> Lightning
                     333  -> 338       -> 342
        - Evocation: Ice  -> Fire      -> Lightning
        - Expansion: Fire -> Lightning -> Ice
                     360  -> 364       -> 369
        */
        /*
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.FireIV:
                case (uint)AID.BlizzardIV:
                case (uint)AID.ThunderIV:
                    ++NumCasts;
                    _aoes.RemoveAt(0);
                    break;
            }
        }
        */
        // remove range instead of one at a time
        // can have 0-0.5s between castevents for expansion and evocation; does this impact AI dodging?
        if (_aoes.Count != 0 && (WorldState.CurrentTime - _lastCastEvent).TotalSeconds > 1d)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.FireIV:
                case (uint)AID.BlizzardIV:
                case (uint)AID.ThunderIV:
                    _lastCastEvent = WorldState.CurrentTime;
                    var limit = _evoOrbCount > 0 ? 4 : 2;
                    NumCasts += limit;
                    _aoes.RemoveRange(0, limit);
                    break;
            }
        }
    }
}
