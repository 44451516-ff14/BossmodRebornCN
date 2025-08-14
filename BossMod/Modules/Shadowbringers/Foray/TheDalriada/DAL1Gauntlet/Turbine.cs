namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class Turbine(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _kb;
    private readonly List<Square> squares = new(6);
    private readonly FlamingCyclone _aoe = module.FindComponent<FlamingCyclone>()!;
    private RelSimplifiedComplexPolygon poly = new();
    private bool polyInit;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _kb);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Turbine:
                _kb = new(spell.LocXZ, 15f, Module.CastFinishAt(spell));
                break;
            case (uint)AID.FlamingCyclone:
                squares.Add(new Square(spell.LocXZ, 10f)); // assume circles are squares for cheap PiP test
                break;
        }
        if (squares.Count == 6)
        {
            AOEShapeCustom combine = new(squares);
            poly = combine.GetCombinedPolygon(Arena.Center);
            polyInit = true;
            squares.Clear();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Turbine)
        {
            _kb = null;
            polyInit = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb is Knockback kb && polyInit)
        {
            // square intentionally slightly smaller to prevent sus knockback
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var center = Arena.Center;
                var polygon = poly;
                hints.AddForbiddenZone(p =>
                {
                    var projected = p + 15f * (p - center).Normalized();
                    if (projected.InSquare(center, 22f) && !polygon.Contains(projected - center))
                        return 1f;
                    return default;
                }, act);
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Module.InBounds(pos);
    }
}
