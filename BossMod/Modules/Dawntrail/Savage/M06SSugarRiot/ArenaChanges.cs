using static BossMod.Dawntrail.Raid.SugarRiotSharedBounds.SugarRiotSharedBounds;

namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private bool _risky = true;
    private AOEInstance? _aoe;
    private bool active;
    public bool DangerousRiver => Arena.Bounds == RiverArena;
    public bool DangerousLava => Arena.Bounds == LavaArena;
    private static readonly PolygonCustom removeWestBridge = new([new(92.707f, 99.682f), new(88.088f, 101.596f), new(90.448f, 107.294f), new(95.068f, 105.382f)]);
    private static readonly PolygonCustom removeEastBridge = new([new(103.371f, 106.475f), new(107.338f, 109.518f), new(111.093f, 104.625f), new(107.127f, 101.58f)]);
    private static readonly PolygonCustom removeNorthBridge = new([new(103.923f, 93.843f), new(104.574f, 88.886f), new(98.459f, 88.081f), new(97.804f, 93.038f)]);
    private static readonly PolygonCustom[] combinedLava = [removeEastBridge, removeNorthBridge, removeWestBridge];
    public static readonly AOEShapeCustom LavaAOE = new(combinedLava);
    public static readonly ArenaBoundsComplex LavaArena = new(DefaultSquare, [.. CombinedRiver, .. combinedLava]);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x04)
        {
            switch (state)
            {
                case 0x00020001u:
                    active = true;
                    break;
                case 0x00800040u:
                    AddAOE(RiverAOE);
                    _risky = true;
                    break;
                case 0x02000100u:
                    AddAOE(LavaAOE);
                    break;
                case 0x00200010u:
                    SetArena(RiverArena);
                    active = false;
                    break;
            }
        }
        else if (index == 0x05)
        {
            switch (state)
            {
                case 0x00020001u:
                    SetArena(LavaArena);
                    break;
                case 0x00080004u:
                    Arena.Bounds = DefaultArena;
                    Arena.Center = ArenaCenter;
                    break;
            }
        }
        void AddAOE(AOEShapeCustom shape)
        => _aoe = new(shape, Arena.Center, default, WorldState.FutureTime(7.1d));
        void SetArena(ArenaBoundsComplex bounds)
        {
            Arena.Bounds = bounds;
            Arena.Center = bounds.Center;
            _aoe = null;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DoubleStyle3)
        {
            AddAOE(RiverAOE with { InvertForbiddenZone = true }, Colors.SafeFromAOE);
            _risky = false;
        }
        else if (spell.Action.ID == (uint)AID.DoubleStyle5)
        {
            AddAOE(RiverAOE);
        }
        void AddAOE(AOEShapeCustom shape, uint color = default)
        => _aoe = new(shape, Arena.Center, default, Module.CastFinishAt(spell, 4.2d), color);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TasteOfFire or (uint)AID.TasteOfThunderSpread)
        {
            _aoe = null;
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
