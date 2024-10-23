namespace BossMod.Endwalker.VariantCriterion.V02MR.V023Gorai;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos ArenaCenter = new(741, -190);
    public static readonly ArenaBoundsSquare StartingBounds = new(22.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
    public static readonly WPos octagonCenter = new(731, -200);
    private static readonly Square[] defaultSquare = [new Square(ArenaCenter, 20)];
    private static readonly AOEShapeCustom square = new([new Square(ArenaCenter, 23)], defaultSquare);
    private static readonly Angle rotation = 22.5f.Degrees();
    private static readonly ArenaBoundsComplex octagonTrap = new(defaultSquare, [new Polygon(octagonCenter, 8.5f, 8, rotation)],
    [new Polygon(octagonCenter, 7.5f, 8, rotation)]);

    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Unenlightenment && Arena.Bounds == StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.5f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x16)
        {
            Arena.Bounds = DefaultBounds;
            _aoe = null;
        }
        else if (state == 0x00020001 && index == 0x3D)
            Arena.Bounds = octagonTrap;
        else if (state == 0x00080004 && index == 0x3D)
            Arena.Bounds = DefaultBounds;
    }
}
