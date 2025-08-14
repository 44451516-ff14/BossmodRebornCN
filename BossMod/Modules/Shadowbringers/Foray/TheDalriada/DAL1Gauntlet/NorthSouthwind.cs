namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class NorthSouthwind(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _kb;
    private readonly Stormcall _aoe = module.FindComponent<Stormcall>()!;
    private readonly List<Shape> shapes = new(4);
    private RelSimplifiedComplexPolygon poly = new();
    private bool polyInit;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _kb);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.WindVisual:
                _kb = new(spell.LocXZ, 40f, Module.CastFinishAt(spell, 0.1d), direction: spell.Rotation, kind: Kind.DirForward);
                break;
            case (uint)AID.PainStormShadow:
                shapes.Add(new ConeV(spell.LocXZ, 35f, spell.Rotation, 65f.Degrees(), 32));
                break;
            case (uint)AID.PainfulGustShadow:
                shapes.Add(new Polygon(spell.LocXZ, 20f, 64));
                break;
            case (uint)AID.FrigidPulseShadow:
                shapes.Add(new DonutV(spell.LocXZ, 7.5f, 25f, 64)); // donut inner radius intentionally slightly smaller to prevent sus knockback
                break;
        }
        if (shapes.Count == 4)
        {
            AOEShapeCustom combine = new(shapes);
            poly = combine.GetCombinedPolygon(Arena.Center);
            polyInit = true;
            shapes.Clear();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NorthWind or (uint)AID.SouthWind)
        {
            _kb = null;
            polyInit = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb is Knockback kb)
        {
            // square intentionally slightly smaller to prevent sus knockback
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var center = Arena.Center;
                var dir = 40f * kb.Direction.ToDirection();
                if (polyInit)
                {
                    hints.AddForbiddenZone(p =>
                    {
                        var projected = p + dir;
                        if (projected.InSquare(center, 22f) && !poly.Contains(projected - center))
                            return 1f;
                        return default;
                    }, act);
                }
                else
                {
                    var aoes = _aoe.ActiveAOEs(slot, actor);
                    if (aoes.Length != 0)
                    {
                        ref readonly var aoe = ref aoes[0];
                        var pos = aoe.Origin;
                        hints.AddForbiddenZone(p =>
                        {
                            var projected = p + dir;
                            if (projected.InSquare(center, 22f) && !projected.InCircle(pos, 35f))
                                return 1f;
                            return default;
                        }, act);
                    }
                    else
                    {
                        hints.AddForbiddenZone(p =>
                        {
                            if ((p + dir).InSquare(center, 22f))
                                return 1f;
                            return default;
                        }, act);
                    }
                }
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (polyInit && poly.Contains(pos - Arena.Center))
        {
            return true;
        }
        var aoes = _aoe.AOEs;
        return aoes.Count != 0 && aoes.Ref(0).Check(pos) || !Module.InBounds(pos);
    }
}
