namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V21Yozakura;

sealed class SealOfRiotousBloom(BossModule module) : Components.GenericAOEs(module)
{
    private enum Element { Fire, Water, Thunder, Wind }
    private readonly List<AOEInstance> _aoes = new(10);
    private readonly List<Element> elements = new(4);
    private static readonly AOEShapeCircle circle = new(9f);
    private static readonly AOEShapeDonut donut = new(5f, 60f);
    private static readonly AOEShapeCone cone = new(70f, 22.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 5 ? 5 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        Element? element = actor.OID switch
        {
            (uint)OID.Fire => Element.Fire,
            (uint)OID.Water => Element.Water,
            (uint)OID.Wind => Element.Wind,
            (uint)OID.Thunder => Element.Thunder,
            _ => null
        };
        if (element is Element e)
        {
            switch (state)
            {
                case 0x00100020u: // seals spawn
                    elements.Add(e);
                    break;
                case 0x00400080u: // seal activates
                    var count = elements.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        if (elements[i] == e)
                        {
                            ActivateAOE(e, WorldState.FutureTime(8.1d));
                            break;
                        }
                    }
                    break;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 4 && spell.Action.ID is (uint)AID.SealOfTheFireblossom or (uint)AID.SealOfTheWindblossom)
        {
            _aoes.RemoveRange(0, 5);
        }
    }

    private void ActivateAOE(Element element, DateTime activation)
    {
        switch (element)
        {
            case Element.Fire:
                AddAOE(circle, activation);
                break;
            case Element.Thunder:
                AddConeAOEs(Angle.AnglesCardinals, activation);
                break;
            case Element.Water:
                AddConeAOEs(Angle.AnglesIntercardinals, activation);
                break;
            case Element.Wind:
                AddAOE(donut, activation);
                break;
        }
        elements.Remove(element);
        var eCount = elements.Count;
        if (_aoes.Count == 5 && eCount != 0)
        {
            var act = WorldState.FutureTime(16.3d);
            for (var i = 0; i < eCount; ++i)
            {
                ActivateAOE(elements[i], act);
            }
        }

        void AddAOE(AOEShape shape, DateTime activation, Angle rotation = default) => _aoes.Add(new(shape, Arena.Center.Quantized(), rotation, activation));

        void AddConeAOEs(Angle[] angles, DateTime activation)
        {
            for (var i = 0; i < 4; ++i)
            {
                AddAOE(cone, activation, angles[i]);
            }
        }
    }
}
