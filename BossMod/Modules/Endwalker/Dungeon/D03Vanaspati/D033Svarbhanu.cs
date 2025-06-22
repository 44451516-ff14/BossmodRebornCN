namespace BossMod.Endwalker.Dungeon.D03Vanaspati.D033Svarbhanu;

public enum OID : uint
{
    Boss = 0x33EB, // R=7.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    AetherialDisruption = 25160, // Boss->self, 7.0s cast, single-target
    ChaoticPulse = 27489, // Boss->self, no cast, single-target
    ChaoticUndercurrentRedVisual = 25164, // Helper->self, no cast, single-target
    ChaoticUndercurrentBlueVisual = 25165, // Helper->self, no cast, single-target
    ChaoticUndercurrentRedRect = 25162, // Helper->self, no cast, range 40 width 10 rect
    ChaoticUndercurrentBlueRect = 25163, // Helper->self, no cast, range 40 width 10 rect
    CosmicKissVisual = 25161, // Boss->self, no cast, single-target
    CosmicKissCircle = 25167, // Helper->location, 3.0s cast, range 6 circle
    CosmicKissRect = 25374, // Helper->self, no cast, range 50 width 10 rect, 15 knockback, away from source
    CosmicKissSpread = 25168, // Helper->player, 8.0s cast, range 6 circle
    CosmicKiss = 25169, // Helper->location, 6.0s cast, range 100 circle, knockback 13, away from source
    CrumblingSky = 25166, // Boss->self, 3.0s cast, single-target
    FlamesOfDecay = 25170, // Boss->self, 5.0s cast, range 40 circle
    GnashingOfTeeth = 25171 // Boss->player, 5.0s cast, single-target
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D033Svarbhanu.ArenaCenter, 25f)], [new Square(D033Svarbhanu.ArenaCenter, 20f)]);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlamesOfDecay && Arena.Bounds != D033Svarbhanu.DefaultArena)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 8.9f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001u && index == 0x07u)
        {
            Arena.Bounds = D033Svarbhanu.DefaultArena;
            _aoe = null;
        }
    }
}

class ChaoticUndercurrent(BossModule module) : Components.GenericAOEs(module)
{
    public enum Pattern { None, BBRR, RRBB, BRRB, RBBR }
    public Pattern currentPattern;
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeRect rect = new(40f, 5f);
    private static readonly Angle rotation = 90f.Degrees();
    private static readonly WPos[] coords = [new(280f, -142f), new(280f, -152f), new(280f, -162f), new(280f, -172f)];
    private CosmicKissKnockback? _kb;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnEventEnvControl(byte index, uint state)
    {
        // index 0x08
        // red blue blue red => 0x00400020, red (-142, -172), blue (-152, -162)
        // blue blue red red => 0x00020001, red (-142, -152), blue (-162, -172) 
        // blue red red blue => 0x00100008, red (-152, -162), blue (-172, -142)
        // red red blue blue => 0x01000080, red (-162, -172), blue (-152, -142)
        if (index == 0x08u)
        {
            currentPattern = state switch
            {
                0x00400020u => Pattern.RBBR,
                0x00020001u => Pattern.BBRR,
                0x00100008u => Pattern.BRRB,
                0x01000080u => Pattern.RRBB,
                _ => Pattern.None
            };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChaoticUndercurrentBlueVisual:
                AddAOEsForPattern(true);
                break;
            case (uint)AID.ChaoticUndercurrentRedVisual:
                AddAOEsForPattern(false);
                break;
            case (uint)AID.ChaoticUndercurrentRedRect:
            case (uint)AID.ChaoticUndercurrentBlueRect:
                AOEs.Clear();
                currentPattern = Pattern.None;
                break;
        }
    }

    private void AddAOEsForPattern(bool isBlue)
    {
        switch (currentPattern)
        {
            case Pattern.RBBR:
                AddAOEs(isBlue ? (1, 2) : (0, 3));
                break;
            case Pattern.BBRR:
                AddAOEs(isBlue ? (2, 3) : (0, 1));
                break;
            case Pattern.BRRB:
                AddAOEs(isBlue ? (0, 3) : (1, 2));
                break;
            case Pattern.RRBB:
                AddAOEs(isBlue ? (0, 1) : (2, 3));
                break;
        }
        void AddAOEs((int, int) indices)
        {
            AddAOE(coords[indices.Item1]);
            AddAOE(coords[indices.Item2]);
            void AddAOE(WPos pos)
            {
                var activation = WorldState.FutureTime(7.7d);
                AOEs.Add(new(rect, WPos.ClampToGrid(pos), rotation, activation));
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AOEs.Count == 0)
            return;
        _kb ??= Module.FindComponent<CosmicKissKnockback>();

        if (_kb!.Casters.Count != 0 && !_kb.IsImmune(slot, Module.CastFinishAt(_kb.Casters[0].CastInfo)))
        { } // remove forbidden zones while knockback is active to not confuse the AI
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class CosmicKissSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.CosmicKissSpread, 6f);
class CosmicKissCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CosmicKissCircle, 6f);

class CosmicKissRect(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(9);
    private static readonly AOEShapeRect rect = new(50f, 5f);
    private static readonly Angle rotation = -90f.Degrees();
    private static readonly WPos[] coords = [new(320f, -142), new(320f, -152), new(320f, -162), new(320f, -172)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 3 ? 3 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001u && _aoes.Count == 0)
        {
            var aoeSets = index switch
            {
                0x0A => // for set 09, 0A, 0C --> 1B, 1D, 1E --> 09, 0A, 0B
                [[0, 2, 3], [0, 1, 3], [1, 2, 3]],
                0x0B => // for set 09, 0B, 0C --> 1B, 1C, 1E --> 0A, 0B, 0C
                [[0, 1, 3], [0, 2, 3], [0, 1, 2]],
                _ => new List<int[]>()
            };

            if (aoeSets.Count != 0)
            {
                AddAOEs(aoeSets[0], 4.3d);
                AddAOEs(aoeSets[1], 9.5d);
                AddAOEs(aoeSets[2], 14.6d);
            }
            void AddAOEs(int[] indices, double delay)
            {
                for (var i = 0; i < 3; ++i)
                    _aoes.Add(new(rect, WPos.ClampToGrid(coords[indices[i]]), rotation, WorldState.FutureTime(delay)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.CosmicKissRect)
            _aoes.RemoveAt(0);
    }
}

class CosmicKissRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.CosmicKiss);

class CosmicKissKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.CosmicKiss, 13f)
{
    private static readonly Angle a90 = 90f.Degrees(), a45 = 45f.Degrees(), a180 = 180f.Degrees();
    private readonly ChaoticUndercurrent _aoe = module.FindComponent<ChaoticUndercurrent>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _aoe.AOEs.Count;
        for (var i = 0; i < count; ++i)
        {
            var z = _aoe.AOEs[i];
            if (z.Shape.Check(pos, z.Origin, z.Rotation))
                return true;
        }
        return !Module.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var component = _aoe.AOEs;
        var count = component.Count;
        if (count != 0 && Casters.Count != 0)
        {
            var source = Casters[0];
            var act = Module.CastFinishAt(source.CastInfo);
            if (IsImmune(slot, act))
                return;

            var forbidden = new List<Func<WPos, float>>(2);

            var hasMinus142 = false;
            var hasMinus152 = false;
            var hasMinus162 = false;
            var hasMinus172 = false;

            for (var i = 0; i < count; ++i)
            {
                var x = component[i].Origin;
                switch ((int)x.Z)
                {
                    case -142:
                        hasMinus142 = true;
                        break;
                    case -152:
                        hasMinus152 = true;
                        break;
                    case -162:
                        hasMinus162 = true;
                        break;
                    case -172:
                        hasMinus172 = true;
                        break;
                }
            }
            var pos = Arena.Center;
            if (hasMinus152 && hasMinus162)
            {
                forbidden.Add(ShapeDistance.InvertedCone(pos, 7f, default, a45));
                forbidden.Add(ShapeDistance.InvertedCone(pos, 7f, a180, a45));
            }
            else if (hasMinus142 && hasMinus172)
            {
                forbidden.Add(ShapeDistance.InvertedCone(pos, 7f, a90, a45));
                forbidden.Add(ShapeDistance.InvertedCone(pos, 7f, -a90, a45));
            }
            else if (hasMinus142 && hasMinus152)
                forbidden.Add(ShapeDistance.InvertedCone(pos, 7f, a180, a90));
            else
                forbidden.Add(ShapeDistance.InvertedCone(pos, 7f, default, a90));
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), Module.CastFinishAt(source.CastInfo));
        }
    }
}

class FlamesOfDecay(BossModule module) : Components.RaidwideCast(module, (uint)AID.FlamesOfDecay);
class GnashingOfTeeth(BossModule module) : Components.SingleTargetCast(module, (uint)AID.GnashingOfTeeth);

class D033SvarbhanuStates : StateMachineBuilder
{
    public D033SvarbhanuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<ChaoticUndercurrent>()
            .ActivateOnEnter<CosmicKissSpread>()
            .ActivateOnEnter<CosmicKissCircle>()
            .ActivateOnEnter<CosmicKissRect>()
            .ActivateOnEnter<CosmicKissKnockback>()
            .ActivateOnEnter<CosmicKissRaidwide>()
            .ActivateOnEnter<FlamesOfDecay>()
            .ActivateOnEnter<GnashingOfTeeth>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 789, NameID = 10719)]
public class D033Svarbhanu(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(24.5f))
{
    public static readonly WPos ArenaCenter = new(300f, -157f);
    public static readonly ArenaBoundsSquare DefaultArena = new(20f);
}
