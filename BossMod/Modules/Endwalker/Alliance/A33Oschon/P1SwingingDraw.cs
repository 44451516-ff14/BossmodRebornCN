namespace BossMod.Endwalker.Alliance.A33Oschon;

class P1SwingingDraw(BossModule module) : Components.GenericAOEs(module, (uint)AID.SwingingDrawAOE)
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeCone _shape = new(60f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var dir = spell.Action.ID switch
        {
            (uint)AID.SwingingDrawCW => -45f.Degrees(),
            (uint)AID.SwingingDrawCCW => 45f.Degrees(),
            _ => default
        };
        if (dir != default)
        {
            dir += Angle.FromDirection(caster.Position - Arena.Center);
            AOEs.Add(new(_shape, (Arena.Center + 25f * dir.ToDirection()).Quantized(), dir + 180f.Degrees(), Module.CastFinishAt(spell, 6.2d)));
        }
    }
}
