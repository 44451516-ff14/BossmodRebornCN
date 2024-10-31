namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D111SpectralThief;

public enum OID : uint
{

    Boss = 0x2DEC, // R0.875
    SpectralThief = 0x2DED, // R0.875
    Marker = 0x1EAED9,
    ChickenKnife = 0x2E71, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport1 = 20437, // Boss->location, no cast, single-target
    Teleport2 = 20501, // SpectralThief->location, no cast, single-target

    SpectralDream = 20427, // Boss->players, 4.0s cast, single-target, tankbuster
    SpectralWhirlwind = 20428, // Boss->self, 4.0s cast, range 60 circle, raidwide

    SpectralGustVisual = 21454, // Boss->self, no cast, single-target
    SpectralGust = 21455, // Helper->player, 6.0s cast, range 5 circle
    ChickenKnife = 20438, // Boss->self, 2.0s cast, single-target
    CowardsCunning = 20439, // ChickenKnife->self, 3.0s cast, range 60 width 2 rect

    Dash = 20435, // Boss->self, 3.0s cast, single-target
    Shadowdash = 20436, // Boss->self, 3.0s cast, single-target
    DashVisual1 = 20429, // Boss->self, no cast, single-target
    DashVisual2 = 20430, // SpectralThief->self, no cast, single-target
    DashVisual3 = 20431, // Boss->self, no cast, single-target
    DashVisual4 = 20432, // SpectralThief->self, no cast, single-target
    Papercutter1 = 20434, // Helper->self, no cast, range 80 width 14 rect
    Papercutter2 = 20433, // Helper->self, no cast, range 80 width 14 rect
    VacuumBlade1 = 20577, // Helper->self, no cast, range 15 circle
    VacuumBlade2 = 20578 // Helper->self, no cast, range 15 circle
}

public enum SID : uint
{
    Dash = 2193 // none->Boss/SpectralThief, extra=0xB0/0xB1/0xB4/0xB3/0xB2/0xB5
}

class SpectralWhirlwind(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SpectralWhirlwind));
class SpectralDream(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.SpectralDream));
class SpectralGust(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SpectralGust), 5);
class CowardsCunning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CowardsCunning), new AOEShapeRect(55, 1, 5));

class VacuumBladePapercutter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly CowardsCunning _aoe = module.FindComponent<CowardsCunning>()!;

    private static readonly AOEShapeCircle circle = new(15);
    private static readonly AOEShapeRect rect = new(40, 7, 40);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.Boss && (SID)status.ID == SID.Dash)
        {
            var activation = WorldState.FutureTime(8.1f);
            foreach (var e in Module.Enemies(OID.Marker))
            {
                switch (status.Extra)
                {
                    case 0xB0:
                        _aoes.Add(new(circle, e.Position, default, activation));
                        break;
                    case 0xB1:
                        _aoes.Add(new(rect, e.Position, Angle.AnglesCardinals[1], activation));
                        break;
                    case 0xB2:
                        _aoes.Add(new(rect, e.Position, Angle.AnglesCardinals[3], activation));
                        break;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Papercutter1 or AID.Papercutter2 or AID.VacuumBlade1 or AID.VacuumBlade2)
            _aoes.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.ActiveCasters.Any())
        { }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class D111SpectralThiefStates : StateMachineBuilder
{
    public D111SpectralThiefStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpectralWhirlwind>()
            .ActivateOnEnter<SpectralDream>()
            .ActivateOnEnter<SpectralGust>()
            .ActivateOnEnter<CowardsCunning>()
            .ActivateOnEnter<VacuumBladePapercutter>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9505)]
public class D111SpectralThief(WorldState ws, Actor primary) : BossModule(ws, primary, new(-680, 450), new ArenaBoundsSquare(19.5f));
