namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

sealed class CrackleHiss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrackleHiss, new AOEShapeCone(25f, 60f.Degrees()))
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arena.IsDawonArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class RipperClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RipperClaw, new AOEShapeCone(9f, 45f.Degrees()))
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arena.IsDawonArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class SpikeFlail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpikeFlail, new AOEShapeCone(25f, 30f.Degrees()))
{
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_arena.IsDawonArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }
}

sealed class LeftRightHammer(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(20f, 90f.Degrees());
    private readonly List<AOEInstance> _aoes = new(4);
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_arena.IsDawonArena)
            return [];

        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1.9d);
        var isNotLastSet = aoes[^1].Activation > deadline;
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation < deadline)
            {
                if (isNotLastSet)
                    aoe.Color = color;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftHammer1 or (uint)AID.LeftHammer2 or (uint)AID.RightHammer1 or (uint)AID.RightHammer2)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftHammer1 or (uint)AID.LeftHammer2 or (uint)AID.RightHammer1 or (uint)AID.RightHammer2)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
