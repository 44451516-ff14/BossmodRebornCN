namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Art;

class Thricecull(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Thricecull));
class AcallamNaSenorach(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AcallamNaSenorach));
class DefilersDeserts(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DefilersDeserts), new AOEShapeRect(35.5f, 4f));
class Pitfall(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Pitfall), 20f);
class LegendaryGeasAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LegendaryGeas), 8f);

class DefilersDesertsPredict(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(35.5f, 4f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(Angle angle) => _aoes.Add(new(cross, spell.LocXZ, angle, Module.CastFinishAt(spell, 6.9f)));
        if (spell.Action.ID == (uint)AID.LegendaryGeas)
        {
            AddAOE(45f.Degrees());
            AddAOE(default);
        }
        else if (spell.Action.ID == (uint)AID.DefilersDeserts)
            _aoes.Clear();
    }
}

class LegendaryGeasStay(BossModule module) : Components.StayMove(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ShadowLinksHelper && actor.Position.AlmostEqual(new(-135f, 750f), 1f))
        {
            if (state == 0x00010002)
                Array.Fill(PlayerStates, new(Requirement.Stay2, WorldState.CurrentTime, 1));
            else if (state == 0x00040008)
                Array.Clear(PlayerStates);
        }
    }
}

class GloryUnearthed(BossModule module) : Components.OpenWorldChasingAOEs(module, new AOEShapeCircle(10f), ActionID.MakeSpell(AID.GloryUnearthedFirst), ActionID.MakeSpell(AID.GloryUnearthedRest), 6.5f, 1.5f, 5, true, (uint)IconID.ChasingAOE);
class PiercingDark(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PiercingDark), 6f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.BaldesionArsenal, GroupID = 639, NameID = 7968, PlanLevel = 70, SortOrder = 1)]
public class BA1Art(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-128.98f, 748f), 29.5f, 64)], [new Rectangle(new(-129f, 718f), 20, 1.15f), new Rectangle(new(-129f, 778f), 20f, 1.48f),
    new Polygon(new(-123.5f, 778f), 1.7f, 8), new Polygon(new(-134.5f, 778f), 1.7f, 8), new Polygon(new(-123.5f, 718f), 1.5f, 8), new Polygon(new(-134.5f, 718f), 1.5f, 8)]);
}
