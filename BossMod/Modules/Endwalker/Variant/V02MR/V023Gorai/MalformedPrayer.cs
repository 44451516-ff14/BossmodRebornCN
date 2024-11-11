namespace BossMod.Endwalker.VariantCriterion.V02MR.V023Gorai;

class MalformedPrayer(BossModule module) : Components.GenericTowers(module)
{
    private readonly ImpurePurgation _aoe = module.FindComponent<ImpurePurgation>()!;

    private static readonly Dictionary<byte, WPos> towerSources = new()
    {
        {0x1F, new(741, -201)}, {0x23, new(741, -179)}, {0x25, new(730, -190)},
        {0x21, new(752, -190)}, {0x24, new(733.222f, -182.222f)},
        {0x26, new(733.222f, -197.778f)}, {0x20, new(748.778f, -197.778f)},
        {0x22, new(748.778f, -182.222f)}};

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && towerSources.TryGetValue(index, out var source))
            Towers.Add(new(source, 4, activation: WorldState.FutureTime(9)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Towers.Count > 0 && (AID)spell.Action.ID == AID.Burst)
            Towers.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.ActiveAOEs(slot, actor).Any() && !Towers.Any(x => x.IsInside(actor)))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, 1));
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void Update()
    {
        if (Towers.Count == 0)
            return;
        for (var i = 0; i < Towers.Count; ++i)
            Towers[i] = new(Towers[i].Position, Towers[i].Radius, i > 0 ? 0 : 1, i > 0 ? 0 : 4, default, Towers[i].Activation);
    }
}
