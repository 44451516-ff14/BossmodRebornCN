namespace BossMod.Dawntrail.Raid.M06NSugarRiot;

sealed class SingleDoubleStyle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(12);
    private static readonly AOEShapeCircle circle = new(15f);
    private bool target;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        switch (tether.ID)
        {
            case (uint)TetherID.ActivateMechanicSingleStyle:
                HandleSingleStyle(source);
                break;
            case (uint)TetherID.ActivateMechanicDoubleStyle1:
                HandleDoubleStyle1(source);
                break;
            case (uint)TetherID.ActivateMechanicDoubleStyle2:
                HandleDoubleStyle2(source);
                break;
        }
    }

    private void HandleSingleStyle(Actor source)
    {
        switch (source.OID)
        {
            case (uint)OID.PaintBomb:
                AddAOE(source.Position, 8.7d);
                break;
            case (uint)OID.HeavenBomb:
                AddAOE(source.Position + 16f * source.Rotation.ToDirection(), 8.7d);
                break;
            case (uint)OID.SweetShot:
                HandleSweetShot(source, target ? 11.7d : 10.7d);
                break;
            case (uint)OID.ThrowUpTarget:
                target = true;
                break;
        }
    }

    private void HandleDoubleStyle1(Actor source)
    {
        switch (source.OID)
        {
            case (uint)OID.PaintBomb:
                AddAOE(source.Position, 9.6d);
                break;
            case (uint)OID.HeavenBomb:
                AddAOE(source.Position + 16f * source.Rotation.ToDirection(), 9.6d);
                break;
            case (uint)OID.ThrowUpTarget:
                target = true;
                // unfortunately target and arrow can be tethered in random order, so we need to update existing aoes
                var count = _aoes.Count;
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                for (var i = 0; i < count; ++i)
                {
                    ref var aoe = ref aoes[i];
                    var sourceP = aoe.Origin;
                    var direction = Angle.FromDirection(Arena.Center - sourceP).ToDirection();
                    var dirVector = WPos.ClampToGrid(sourceP + 60f * direction) - sourceP;
                    aoe.Shape = new AOEShapeRect(dirVector.Length(), 3.5f);
                    aoe.Rotation = Angle.FromDirection(dirVector);
                }
                break;
        }
    }

    private void HandleDoubleStyle2(Actor source)
    {
        switch (source.OID)
        {
            case (uint)OID.SweetShot:
                HandleSweetShot(source, target ? 10.7d : 9.6d);
                break;
            case (uint)OID.PaintBomb:
                AddAOE(source.Position, 9.6d);
                break;
        }
    }

    private void HandleSweetShot(Actor source, double time)
    {
        var sourceP = source.Position;
        var direction = target ? Angle.FromDirection(Arena.Center - sourceP).ToDirection() : source.Rotation.ToDirection();
        var dirVector = WPos.ClampToGrid(sourceP + 60f * direction) - sourceP;
        _aoes.Add(new(new AOEShapeRect(dirVector.Length(), 3.5f), WPos.ClampToGrid(sourceP), Angle.FromDirection(dirVector), WorldState.FutureTime(time)));
    }

    private void AddAOE(WPos position, double time)
    {
        _aoes.Add(new(circle, WPos.ClampToGrid(position), default, WorldState.FutureTime(time)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Burst1 or (uint)AID.Burst2 or (uint)AID.Rush)
        {
            _aoes.Clear();
            target = false;
        }
    }
}
