namespace BossMod.Endwalker.VariantCriterion.V01SS.V011Geryon;

class ColossalStrike(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ColossalStrike));

class ColossalCharge1(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.ColossalCharge1), 7f);
class ColossalCharge2(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.ColossalCharge2), 7f);

class ColossalLaunch(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ColossalLaunch), new AOEShapeRect(20f, 20f));
class ExplosionAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ExplosionAOE), 15);
class ExplosionDonut(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ExplosionDonut), new AOEShapeDonut(5f, 17f));

class ColossalSlam(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ColossalSlam), new AOEShapeCone(60f, 30f.Degrees()));
class ColossalSwing(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ColossalSwing), new AOEShapeCone(60f, 90f.Degrees()));

class SubterraneanShudder(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SubterraneanShudder));
class RunawaySludge(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RunawaySludge), 9);

class Shockwave(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = [];
    private static readonly AOEShapeRect _shape = new(40f, 40f);
    private readonly RunawaySludge _aoe = module.FindComponent<RunawaySludge>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            var act = Module.CastFinishAt(spell);
            // knockback rect always happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(Arena.Center, 15, act, _shape, spell.Rotation + 90f.Degrees(), Kind.DirForward));
            _sources.Add(new(Arena.Center, 15, act, _shape, spell.Rotation - 90f.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return !Module.InBounds(pos);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11442)]
public class V011Geryon(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position.X < -150f ? new(-213f, 101f) : primary.Position.X > 100f ? new(-213f, 101f) : default, new ArenaBoundsSquare(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.PowderKegRed));
        Arena.Actors(Enemies((uint)OID.PowderKegBlue));
    }
}
