namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class Rampage(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(5);
    private static readonly AOEShapeCircle _shapeLast = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void Update()
    {
        if (AOEs.Count > 1)
        {
            AOEs.Ref(0).Color = Colors.Danger;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RampagePreviewCharge:
                var toDest = spell.LocXZ - caster.Position;
                AOEs.Add(new(new AOEShapeRect(toDest.Length(), 5f), caster.Position, Angle.FromDirection(toDest), Module.CastFinishAt(spell, 5.1d + 0.2d * AOEs.Count)));
                break;
            case (uint)AID.RampagePreviewLast:
                AOEs.Add(new(_shapeLast, spell.LocXZ, default, Module.CastFinishAt(spell, 6.3d)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RampageAOECharge or (uint)AID.RampageAOELast)
        {
            ++NumCasts;
            if (AOEs.Count != 0)
            {
                AOEs.RemoveAt(0);
            }
        }
    }
}
