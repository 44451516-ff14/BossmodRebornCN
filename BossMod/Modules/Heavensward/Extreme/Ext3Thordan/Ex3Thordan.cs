namespace BossMod.Heavensward.Extreme.Ex3Thordan;

class AscalonsMight(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.AscalonsMight), new AOEShapeCone(11.8f, 45.Degrees()));

abstract class HeavenlySlash(BossModule module, OID oid) : Components.Cleave(module, ActionID.MakeSpell(AID.HeavenlySlash), new AOEShapeCone(10.2f, 45.Degrees()), (uint)oid);
class HeavenlySlashAdelphel(BossModule module) : HeavenlySlash(module, OID.SerAdelphel);
class HeavenlySlashJanlenoux(BossModule module) : HeavenlySlash(module, OID.SerJanlenoux);

class Meteorain(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MeteorainAOE), 6);
class AscalonsMercy(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AscalonsMercy), new AOEShapeCone(34.8f, 10.Degrees()));
class AscalonsMercyHelper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AscalonsMercyAOE), new AOEShapeCone(34.5f, 10.Degrees()));
class DragonsRage(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DragonsRage), 6, 8, 8);
class LightningStorm(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.LightningStormAOE), 5);
class Heavensflame(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavensflameAOE), 6);
class Conviction(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.ConvictionAOE), 3);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, ActionID.MakeSpell(AID.HolyChain));
class SerZephirin(BossModule module) : Components.Adds(module, (uint)OID.SerZephirin);

class BossReappear(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BossReappear));
class LightOfAscalon(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.LightOfAscalon), true)
{
    private readonly List<Source> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (_sources.Count > 0 && (AID)spell.Action.ID == AID.LightOfAscalon)
            _sources.RemoveAt(0);
        else if ((AID)spell.Action.ID == AID.BossReappear)
        {
            for (var i = 0; i < 7; ++i)
                _sources.Add(new(new(-0.822f, -16.314f), 3, WorldState.FutureTime(10.7f + i * 1.3f))); // knockback originates from a helper
        }
    }
}

class UltimateEnd(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.UltimateEndAOE));
class HeavenswardLeap(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.HeavenswardLeap));
class PureOfSoul(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.PureOfSoul));
class AbsoluteConviction(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.AbsoluteConviction));

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(21, 24);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        yield return new(donut, Arena.Center);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus), veyn (from Unreal Thordan)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 91, NameID = 3632, PlanLevel = 60)]
public class Ex3Thordan(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(23.5f))
{
    public static readonly ArenaBoundsCircle DefaultBounds = new(21);
}
