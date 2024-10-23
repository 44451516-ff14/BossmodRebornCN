namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class RuinfallAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RuinfallAOE), new AOEShapeCircle(6));

class RuinfallKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.RuinfallKB), 21, stopAfterWall: true, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default && actor.Role != Role.Tank)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position, new Angle(), 1, 0, 20), source.Activation);
        }
    }
}

class RuinfallTower(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.RuinfallTower), 6, 2, 2)
{
    public override void Update()
    {
        if (Towers.Count == 0)
            return;
        var forbidden = Raid.WithSlot().WhereActor(p => p.Role != Role.Tank).Mask();
        foreach (ref var t in Towers.AsSpan())
            t.ForbiddenSoakers = forbidden;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Towers.Count > 0 && Towers.Any(x => x.IsInside(actor) && (Towers.FirstOrDefault().Activation - WorldState.CurrentTime).TotalSeconds < 5))
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
    }
}
