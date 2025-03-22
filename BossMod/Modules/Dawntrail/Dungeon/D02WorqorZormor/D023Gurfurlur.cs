namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D023Gurfurlur;

public enum OID : uint
{
    Boss = 0x415F, // R7.000, x1
    AuraSphere = 0x4162, // R1.0
    BitingWind = 0x4160, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    HeavingHaymakerVisual = 36269, // Boss->self, 5.0s cast, single-target
    HeavingHaymaker = 36375, // Helper->self, 5.3s cast, range 60 circle

    Stonework = 36301, // Boss->self, 3.0s cast, single-target
    LithicImpact = 36302, // Helper->self, 6.8s cast, range 4 width 4 rect
    GreatFlood = 36307, // Helper->self, 7.0s cast, range 80 width 60 rect

    Allfire1 = 36303, // Helper->self, 7.0s cast, range 10 width 10 rect
    Allfire2 = 36304, // Helper->self, 8.5s cast, range 10 width 10 rect
    Allfire3 = 36305, // Helper->self, 10.0s cast, range 10 width 10 rect

    VolcanicDrop = 36306, // Helper->player, 5.0s cast, range 6 circle

    SledgeHammerMarker = 36315, // Helper->player, no cast, single-target
    Sledgehammer1 = 36313, // Boss->self/players, 5.0s cast, range 60 width 8 rect, line stack
    Sledgehammer2 = 36314, // Boss->self, no cast, range 60 width 8 rect
    Sledgehammer3 = 39260, // Boss->self, no cast, range 60 width 8 rect

    ArcaneStomp = 36319, // Boss->self, 3.0s cast, single-target

    ShroudOfEons1 = 36321, // AuraSphere->player, no cast, single-target
    ShroudOfEons2 = 36322, // AuraSphere->Boss, no cast, single-target

    EnduringGlory = 36320, // Boss->self, 6.0s cast, range 60 circle

    Windswrath1 = 36310, // Helper->self, 7.0s cast, range 40
    Windswrath2 = 39074, // Helper->self, 15.0s cast, range 40 circle

    Whirlwind = 36311 // Helper->self, no cast, range 5 circle
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D023Gurfurlur.ArenaCenter, 25f)], [new Square(D023Gurfurlur.ArenaCenter, 20f)]);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavingHaymaker && Arena.Bounds == D023Gurfurlur.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 0.6f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x18)
        {
            Arena.Bounds = D023Gurfurlur.DefaultBounds;
            _aoe = null;
        }
    }
}

class AuraSphere(BossModule module) : BossComponent(module)
{
    public static List<Actor> GetOrbs(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.AuraSphere);
        var count = orbs.Count;
        if (count == 0)
            return [];

        var filteredorbs = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = orbs[i];
            if (!z.IsDead)
                filteredorbs.Add(z);
        }
        return filteredorbs;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count != 0)
            hints.Add("Soak the orbs!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        if (count != 0)
        {
            var orbz = new Func<WPos, float>[count];
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            for (var i = 0; i < count; ++i)
            {
                var o = orbs[i];
                orbz[i] = ShapeDistance.InvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(0f, 1f), 0.5f, 0.5f, 0.5f);
            }
            hints.AddForbiddenZone(ShapeDistance.Intersection(orbz), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = GetOrbs(Module);
        var count = orbs.Count;
        for (var i = 0; i < count; ++i)
            Arena.AddCircle(orbs[i].Position, 1f, Colors.Safe);
    }
}

class SledgeHammer(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.SledgeHammerMarker), ActionID.MakeSpell(AID.Sledgehammer3), 4.9f);
class HeavingHaymaker(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HeavingHaymaker));
class LithicImpact(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LithicImpact), new AOEShapeRect(4f, 2f));
class Whirlwind(BossModule module) : Components.Voidzone(module, 5f, m => m.Enemies(OID.BitingWind), 7);

class GreatFlood(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.GreatFlood), 25f, kind: Kind.DirForward)
{
    private readonly Allfire _aoe = module.FindComponent<Allfire>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        var source = Casters[0];
        var component = _aoe.AOEs;
        if (_aoe.AOEs.Count == 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(source.Position, source.Rotation, 15f, default, 20f), Module.CastFinishAt(source.CastInfo));
    }
}

class Allfire(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(10f, 5f);
    private static readonly AOEShapeRect safespot = new(15f, 10f, InvertForbiddenZone: true);
    public readonly List<AOEInstance> AOEs = new(16);
    private bool first = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(AOEs);
        if (!first)
            return aoes;

        var max = count >= 12 ? 12 : count == 8 ? 8 : 4;
        var deadline = aoes[0].Activation.AddSeconds(1d);
        var color = Colors.Danger;
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation < deadline)
            {
                if (count > 4)
                    aoe.Color = color;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (first && spell.Action.ID is (uint)AID.Allfire1 or (uint)AID.Allfire2 or (uint)AID.Allfire3)
        {
            AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (AOEs.Count == 16)
                AOEs.SortBy(x => x.Activation);
        }
        else if (!first && spell.Action.ID == (uint)AID.GreatFlood)
        {
            AOEs.Add(new(safespot, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), Colors.SafeFromAOE));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.Allfire1 or (uint)AID.Allfire2 or (uint)AID.Allfire3 or (uint)AID.GreatFlood)
        {
            AOEs.RemoveAt(0);
            if (AOEs.Count == 0)
                first = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (AOEs.Count == 0)
            return;

        if (first)
        {
            base.AddHints(slot, actor, hints);
            return;
        }
        hints.Add("Wait inside safespot for knockback!", !AOEs[0].Check(actor.Position));
    }
}

class VolcanicDrop(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.VolcanicDrop), 6f);
class EnduringGlory(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EnduringGlory));
class Windswrath1Raidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Windswrath1));
class Windswrath2Raidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Windswrath2));
class GreatFloodRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GreatFlood));

class Windswrath(BossModule module, AID aid) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(aid), 15f);
class Windswrath1(BossModule module) : Windswrath(module, AID.Windswrath1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        var source = Casters[0];
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(source.Position, 5f), Module.CastFinishAt(source.CastInfo));
    }
}

class Windswrath2(BossModule module) : Windswrath(module, AID.Windswrath2)
{
    private enum Pattern { None, EWEW, WEWE }
    private Pattern CurrentPattern;
    private static readonly Angle a15 = 15f.Degrees(), a165 = 165f.Degrees(), a105 = 105f.Degrees(), a75 = 75f.Degrees();
    private readonly Whirlwind _aoe = module.FindComponent<Whirlwind>()!;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BitingWind)
        {
            if (actor.Position.Z == -180f)
                CurrentPattern = Pattern.EWEW;
            else if (actor.Position.Z == -210f)
                CurrentPattern = Pattern.WEWE;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return !Module.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        var source = Casters[0];

        var forbidden = new List<Func<WPos, float>>(4);

        if (_aoe.ActiveAOEs(slot, actor).Length != 0)
        {
            var act = Module.CastFinishAt(source.CastInfo);
            var timespan = (act - WorldState.CurrentTime).TotalSeconds;
            if (timespan <= 3d)
            {
                var patternWEWE = CurrentPattern == Pattern.WEWE;
                var origin = source.Position;
                forbidden.Add(ShapeDistance.InvertedCone(origin, 5f, patternWEWE ? a15 : -a15, a15));
                forbidden.Add(ShapeDistance.InvertedCone(origin, 5f, patternWEWE ? -a165 : a165, a15));
                forbidden.Add(ShapeDistance.InvertedCone(origin, 5f, patternWEWE ? a105 : -a105, a15));
                forbidden.Add(ShapeDistance.InvertedCone(origin, 5f, patternWEWE ? -a75 : a75, a15));
            }
            else
                forbidden.Add(ShapeDistance.InvertedCircle(source.Position, 8f));
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), act);
        }
    }
}

class D023GurfurlurStates : StateMachineBuilder
{
    public D023GurfurlurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<HeavingHaymaker>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<AuraSphere>()
            .ActivateOnEnter<LithicImpact>()
            .ActivateOnEnter<GreatFloodRaidwide>()
            .ActivateOnEnter<Allfire>()
            .ActivateOnEnter<GreatFlood>()
            .ActivateOnEnter<VolcanicDrop>()
            .ActivateOnEnter<EnduringGlory>()
            .ActivateOnEnter<SledgeHammer>()
            .ActivateOnEnter<Windswrath1>()
            .ActivateOnEnter<Windswrath1Raidwide>()
            .ActivateOnEnter<Windswrath2>()
            .ActivateOnEnter<Windswrath2Raidwide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12705)]
public class D023Gurfurlur(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-54f, -195f);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
}
