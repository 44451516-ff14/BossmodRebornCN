namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

sealed class VerdantScarletPlume(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(3f, 12f);
    private static readonly AOEShapeRect rect = new(30f, 15f);
    private readonly List<AOEInstance> _aoes = new(12);
    private bool first = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (first)
        {
            AOEShape? shape = actor.OID switch
            {
                (uint)OID.VerdantPlume => donut,
                (uint)OID.ScarletPlume => circle,
                _ => null
            };
            if (shape != null)
                AddAOE(shape, actor.Position, WorldState.FutureTime(10.5d));
        }
    }

    private void AddAOE(AOEShape shape, WPos position, DateTime activation) => _aoes.Add(new(shape, position.Quantized(), default, activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Flutter)
        {
            var verdantPlumes = Module.Enemies((uint)OID.VerdantPlume);
            var scarletPlumes = Module.Enemies((uint)OID.ScarletPlume);
            var countV = verdantPlumes.Count;
            var countS = scarletPlumes.Count;
            var rot = spell.Rotation;
            var origin = spell.LocXZ;
            var dir = 28f * rot.ToDirection();
            var act = WorldState.FutureTime(8.5d);
            for (var i = 0; i < countV; ++i)
            {
                var pos = verdantPlumes[i].Position;
                if (rect.Check(pos, origin, rot))
                    AddAOE(donut, pos + dir, act);
            }
            for (var i = 0; i < countS; ++i)
            {
                var pos = scarletPlumes[i].Position;
                if (rect.Check(pos, origin, rot))
                    AddAOE(circle, pos + dir, act);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Burn or (uint)AID.Explosion)
        {
            _aoes.Clear();
            first = false;
        }
    }
}
