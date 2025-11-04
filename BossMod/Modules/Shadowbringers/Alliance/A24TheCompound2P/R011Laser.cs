namespace BossMod.Shadowbringers.Alliance.A24TheCompound2P;

sealed class R011Laser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly AOEShapeRect rect = new(70f, 7.5f);
    private readonly List<(WPos source, Angle rotation, Actor target)> _tethers = new(2); // tether target can teleport after tether got applied (in the same frame), leading to incorrect locations if used directly in OnTethered
    private int movedLasers;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => movedLasers != 0 ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Transfer1 && WorldState.Actors.Find(tether.Target) is Actor t)
        {
            _tethers.Add((source.Position, source.Rotation, t));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.R011LaserVisual)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 0.6d)));
        }
    }

    public override void Update()
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            var countT = _tethers.Count - 1;
            for (var j = countT; j >= 0; --j)
            {
                var t = _tethers[j];
                if (t.target.Position.AlmostEqual(aoe.Origin, 1f))
                {
                    aoe.Origin = t.source;
                    aoe.Rotation = t.rotation;
                    _tethers.RemoveAt(j);
                    ++movedLasers;
                    break;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.R011Laser:
                _aoes.Clear();
                movedLasers = 0;
                break;
            case (uint)AID.ForcedTransfer1:
            case (uint)AID.ForcedTransfer2:
                _tethers.Clear(); // other mechanics use the same tether ID
                break;
        }
    }
}
