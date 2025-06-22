namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeRect rect = new(20f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        if (count == 1)
        {
            aoes[0].Risky = true;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LegitimateForceFirstR:
                AddAOEs(caster, -90f, 90f);
                break;
            case (uint)AID.LegitimateForceFirstL:
                AddAOEs(caster, 90f, -90f);
                break;
        }
        void AddAOEs(Actor caster, float first, float second)
        {
            AddAOE(first);
            AddAOE(second, 3.1f, false);
            void AddAOE(float offset, float delay = default, bool first = true) => AOEs.Add(new(rect, caster.Position, spell.Rotation + offset.Degrees(), Module.CastFinishAt(spell, delay), first ? Colors.Danger : default, first));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LegitimateForceFirstL:
            case (uint)AID.LegitimateForceFirstR:
            case (uint)AID.LegitimateForceSecondL:
            case (uint)AID.LegitimateForceSecondR:
                ++NumCasts;
                if (AOEs.Count != 0)
                    AOEs.RemoveAt(0);
                break;
        }
    }
}
