namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class AuroralUppercut(BossModule module) : Components.GenericKnockback(module, ignoreImmunes: true)
{
    private Knockback? _source;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _source != null && actor.FindStatus(SID.Knockback) == null ? Utils.ZeroOrOne(ref _source) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = spell.Action.ID switch
        {
            (uint)AID.AuroralUppercut1 => 12f,
            (uint)AID.AuroralUppercut2 => 25f,
            (uint)AID.AuroralUppercut3 => 38f,
            _ => 0f
        };
        if (distance != 0f)
            _source = new(Arena.Center, distance, Module.CastFinishAt(spell, 1.6f));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (_source != null && status.ID == (uint)SID.Knockback)
        {
            NumCasts = 1;
            _source = null;
        }
    }
}

sealed class AuroralUppercutHint(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle a45 = 45f.Degrees(), a135 = 135f.Degrees(), a44 = 44f.Degrees(), a13 = 12.5f.Degrees(), a59 = 59f.Degrees();
    private static readonly WPos center = A11Prishe.ArenaCenter;
    private AOEInstance? _aoe;
    private static readonly AOEShapeCustom hintENVC00020001KB25 = new([new DonutSegmentHA(center, 4f, 10f, -144f.Degrees(), a44), new DonutSegmentHA(center, 4f, 10f, 36f.Degrees(), a44)],
    [new ConeHA(center, 10f, -a135, a13), new ConeHA(center, 10f, a45, a13)], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC02000100KB25 = new([new DonutSegmentHA(center, 4f, 10f, 126f.Degrees(), a44), new DonutSegmentHA(center, 4f, 10f, -54f.Degrees(), a44)],
    [new ConeHA(center, 10f, a135, a13), new ConeHA(center, 10f, -a45, a13)], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC00020001KB38 = new([new ConeHA(center, 5f, -a135, a13), new ConeHA(center, 5f, a45, a13)], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC02000100KB38 = new([new ConeHA(center, 5f, a135, a13), new ConeHA(center, 5f, -a45, a13)], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC00020001KB12 = new([new ConeHA(center, 5f, a135, a59), new ConeHA(center, 5f, -a45, a59),
    new ConeV(ArenaChanges.MiddleENVC00020001[0].Center + new WDir(-9f, -9f), 3, -a135, a45, 3),
    new ConeV(ArenaChanges.MiddleENVC00020001[1].Center + new WDir(9f, 9f), 3, a45, a45, 3),
    new Rectangle(center + new WDir(-3f, -15f), 5f, 1f), new Rectangle(center + new WDir(3f, 15f), 5f, 1f)], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom hintENVC02000100KB12 = new([new ConeHA(center, 5f, -a135, a59), new ConeHA(center, 5f, a45, a59),
    new ConeV(ArenaChanges.MiddleENVC02000100[0].Center + new WDir(9f, -9f), 3f, -a45, a45, 3),
    new ConeV(ArenaChanges.MiddleENVC02000100[1].Center + new WDir(-9f, 9f), 3, a135, a45, 3),
    new Rectangle(center + new WDir(-15f, 3f), 1f, 5f), new Rectangle(center + new WDir(15f, -3f), 1f, 5f)], InvertForbiddenZone: true);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = spell.Action.ID switch
        {
            (uint)AID.AuroralUppercut1 => 12f,
            (uint)AID.AuroralUppercut2 => 25f,
            (uint)AID.AuroralUppercut3 => 38f,
            _ => 0f
        };
        switch (distance)
        {
            case 12f:
                if (Arena.Bounds == ArenaChanges.ArenaENVC00020001)
                    SetAOE(hintENVC00020001KB12);
                else if (Arena.Bounds == ArenaChanges.ArenaENVC02000100)
                    SetAOE(hintENVC02000100KB12);
                break;
            case 25f:
                if (Arena.Bounds == ArenaChanges.ArenaENVC00020001)
                    SetAOE(hintENVC00020001KB25);
                else if (Arena.Bounds == ArenaChanges.ArenaENVC02000100)
                    SetAOE(hintENVC02000100KB25);
                break;
            case 38f:
                if (Arena.Bounds == ArenaChanges.ArenaENVC00020001)
                    SetAOE(hintENVC00020001KB38);
                else if (Arena.Bounds == ArenaChanges.ArenaENVC02000100)
                    SetAOE(hintENVC02000100KB38);
                break;
        }
        void SetAOE(AOEShapeCustom shape) => _aoe = new(shape, Arena.Center, default, Module.CastFinishAt(spell, 1.6f), Colors.SafeFromAOE);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (_aoe != null && status.ID == (uint)SID.Knockback)
            _aoe = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}
