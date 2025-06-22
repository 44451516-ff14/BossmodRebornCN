namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class Rampage(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(5);

    private static readonly AOEShapeCircle _shapeLast = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        Span<AOEInstance> aoes = new AOEInstance[count];
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            var aoe = AOEs[i];
            aoes[i] = i == 0 ? count > 1 ? aoe with { Color = color } : aoe : aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RampagePreviewCharge:
                var toDest = spell.LocXZ - caster.Position;
                AOEs.Add(new(new AOEShapeRect(toDest.Length(), 5f), caster.Position, Angle.FromDirection(toDest), Module.CastFinishAt(spell, 5.1f + 0.2f * AOEs.Count)));
                break;
            case (uint)AID.RampagePreviewLast:
                AOEs.Add(new(_shapeLast, spell.LocXZ, default, Module.CastFinishAt(spell, 6.3f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RampageAOECharge or (uint)AID.RampageAOELast)
        {
            ++NumCasts;
            if (AOEs.Count != 0)
                AOEs.RemoveAt(0);
        }
    }
}
