namespace BossMod.Heavensward.Extreme.Ex3Thordan;

class AscalonsMight(BossModule module) : Components.Cleave(module, (uint)AID.AscalonsMight, new AOEShapeCone(11.8f, 45f.Degrees()));

abstract class HeavenlySlash(BossModule module, OID oid) : Components.Cleave(module, (uint)AID.HeavenlySlash, new AOEShapeCone(10.2f, 45f.Degrees()), [(uint)oid]);
class HeavenlySlashAdelphel(BossModule module) : HeavenlySlash(module, OID.SerAdelphel);
class HeavenlySlashJanlenoux(BossModule module) : HeavenlySlash(module, OID.SerJanlenoux);

class Meteorain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorainAOE, 6);
class AscalonsMercy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AscalonsMercy, new AOEShapeCone(34.8f, 10f.Degrees()));
class AscalonsMercyHelper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AscalonsMercyAOE, new AOEShapeCone(34.5f, 10f.Degrees()));
class DragonsRage(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DragonsRage, 6f, 8, 8);
class LightningStorm(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.LightningStormAOE, 5f);
class Heavensflame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavensflameAOE, 6f);
class Conviction(BossModule module) : Components.CastTowers(module, (uint)AID.ConvictionAOE, 3f);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, (uint)AID.HolyChain);
class SerZephirin(BossModule module) : Components.Adds(module, (uint)OID.SerZephirin);

class BossReappear(BossModule module) : Components.CastCounter(module, (uint)AID.BossReappear);
class LightOfAscalon(BossModule module) : Components.GenericKnockback(module, (uint)AID.LightOfAscalon, true)
{
    private readonly List<Knockback> _sources = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (_sources.Count > 0 && spell.Action.ID == (uint)AID.LightOfAscalon)
            _sources.RemoveAt(0);
        else if (spell.Action.ID == (uint)AID.BossReappear)
        {
            for (var i = 0; i < 7; ++i)
                _sources.Add(new(new(-0.822f, -16.314f), 3, WorldState.FutureTime(10.7d + i * 1.3d))); // knockback originates from a helper
        }
    }
}

class UltimateEnd(BossModule module) : Components.CastCounter(module, (uint)AID.UltimateEndAOE);
class HeavenswardLeap(BossModule module) : Components.CastCounter(module, (uint)AID.HeavenswardLeap);
class PureOfSoul(BossModule module) : Components.CastCounter(module, (uint)AID.PureOfSoul);
class AbsoluteConviction(BossModule module) : Components.CastCounter(module, (uint)AID.AbsoluteConviction);

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(21f, 24f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return new AOEInstance[1] { new(donut, Arena.Center) };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 91, NameID = 3632, PlanLevel = 60)]
public class Ex3Thordan(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(23.5f))
{
    public static readonly ArenaBoundsCircle DefaultBounds = new(21f);
}
