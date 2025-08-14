namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

sealed class MobileHaloCrossray(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeCross Cross = new(60f, 8.5f);
    public static readonly AOEShapeDonut Donut = new(9f, 60f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override bool KeepOnPhaseChange => true;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.MobileHaloVisual1 or (uint)AID.MobileHaloVisual2 or (uint)AID.MobileHaloVisual3 or (uint)AID.MobileHaloVisual4 => Donut,
            (uint)AID.MobileCrossrayVisual1 or (uint)AID.MobileCrossrayVisual2 or (uint)AID.MobileCrossrayVisual3 or (uint)AID.MobileCrossrayVisual4 => Cross,
            _ => null
        };
        if (shape != null)
        {
            WDir dir = spell.Action.ID switch
            {
                (uint)AID.MobileHaloVisual1 or (uint)AID.MobileCrossrayVisual1 => new(default, 18f),
                (uint)AID.MobileHaloVisual2 or (uint)AID.MobileCrossrayVisual2 => new(-18f, default),
                (uint)AID.MobileHaloVisual3 or (uint)AID.MobileCrossrayVisual3 => new(18f, default),
                _ => new(default, -18f)
            };
            _aoe = new(shape, (Arena.Center + dir).Quantized(), default, Module.CastFinishAt(spell, 2.1d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.MobileCrossray or (uint)AID.MobileHalo)
        {
            _aoe = null;
        }
    }
}
