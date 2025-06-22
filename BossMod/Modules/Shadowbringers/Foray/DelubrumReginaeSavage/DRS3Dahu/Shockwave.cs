namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

sealed class Shockwave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCone cone = new(20f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftSidedShockwaveFirst or (uint)AID.RightSidedShockwaveFirst)
        {
            AddAOE();
            AddAOE(180f.Degrees(), 2.6f);
            void AddAOE(Angle offset = default, float delay = default) => _aoes.Add(new(cone, spell.LocXZ, spell.Rotation + offset, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.LeftSidedShockwaveFirst or (uint)AID.RightSidedShockwaveFirst or (uint)AID.LeftSidedShockwaveSecond or (uint)AID.RightSidedShockwaveSecond)
            _aoes.RemoveAt(0);
    }
}
