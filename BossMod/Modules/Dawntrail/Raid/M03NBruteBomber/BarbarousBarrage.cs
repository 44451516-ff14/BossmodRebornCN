namespace BossMod.Dawntrail.Raid.M03NBruteBomber;

sealed class BarbarousBarrageTower(BossModule module) : Components.GenericTowers(module)
{
    private static readonly WPos[] positions = [new(106f, 94f), new(94f, 106f), new(106f, 106f), new(94f, 94f)];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x01u && state == 0x00020004u)
            for (var i = 0; i < 4; ++i)
                Towers.Add(new(positions[i], 4f, 1, 1, default, WorldState.FutureTime(10d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion)
            Towers.Clear();
    }
}

sealed class BarbarousBarrageKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly BarbarousBarrageTower _tower = module.FindComponent<BarbarousBarrageTower>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var towers = _tower.Towers;
        var count = towers.Count;
        if (count == 0)
            return [];
        Span<Knockback> sources = new Knockback[count];
        for (var i = 0; i < count; ++i)
        {
            var t = towers[i];
            sources[i] = new(t.Position, 22f, t.Activation, circle);
        }
        return sources;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var towers = _tower.Towers;
        var count = towers.Count;
        if (count == 0)
            return;
        if ((towers[0].Activation - WorldState.CurrentTime).TotalSeconds < 5d)
        {
            for (var i = 0; i < count; ++i)
            {
                if (towers[i].IsInside(actor))
                {
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Surecast), actor, ActionQueue.Priority.High);
                    return;
                }
            }
        }
    }
}
