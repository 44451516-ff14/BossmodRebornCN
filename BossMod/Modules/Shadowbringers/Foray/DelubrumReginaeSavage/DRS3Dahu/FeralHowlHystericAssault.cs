namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

// these two abilities are very similar, only differ by activation delay and action id
// TODO: not all the wall is safe...
abstract class FeralHowlHystericAssault(BossModule module, uint aidCast, uint aidAOE, float delay) : Components.GenericKnockback(module, aidAOE, true, stopAtWall: true)
{
    private Knockback? _source;
    private HuntersClaw? _aoe = module.FindComponent<HuntersClaw>();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _source);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == aidCast)
            _source = new(caster.Position, 30f, Module.CastFinishAt(spell, delay));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _aoe ??= Module.FindComponent<HuntersClaw>();
        if (_aoe == null)
            return;
        var count = _aoe.Casters.Count;
        if (count == 0)
            return;
        if (_source is Knockback source)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
            var forbidden = new Func<WPos, float>[count];
            var pos = Module.PrimaryActor.Position;
            for (var i = 0; i < count; ++i)
            {
                var a = aoes[i].Origin;
                forbidden[i] = ShapeDistance.Cone(pos, 100f, Module.PrimaryActor.AngleTo(a), Angle.Asin(8f / (a - pos).Length()));
            }
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), source.Activation);
        }
    }
}

sealed class FeralHowl(BossModule module) : FeralHowlHystericAssault(module, (uint)AID.FeralHowl, (uint)AID.FeralHowlAOE, 2.1f);
sealed class HystericAssault(BossModule module) : FeralHowlHystericAssault(module, (uint)AID.HystericAssault, (uint)AID.HystericAssaultAOE, 0.9f);
