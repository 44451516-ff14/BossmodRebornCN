namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class Towerfall(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(30f, 5f);
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TerrestrialTitans)
        {
            AddAOE();
            AddAOE(180f.Degrees());
        }
        else if (spell.Action.ID == (uint)AID.Towerfall)
        {
            if (_aoes.Count == 4)
                _aoes.Clear();
            AddAOE(delay: 0f);
        }
        void AddAOE(Angle offset = default, float delay = 14.7f)
        => _aoes.Add(new(rect, spell.LocXZ, spell.Rotation + offset, Module.CastFinishAt(spell, delay)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Towerfall)
        {
            ++NumCasts;
        }
    }
}

sealed class FangedCrossing(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(21f, 3.5f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.GleamingFang1 && id == 0x11D1u)
            _aoes.Add(new(cross, WPos.ClampToGrid(actor.Position), actor.Rotation, WorldState.FutureTime(6d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FangedCrossing)
        {
            ++NumCasts;
        }
    }
}

sealed class TerrestrialTitans(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TerrestrialTitans, 5f);
