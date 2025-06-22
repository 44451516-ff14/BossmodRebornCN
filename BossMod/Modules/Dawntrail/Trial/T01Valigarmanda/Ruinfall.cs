namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class RuinfallAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RuinfallAOE, 6f);

class RuinfallKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RuinfallKB, 21f, stopAfterWall: true, kind: Kind.DirForward)
{
    private readonly RuinfallTower _tower = module.FindComponent<RuinfallTower>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        if (actor.Role != Role.Tank)
        {
            var source = Casters[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position, new WDir(default, 1f), 1f, default, 20f), Module.CastFinishAt(source.CastInfo));
            return;
        }
        var towers = _tower.Towers;
        var count = towers.Count;
        if (count == 0)
            return;
        var t0 = towers[0];
        var isDelayDeltaLow = (t0.Activation - WorldState.CurrentTime).TotalSeconds < 5d;
        var isActorInsideTower = false;
        if (t0.IsInside(actor))
            isActorInsideTower = true;
        if (isDelayDeltaLow && isActorInsideTower)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
    }
}

class RuinfallTower(BossModule module) : Components.CastTowers(module, (uint)AID.RuinfallTower, 6f, 2, 2)
{
    public override void Update()
    {
        if (Towers.Count == 0)
            return;
        var forbidden = Raid.WithSlot(false, true, true).WhereActor(p => p.Role != Role.Tank).Mask();
        foreach (ref var t in Towers.AsSpan())
            t.ForbiddenSoakers = forbidden;
    }
}
