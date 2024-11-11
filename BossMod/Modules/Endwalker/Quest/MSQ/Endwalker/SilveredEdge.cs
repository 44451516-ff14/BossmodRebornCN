namespace BossMod.Endwalker.Quest.MSQ.Endwalker;

class SilveredEdge(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private bool active;
    private bool casting;
    private Angle _rotation;
    private static readonly Angle a120 = 120.Degrees(), a240 = 240.Degrees();
    private static readonly AOEShapeRect rect = new(40, 3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (active)
        {
            var pos = Module.PrimaryActor.Position;
            if (casting)
            {
                var angle = Angle.FromDirection(actor.Position - pos);
                yield return new(rect, pos, angle, _activation);
                yield return new(rect, pos, angle + a120, _activation);
                yield return new(rect, pos, angle + a240, _activation);
            }
            else
            {
                yield return new(rect, pos, _rotation, _activation);
                yield return new(rect, pos, _rotation + a120, _activation);
                yield return new(rect, pos, _rotation + a240, _activation);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SilveredEdge)
        {
            active = true;
            casting = true;
            _activation = Module.CastFinishAt(spell, 1.4f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SilveredEdge)
        {
            casting = false;
            _rotation = Angle.FromDirection(Raid.Player()!.Position - caster.Position);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SilveredEdgeVisual)
        {
            if (++NumCasts == 3)
            {
                NumCasts = 0;
                active = false;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!casting)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
