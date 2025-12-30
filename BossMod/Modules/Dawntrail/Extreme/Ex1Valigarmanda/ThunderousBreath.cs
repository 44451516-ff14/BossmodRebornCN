namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class ThunderousBreath : Components.CastCounter
{
    public ThunderousBreath(BossModule module) : base(module, (uint)AID.ThunderousBreathAOE)
    {
        var platform = module.FindComponent<ThunderPlatform>();
        if (platform != null)
        {
            var party = module.Raid.WithSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                var slot = party[i].Item1;
                platform.RequireHint[slot] = platform.RequireLevitating[slot] = true;
            }
        }
    }
}

sealed class ArcaneLighning(BossModule module) : Components.GenericAOEs(module, (uint)AID.ArcaneLightning)
{
    public readonly List<AOEInstance> AOEs = [];

    private readonly AOEShapeRect rect = new(50f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ArcaneSphere)
        {
            var pos = actor.Position.Quantized();
            var rot = actor.Rotation;
            AOEs.Add(new(rect, pos, rot, WorldState.FutureTime(8.6d), shapeDistance: rect.Distance(pos, rot)));
        }
    }
}
