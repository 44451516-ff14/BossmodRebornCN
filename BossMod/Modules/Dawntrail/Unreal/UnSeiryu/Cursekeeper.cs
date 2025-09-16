namespace BossMod.Dawntrail.Unreal.UnSeiryu;

sealed class Cursekeeper(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private ulong prevTarget;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Cursekeeper)
        {
            prevTarget = spell.TargetID;
            if (WorldState.Actors.Find(prevTarget) is Actor t)
            {
                CurrentBaits.Add(new(Module.PrimaryActor, t, circle, Module.CastFinishAt(spell, 3.1d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.InfirmSoul)
        {
            ++NumCasts;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Role == Role.Tank && Module.PrimaryActor.TargetID == prevTarget)
        {
            hints.Add(prevTarget != actor.InstanceID ? "Provoke!" : "Pass aggro!");
        }
        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.Role == Role.Tank && actor.InstanceID is var id && prevTarget != id && Module.PrimaryActor.TargetID != id)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Provoke), actor, ActionQueue.Priority.High);
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void Update()
    {
        if (CurrentBaits.Count != 0 && WorldState.Actors.Find(Module.PrimaryActor.TargetID) is Actor t)
        {
            CurrentBaits.Ref(0).Target = t;
        }
    }
}

sealed class KarmicCurse(BossModule module) : Components.RaidwideInstant(module, (uint)AID.KarmicCurse, 4d)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action.ID == (uint)AID.InfirmSoul)
        {
            Activation = WorldState.FutureTime(Delay);
        }
    }
}
