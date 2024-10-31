namespace BossMod.Stormblood.Dungeon.D05CastrumAbania.D051MagnaRoader;

public enum OID : uint
{
    Boss = 0x1AA9, // R3.2
    MarkXLIIIMiniCannon = 0x1AAC, // R2.0
    TwelfthLegionTriarius = 0x1AAB, // R0.5
    TwelfthLegionOptio = 0x1AAA, // R0.5
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // TwelfthLegionOptio->player, no cast, single-target

    MagitekFireII = 7957, // Boss->location, 3.0s cast, range 5 circle
    MagitekFireIII = 7958, // Boss->self, 3.0s cast, range 40+R circle

    WildSpeeVisual = 8318, // Boss->location, 6.0s cast, range 40+R width 6 rect
    HaywireVisual = 7959, // Boss->location, no cast, width 6 rect charge
    HaywireTelegraph = 7960, // Helper->location, 6.5s cast, range 40+R width 6 rect
    WildSpeed = 8184, // Helper->location, no cast, range 40+R width 6 rect

    MagitekPulseVisual1 = 7961, // MarkXLIIIMiniCannon->location, 3.0s cast, single-target
    MagitekPulseVisual2 = 8325, // TwelfthLegionTriarius->self, 3.0s cast, single-target
    MagitekPulse = 8336, // Helper->location, 3.0s cast, range 6 circle

    Wheel = 7956 // Boss->player, no cast, single-target, tankbuster
}

public enum SID : uint
{
    Fetters = 1399 // Helper->player/Helper/Boss, extra=0x0
}

class MagitekPulsePlayer(BossModule module) : BossComponent(module)
{
    private readonly WildSpeedHaywire _aoe = module.FindComponent<WildSpeedHaywire>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AI.AIManager.Instance?.Beh == null || !_aoe.ActiveAOEs(slot, actor).Any())
            return;
        var forbidden = new List<Func<WPos, float>>();
        var turrets = Module.Enemies(OID.MarkXLIIIMiniCannon).Where(x => x.IsTargetable).ToList();
        foreach (var t in turrets)
            forbidden.Add(ShapeDistance.InvertedCircle(t.Position, 3));
        var closestTurret = turrets.Closest(actor.Position);
        if (closestTurret != null)
        {
            var distance = (actor.Position - closestTurret.Position).LengthSq();
            if (forbidden.Count > 0 && distance > 9)
                hints.AddForbiddenZone(p => forbidden.Max(f => f(p)));
            else if (distance < 9)
            {
                hints.InteractWithTarget = closestTurret;
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.MagitekPulse), null, ActionQueue.Priority.High, targetPos: Module.PrimaryActor.PosRot.XYZ());
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.Enemies(OID.MarkXLIIIMiniCannon).Any(x => x.IsTargetable) && _aoe.ActiveAOEs(default, Actor.FakeActor).Any())
            hints.Add("Use the turrets to stun the boss!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!(Module.Enemies(OID.MarkXLIIIMiniCannon).Any(x => x.IsTargetable) && _aoe.ActiveAOEs(pcSlot, pc).Any()))
            return;
        foreach (var a in Module.Enemies(OID.MarkXLIIIMiniCannon).Where(x => x.IsTargetable))
            Arena.AddCircle(a.Position, 3, Colors.Safe);
    }
}

class WildSpeedHaywire(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40.5f, 3);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        for (var i = 1; i < _aoes.Count; ++i)
            yield return _aoes[i];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HaywireTelegraph)
            _aoes.Add(new(rect, caster.Position, spell.Rotation, Module.CastFinishAt(spell, -0.2f))); // actual dmg AOE happens ~0.2s before cast ends
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.WildSpeed)
            _aoes.RemoveAt(0);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Fetters && actor == Module.PrimaryActor)
            _aoes.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveAOEs(slot, actor).Any() && Module.Enemies(OID.MarkXLIIIMiniCannon).Any(x => x.IsTargetable))
        { }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class MagitekPulse(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekPulse), 6);
class MagitekFireII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekFireII), 5);
class MagitekFireIII(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MagitekFireIII));

class D051MagnaRoaderStates : StateMachineBuilder
{
    public D051MagnaRoaderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WildSpeedHaywire>()
            .ActivateOnEnter<MagitekFireII>()
            .ActivateOnEnter<MagitekFireIII>()
            .ActivateOnEnter<MagitekPulsePlayer>()
            .ActivateOnEnter<MagitekPulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 242, NameID = 6263)]
public class D051MagnaRoader(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(-212.99f, 186.99f), 19.55f)], [new Rectangle(new(-213, 167), 20, 1.25f), new Rectangle(new(-213, 208), 20, 2.1f)]);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.TwelfthLegionOptio).Concat([PrimaryActor]));
        Arena.Actors(Enemies(OID.MarkXLIIIMiniCannon), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var comp = FindComponent<WildSpeedHaywire>()?.ActiveAOEs(slot, actor).Any();
        if (comp != null && (bool)comp && Enemies(OID.MarkXLIIIMiniCannon).Any(x => x.IsTargetable))
            foreach (var e in hints.PotentialTargets)
                e.Priority = -1; // targeting anything will leave the cannon
        else
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.TwelfthLegionOptio => 2,
                    OID.Boss => 1,
                    _ => 0
                };
            }
    }
}
