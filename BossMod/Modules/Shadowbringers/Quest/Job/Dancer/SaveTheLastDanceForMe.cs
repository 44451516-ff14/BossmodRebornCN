namespace BossMod.Shadowbringers.Quest.Job.Dancer.SaveTheLastDanceForMe;

public enum OID : uint
{
    Boss = 0x2AC7, // R2.400, x1
    ShadowySpume = 0x2AC8, // R0.800, x0 (spawn during fight)
    ForebodingAura = 0x2ACB, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    Dread = 17476, // Boss->location, 3.0s cast, range 5 circle
    Anguish = 17487, // ->2ACD, 5.5s cast, range 6 circle
    WhelmingLossFirst = 17480, // AethericShadow->self, 5.0s cast, range 5 circle
    WhelmingLossRest = 17481, // AethericShadow1->self, no cast, range 5 circle
    BitterLove = 15650, // 2AC9->self, 3.0s cast, range 12 120-degree cone
}

class Dread(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dread, 5f);
class BitterLove(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitterLove, new AOEShapeCone(12f, 60f.Degrees()));
class WhelmingLoss(BossModule module) : Components.Exaflare(module, 5f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WhelmingLossFirst)
            Lines.Add(new(caster.Position, 5f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 1d, 7, 3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WhelmingLossFirst or (uint)AID.WhelmingLossRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
class Adds(BossModule module) : Components.Adds(module, (uint)OID.ShadowySpume);
class Anguish(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Anguish, 6f);

class ForebodingAura(BossModule module) : Components.Voidzone(module, 8f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ForebodingAura);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class AethericShadowStates : StateMachineBuilder
{
    public AethericShadowStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ForebodingAura>()
            .ActivateOnEnter<Anguish>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<WhelmingLoss>()
            .ActivateOnEnter<Dread>()
            .ActivateOnEnter<BitterLove>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68790, NameID = 8493)]
public class AethericShadow(WorldState ws, Actor primary) : BossModule(ws, primary, new(73.6f, -743.6f), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.FindStatus((uint)DNC.SID.ClosedPosition) == null && Raid.WithoutSlot(false, false).Exclude(actor).FirstOrDefault() is Actor partner)
        {
            hints.ActionsToExecute.Push(ActionID.MakeSpell(DNC.AID.ClosedPosition), partner, ActionQueue.Priority.VeryHigh);
        }
    }
}

