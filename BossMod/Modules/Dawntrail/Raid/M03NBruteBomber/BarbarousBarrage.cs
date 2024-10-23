namespace BossMod.Dawntrail.Raid.M03NBruteBomber;

class BarbarousBarrageTower(BossModule module) : Components.GenericTowers(module)
{
    private static readonly WPos[] positions = [new(106, 94), new(94, 106), new(106, 106), new(94, 94)];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x01 && state == 0x00020004)
            foreach (var p in positions)
                Towers.Add(new(p, 4, 1, 1, default, WorldState.FutureTime(10)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Explosion)
            Towers.Clear();
    }
}

class BarbarousBarrageKnockback(BossModule module) : Components.Knockback(module)
{
    private static readonly AOEShapeCircle circle = new(4);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var towers = Module.FindComponent<BarbarousBarrageTower>()!.Towers;
        if (towers.Count != 0)
            foreach (var t in towers)
                yield return new(t.Position, 22, t.Activation, circle);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var towers = Module.FindComponent<BarbarousBarrageTower>()!.Towers;
        var isDelayDeltaLow = (towers.FirstOrDefault().Activation - WorldState.CurrentTime).TotalSeconds < 5;
        var isActorInsideTower = towers.Any(x => x.IsInside(actor));
        if (towers.Count > 0 && isDelayDeltaLow && isActorInsideTower)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Surecast), actor, ActionQueue.Priority.High);
        }
    }
}
