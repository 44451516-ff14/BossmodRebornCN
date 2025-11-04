namespace BossMod.Stormblood.Trial.T05Yojimbo;

class Enchain(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _targets = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((tether.ID == 1 || tether.ID == 57) && source.Type == ActorType.Player)
            _targets.Add(source);
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if ((tether.ID == 1 || tether.ID == 57) && source.Type == ActorType.Player)
            _targets.Remove(source);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets.Count > 0)
            hints.Add(_targets.Contains(actor) ? "You're about to be bound" : "Prepare to free bound players");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_targets.Contains(actor))
            hints.AddForbiddenZone(new AOEShapeCircle(20f).Distance(Module.PrimaryActor.Position, default), DateTime.MaxValue);

        foreach (var ironChain in Module.Enemies((uint)OID.IronChain))
            hints.SetPriority(ironChain, 1);
    }
}
