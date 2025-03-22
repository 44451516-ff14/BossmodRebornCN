namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class TorrentialTridentLanding(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TorrentialTridentLanding));

class TorrentialTridentAOE(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(6);
    private static readonly AOEShapeCircle _shape = new(18f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 5 ? 5 : count;
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        if (count > 1)
            aoes[0].Color = Colors.Danger;
        return aoes[..max];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TorrentialTridentLanding:
                AOEs.Add(new(_shape, WPos.ClampToGrid(caster.Position), default, WorldState.FutureTime(13.8d)));
                break;
            case (uint)AID.TorrentialTridentAOE:
                ++NumCasts;
                if (AOEs.Count != 0)
                    AOEs.RemoveAt(0);
                break;
        }
    }
}
