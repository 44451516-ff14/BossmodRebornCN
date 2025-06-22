namespace BossMod.Stormblood.Quest.MSQ.HisForgottenHome;

public enum OID : uint
{
    Boss = 0x213A,
    SoftshellOfTheRed = 0x213B, // R1.6
    SoftshellOfTheRed1 = 0x213C, // R1.6
    SoftshellOfTheRed2 = 0x213D, // R1.6
    Helper = 0x233C
}

public enum AID : uint
{
    Kasaya = 8585, // SoftshellOfTheRed->self, 2.5s cast, range 6+R 120-degree cone
    WaterIII = 5831, // Boss->location, 3.0s cast, range 8 circle
    BlizzardIII = 1087, // Boss->location, 3.0s cast, range 5 circle
}

class Kasaya(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Kasaya, new AOEShapeCone(7.6f, 60f.Degrees()));
class WaterIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaterIII, 8f);

class BlizzardIIIIcon(BossModule module) : Components.BaitAwayIcon(module, 5f, 26u)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlizzardIII)
            CurrentBaits.Clear();
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor == Module.PrimaryActor)
            CurrentBaits.Clear();
    }
}
class BlizzardIIICast(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.BlizzardIII, m => m.Enemies(0x1E8D9C).Where(x => x.EventState != 7), 0);

class SlickshellCaptainStates : StateMachineBuilder
{
    public SlickshellCaptainStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<Kasaya>()
            .ActivateOnEnter<BlizzardIIIIcon>()
            .ActivateOnEnter<BlizzardIIICast>()
            .Raw.Update = () => Module.Raid.Player()?.IsDeadOrDestroyed ?? true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68563, NameID = 6891)]
public class SlickshellCaptain(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom([new(464.25f, 320.19f), new(455.65f, 313.35f), new(457.72f, 308.20f), new(445.00f, 292.92f), new(468.13f, 283.56f),
    new(495.55f, 299.63f), new(487.19f, 313.73f)])]);

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // attack anyone targeting isse
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = hints.PotentialTargets[i];
            h.Priority = WorldState.Actors.Find(h.Actor.TargetID)?.OID == 0x2138 ? 1 : 0;
        }
    }
}

