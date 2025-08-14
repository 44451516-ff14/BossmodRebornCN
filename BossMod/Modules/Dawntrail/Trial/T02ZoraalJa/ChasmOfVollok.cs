namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

sealed class ChasmOfVollok(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(16);
    private const float platformOffset = 21.2132f;
    private static readonly AOEShapeRect rect = new(5f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChasmOfVollok1)
        {
            if (Arena.InBounds(caster.Position))
                AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            else
            {
                var pos = spell.LocXZ;
                // the visual cast happens on one of the side platforms at intercardinals, offset by 30
                var offset = new WDir(pos.X > Arena.Center.X ? -platformOffset : +platformOffset, pos.Z > Arena.Center.Z ? -platformOffset : +platformOffset);
                AOEs.Add(new(rect, (pos + offset).Quantized(), spell.Rotation, Module.CastFinishAt(spell)));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChasmOfVollok1 or (uint)AID.ChasmOfVollok2)
            AOEs.Clear();
    }
}
