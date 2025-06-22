namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class SwordQuiverRaidwide(BossModule module) : Components.CastCounter(module, default)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SwordQuiverRaidwide1 or (uint)AID.SwordQuiverRaidwide2 or (uint)AID.SwordQuiverRaidwide3 or (uint)AID.SwordQuiverRaidwide4)
            ++NumCasts;
    }
}

sealed class SwordQuiverBurst(BossModule module) : Components.GenericAOEs(module, (uint)AID.SwordQuiverBurst)
{
    private readonly List<AOEInstance> _aoes = new(4);

    private static readonly AOEShapeRect _shape = new(30f, 6f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SwordQuiverN or (uint)AID.SwordQuiverC or (uint)AID.SwordQuiverS)
        {
            var activation = Module.CastFinishAt(spell, 8.9f);
            var pos = Arena.Center;
            _aoes.Add(new(_shape, pos, -0.003f.Degrees(), activation));
            _aoes.Add(new(_shape, pos, 117.998f.Degrees(), activation));
            _aoes.Add(new(_shape, pos, -118.003f.Degrees(), activation));

            var zOffset = spell.Action.ID switch
            {
                (uint)AID.SwordQuiverN => -10f,
                (uint)AID.SwordQuiverS => +10f,
                _ => 0
            };
            _aoes.Add(new(_shape, Arena.Center + new WDir(default, zOffset), 89.999f.Degrees(), activation));
        }
    }
}

sealed class SwordQuiverLaceration(BossModule module) : Components.CastCounter(module, (uint)AID.SwordQuiverLaceration)
{
    private static readonly AOEShapeCone _shape = new(40f, 30f.Degrees()); // TODO: verify angle

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var dir = Angle.FromDirection(actor.Position - Arena.Center);
        var clipped = Raid.WithoutSlot(false, true, true).Exclude(actor).InShape(_shape, Arena.Center, dir).CountByCondition(p => p.Class.IsSupport() == actor.Class.IsSupport());
        if (clipped.match != 0 || clipped.mismatch != 1)
            hints.Add("Spread by roles!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        _shape.Outline(Arena, Arena.Center, Angle.FromDirection(pc.Position - Arena.Center));
    }
}
