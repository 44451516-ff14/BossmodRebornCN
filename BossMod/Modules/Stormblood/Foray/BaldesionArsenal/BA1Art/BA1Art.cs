namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Art;

sealed class Thricecull(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Thricecull);
sealed class AcallamNaSenorach(BossModule module) : Components.RaidwideCast(module, (uint)AID.AcallamNaSenorach);
sealed class DefilersDeserts(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DefilersDeserts, new AOEShapeRect(35.5f, 4f));
sealed class Pitfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pitfall, 20f);
sealed class LegendaryGeasAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LegendaryGeas, 8f);

sealed class DefilersDesertsPredict(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(35.5f, 4f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(Angle angle) => _aoes.Add(new(cross, spell.LocXZ, angle, Module.CastFinishAt(spell, 6.9d)));
        var id = spell.Action.ID;
        if (id == (uint)AID.LegendaryGeas)
        {
            AddAOE(45f.Degrees());
            AddAOE(default);
        }
        else if (id == (uint)AID.DefilersDeserts)
        {
            _aoes.Clear();
        }
    }
}

sealed class LegendaryGeasStay(BossModule module) : Components.StayMove(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ShadowLinksHelper && actor.Position.AlmostEqual(new(-135f, 750f), 1f))
        {
            if (state == 0x00010002u)
            {
                Array.Fill(PlayerStates, new(Requirement.Stay2, WorldState.CurrentTime, 1));
            }
            else if (state == 0x00040008u)
            {
                Array.Clear(PlayerStates);
            }
        }
    }
}

sealed class GloryUnearthed(BossModule module) : Components.StandardChasingAOEs(module, 10f, (uint)AID.GloryUnearthedFirst, (uint)AID.GloryUnearthedRest, 6.5f, 1.5d, 5, true, (uint)IconID.GloryUnearthed);
sealed class PiercingDark(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.PiercingDark, 6f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.BaldesionArsenal, GroupID = 639, NameID = 7968, PlanLevel = 70, SortOrder = 1)]
public sealed class BA1Art(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-128.98f, 748f), 29.5f, 64)], [new Rectangle(new(-129f, 718f), 20, 1.15f), new Rectangle(new(-129f, 778f), 20f, 1.48f),
    new Polygon(new(-123.5f, 778f), 1.7f, 8), new Polygon(new(-134.5f, 778f), 1.7f, 8), new Polygon(new(-123.5f, 718f), 1.5f, 8), new Polygon(new(-134.5f, 718f), 1.5f, 8)]);

    protected override bool CheckPull() => base.CheckPull() && (Center - Raid.Player()!.Position).LengthSq() < 1e4f;
}
