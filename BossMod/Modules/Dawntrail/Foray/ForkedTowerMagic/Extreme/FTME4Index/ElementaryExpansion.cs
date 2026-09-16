namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

sealed class ElementaryExpansion(BossModule module) : Components.GenericAOEs(module)
{
    // x6 rings (x12 EventCasts) twice
    private readonly OmniElementPanels _panels = module.FindComponent<OmniElementPanels>()!;
    private readonly ElementIII _element3 = module.FindComponent<ElementIII>()!;
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCone _cone = new(30f, 30f.Degrees());
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

        SortHelpers.SortAOEByActivation(_aoes);
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = count > 2 ? 2 : count;
        return aoes[..max];
    }

    // spawns actor for each elemental ring
    // creation vs renderflag only 0.04s apart, use creation
    // creation @109.856 -> 116.567 (6.7s)
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.ExpansionFire or (uint)OID.ExpansionIce or (uint)OID.ExpansionThunder)
        {
            var panelId = actor.OID switch
            {
                (uint)OID.ExpansionFire => (uint)OID.OmniElementFire,
                (uint)OID.ExpansionIce => (uint)OID.OmniElementIce,
                (uint)OID.ExpansionThunder => (uint)OID.OmniElementThunder,
                _ => default
            };

            if (panelId == default)
            {
                return;
            }

            var panels = CollectionsMarshal.AsSpan(_panels.Actors);
            var count = panels.Length;
            for (var i = 0; i < count; i++)
            {
                ref var panel = ref panels[i];
                if (panel.OID == panelId)
                {
                    var act = WorldState.FutureTime(6.7d);
                    var rotation = panel.Rotation;
                    _aoes.Add(new(_cone, Module.PrimaryActor.Position, rotation, act));
                    _aoes.Add(new(_cone, Module.PrimaryActor.Position, rotation + 180f.Degrees(), act));
                    break;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
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
    }
#if DEBUG
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        hints.Add($"Expansion[{NumCasts}]");
    }
#endif
}
