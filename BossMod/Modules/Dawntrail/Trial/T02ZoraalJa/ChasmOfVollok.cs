namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

class ChasmOfVollok(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly float platformOffset = 30 / MathF.Sqrt(2);
    private static readonly AOEShapeRect rect = new(2.5f, 2.5f, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChasmOfVollok1)
        {
            if (Arena.InBounds(caster.Position))
                AOEs.Add(new(rect, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            else
            {
                // the visual cast happens on one of the side platforms at intercardinals, offset by 30
                var offset = new WDir(caster.Position.X > Arena.Center.X ? -platformOffset : +platformOffset, caster.Position.Z > Arena.Center.Z ? -platformOffset : +platformOffset);
                AOEs.Add(new(rect, caster.Position + offset, spell.Rotation, Module.CastFinishAt(spell)));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChasmOfVollok1 or AID.ChasmOfVollok2)
            AOEs.Clear();
    }
}
