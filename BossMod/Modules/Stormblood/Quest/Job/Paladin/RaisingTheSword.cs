namespace BossMod.Stormblood.Quest.Job.Paladin.RaisingTheSword;

public enum OID : uint
{
    Boss = 0x1B51,
    AldisSwordOfNald = 0x18D6, // R0.5
    TaintedWindSprite = 0x1B52, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    ShudderingSwipeCast = 8136, // Boss->player, 3.0s cast, single-target
    ShudderingSwipeAOE = 8137, // AldisSwordOfNald->self, 3.0s cast, range 60+R 30-degree cone
    NaldsWhisper = 8141, // AldisSwordOfNald->self, 9.0s cast, range 4 circle
    VictorySlash = 8134, // Boss->self, 3.0s cast, range 6+R 120-degree cone
}

class VictorySlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VictorySlash, new AOEShapeCone(6.5f, 60f.Degrees()));
class ShudderingSwipeCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShudderingSwipeAOE, new AOEShapeCone(60f, 15f.Degrees()));
class ShudderingSwipeKB(BossModule module) : Components.GenericKnockback(module, (uint)AID.ShudderingSwipeCast, stopAtWall: true)
{
    private readonly TheFourWinds _aoe = module.FindComponent<TheFourWinds>()!;
    private Actor? _caster;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_caster is Actor c)
            return new Knockback[1] { new(c.Position, 10f, Module.CastFinishAt(c.CastInfo), null, c.AngleTo(actor), Kind.DirForward) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShudderingSwipeCast)
            _caster = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShudderingSwipeCast)
            _caster = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_caster is Actor source)
        {
            var aoes = _aoe.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            if (len == 0)
                return;
            var voidzones = new Func<WPos, float>[len];
            for (var i = 0; i < len; ++i)
            {
                voidzones[i] = new(ShapeDistance.Circle(aoes[i].Origin, 6f));
            }
            var windzone = ShapeDistance.Union(voidzones);
            float projectedDist(WPos pos)
            {
                var dir = source.DirectionTo(pos);
                var projected = pos + 10f * dir;
                return windzone(projected);
            }
            hints.AddForbiddenZone(projectedDist, Module.CastFinishAt(source.CastInfo));
        }
    }
}
class NaldsWhisper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaldsWhisper, 20f);
class TheFourWinds(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.TaintedWindSprite);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class AldisSwordOfNaldStates : StateMachineBuilder
{
    public AldisSwordOfNaldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheFourWinds>()
            .ActivateOnEnter<NaldsWhisper>()
            .ActivateOnEnter<VictorySlash>()
            .ActivateOnEnter<ShudderingSwipeKB>()
            .ActivateOnEnter<ShudderingSwipeCone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 270, NameID = 6311)]
public class AldisSwordOfNald(WorldState ws, Actor primary) : BossModule(ws, primary, new(-89.3f, 0), new ArenaBoundsCircle(20.5f));
