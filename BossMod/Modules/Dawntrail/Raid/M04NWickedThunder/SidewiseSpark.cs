namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

sealed class SidewiseSpark(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(60f, 90f.Degrees());
    private static readonly AOEShapeRect rect = new(40f, 8f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = aoe with { Color = count == 1 || count > 1 && aoe.Activation == _aoes[1].Activation ? 0 : Colors.Danger };
            else
                aoes[i] = aoe with { Risky = aoes[0].Activation == aoe.Activation };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        switch (spell.Action.ID)
        {
            case (uint)AID.SidewiseSpark1:
            case (uint)AID.SidewiseSpark2:
            case (uint)AID.SidewiseSpark3:
            case (uint)AID.SidewiseSpark4:
                AddAOE(cone);
                break;
            case (uint)AID.Burst:
                AddAOE(rect);
                break;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.WickedReplica)
        {
            switch (id)
            {
                case 0x11D8:
                    AddAOE(90f.Degrees());

                    break;
                case 0x11D6:
                    AddAOE(-90f.Degrees());
                    break;
            }
            void AddAOE(Angle offset) => _aoes.Add(new(cone, actor.Position, actor.Rotation + offset, WorldState.FutureTime(8d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.SidewiseSpark1:
                case (uint)AID.SidewiseSpark2:
                case (uint)AID.SidewiseSpark3:
                case (uint)AID.SidewiseSpark4:
                case (uint)AID.SidewiseSpark5:
                case (uint)AID.SidewiseSpark6:
                case (uint)AID.Burst:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}
