using static BossMod.Dawntrail.Raid.SugarRiotSharedBounds.SugarRiotSharedBounds;

namespace BossMod.Dawntrail.Raid.M06NSugarRiot;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private bool _risky = true;
    private AOEInstance? _aoe;
    private bool active;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x04)
            return;
        switch (state)
        {
            case 0x00020001u:
                active = true;
                break;
            case 0x00800040u:
                _aoe = new(RiverAOE, Arena.Center, default, WorldState.FutureTime(7d));
                _risky = true;
                break;
            case 0x00200010u:
                Arena.Bounds = RiverArena;
                Arena.Center = RiverArena.Center;
                active = false;
                _aoe = null;
                break;
            case 0x08000004u:
                Arena.Bounds = DefaultArena;
                Arena.Center = ArenaCenter;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoe != null)
            return;
        if (spell.Action.ID == (uint)AID.TasteOfFire)
        {
            _aoe = new(RiverAOE with { InvertForbiddenZone = true }, Arena.Center, default, Module.CastFinishAt(spell), Colors.SafeFromAOE);
            _risky = false;
        }
        else if (spell.Action.ID == (uint)AID.TasteOfThunder)
        {
            _aoe = new(RiverAOE, Arena.Center, default, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TasteOfFire or (uint)AID.TasteOfThunder)
        {
            _aoe = null;
            _risky = true;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!active)
            return;
        if (_aoe == null)
            return;
        var isInside = _aoe.Value.Check(actor.Position);
        if (!_risky)
        {
            hints.Add("Be inside river!", !isInside);
            return;
        }
        if (isInside)
            hints.Add("GTFO from river!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (!active)
            return;
        var pos = actor.Position;
        if (actor.PrevPosition != pos)
        {
            hints.WantJump = IntersectJumpEdge(pos, (pos - actor.PrevPosition).Normalized(), 1f);
        }
    }
}
