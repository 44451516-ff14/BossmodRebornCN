namespace BossMod.Endwalker.Savage.P3SPhoinix;

sealed class HeatOfCondemnation(BossModule module) : Components.TankbusterTether(module, (uint)AID.HeatOfCondemnationAOE, (uint)TetherID.HeatOfCondemnation, 6f);
sealed class TrailOfCondemnationAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TrailOfCondemnationAOE, new AOEShapeRect(40f, 7.5f));
sealed class SearingBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SearingBreezeAOE, 6f);

sealed class Cinderwing(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftCinderwing, (uint)AID.RightCinderwing], new AOEShapeCone(60f, 90f.Degrees()));

sealed class DevouringBrand(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCross cross = new(40f, 5f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DevouringBrandAOE)
        {
            _aoe = [new(cross, spell.LocXZ, default, Module.CastFinishAt(spell, 2.2d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00080004u)
        {
            _aoe = [];
        }
    }
}

sealed class SunBirdLarge(BossModule module) : Components.Adds(module, (uint)OID.SunbirdLarge)
{
    public int FinishedTethers;
    public override void Update()
    {
        var comp = Module.FindComponent<BirdTether>();
        if (comp != null)
            FinishedTethers = comp.NumFinishedChains;
    }
}

sealed class SunBirdSmall(BossModule module) : Components.Adds(module, (uint)OID.SunbirdSmall);
sealed class DarkenedFireAdd(BossModule module) : Components.Adds(module, (uint)OID.DarkenedFire);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 807u, NameID = 10720u, PlanLevel = 90)]
public sealed class P3S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
