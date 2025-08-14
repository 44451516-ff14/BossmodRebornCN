namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

sealed class Obey(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(50f, 7f);
    public static readonly AOEShapeDonut Donut = new(12f, 60f);
    private readonly List<AOEInstance> _aoes = new(3);
    private readonly DAL3SaunionDawon bossmod = (DAL3SaunionDawon)module;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is (uint)OID.FireBrandJump or (uint)OID.FrigidPulseJump && state is 0x00010002u or 0x00100020u or 0x00400080u)
        {
            if (bossmod.BossDawon?.IsDeadOrDestroyed ?? true)
            {
                return;
            }
            AOEShape? shape = actor.OID switch
            {
                (uint)OID.FireBrandJump => cross,
                (uint)OID.FrigidPulseJump => Donut,
                _ => null
            };
            if (shape != null)
            {
                Angle rotation = default;
                if (shape == cross)
                {
                    rotation = Angle.FromDirection(actor.Position.Quantized() - (_aoes.Count == 0 ? bossmod.BossDawon!.Position : _aoes[^1].Origin));
                }
                _aoes.Add(new(shape, actor.Position.Quantized(), rotation, _aoes.Count == 0 ? WorldState.FutureTime(16.2d) : _aoes[0].Activation.AddSeconds(6.4d * _aoes.Count)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.FireBrand or (uint)AID.FrigidPulseJump)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void Update()
    {
        if (_aoes.Count != 0)
        {
            if (bossmod.BossDawon?.IsDeadOrDestroyed ?? true)
            {
                _aoes.Clear();
            }
        }
    }
}
