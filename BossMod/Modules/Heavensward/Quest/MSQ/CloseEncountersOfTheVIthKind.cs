namespace BossMod.Heavensward.Quest.MSQ.CloseEncountersOfTheVIthKind;

public enum OID : uint
{
    Boss = 0xF1C, // R0.550, x?
    Puddle = 0x1E88F5, // R0.500, x?
    TerminusEst = 0xF5D, // R1.000, x?
}

public enum AID : uint
{
    HandOfTheEmpire = 4000, // Boss->location, 2.0s cast, range 2 circle
    TerminusEstBoss = 4005, // Boss->self, 3.0s cast, range 50 circle
    TerminusEstAOE = 3825, // TerminusEst->self, no cast, range 40+R width 4 rect
}

class RegulaVanHydrusStates : StateMachineBuilder
{
    public RegulaVanHydrusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<HandOfTheEmpire>();
    }
}

class HandOfTheEmpire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HandOfTheEmpire, 2f);
class Voidzone(BossModule module) : Components.Voidzone(module, 8f, GetPuddles)
{
    private static List<Actor> GetPuddles(BossModule module) => module.Enemies((uint)OID.Puddle);
}

class TerminusEst(BossModule module) : Components.GenericAOEs(module, (uint)AID.TerminusEstAOE)
{
    private bool _active;
    private static readonly AOEShapeRect rect = new(40, 2);

    public static List<Actor> GetTerminusEst(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.TerminusEst);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var terminus = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                terminus.Add(z);
        }
        return terminus;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(GetTerminusEst(Module), Colors.Danger, true);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var terminus = GetTerminusEst(Module);
        var count = terminus.Count;
        if (!_active || count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var t = terminus[i];
            aoes[i] = new(rect, t.Position.Quantized(), t.Rotation);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TerminusEstBoss)
            _active = true;
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.TerminusEst)
            _active = false;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67203, NameID = 3818)]
public class RegulaVanHydrus(WorldState ws, Actor primary) : BossModule(ws, primary, new(252.75f, 553f), new ArenaBoundsCircle(19.5f));

