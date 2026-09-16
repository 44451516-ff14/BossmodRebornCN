namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

sealed class ElementaryEvocation(BossModule module) : Components.GenericAOEs(module)
{
    private readonly OmniElementPanels _panels = module.FindComponent<OmniElementPanels>()!;
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCone _cone = new(30f, 30f.Degrees());
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        SortHelpers.SortAOEByActivation(_aoes);
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        // depending on how filled arena gets, limit both Evocation and Expansion to 2, or maybe 2 if both active
        var max = count > 2 ? 2 : count;
        return aoes[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        var panelId = actor.OID switch
        {
            (uint)OID.SwirlingOrb => (uint)OID.OmniElementIce,
            (uint)OID.BallOfFire => (uint)OID.OmniElementFire,
            (uint)OID.BallOfLevin => (uint)OID.OmniElementThunder,
            _ => default
        };

        if (panelId == default)
        {
            return;
        }

        var ballRotation = actor.Rotation;
        Actor? targetPanel = null;
        var panels = CollectionsMarshal.AsSpan(_panels.Actors);
        var count = panels.Length;
        for (var i = 0; i < count; i++)
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
            // -30, -90, -150
            var delay = distance.AlmostEqual(-30f.Degrees(), 0.1f) ? 0 : distance.AlmostEqual(-90f.Degrees(), 0.1f) ? 1 : 2;
            var activation = WorldState.FutureTime(8d + 2.4d * delay);
            _aoes.Add(new(_cone, Module.PrimaryActor.Position, panelRotation, activation));
            _aoes.Add(new(_cone, Module.PrimaryActor.Position, panelRotation + 180f.Degrees(), activation));
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
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }
}
