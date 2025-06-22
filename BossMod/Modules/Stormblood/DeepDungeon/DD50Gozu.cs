namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.DD50Gozu;

public enum OID : uint
{
    Boss = 0x23E9, // R3.45
    Gloom = 0x23EA, // R1.0
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    EyeOfTheFire = 11922, // Boss->self, 3.0s cast, range 40 circle, gaze mechanic
    Visual = 11923, // Boss->self, no cast, single-target, used after EyeOfTheFire
    RustingClaw = 11919, // Boss->self, 3.5s cast, range 8+R 120-degree cone
    TheSpin = 11921, // Boss->self, 7.5s cast, range 40+R circle, proximity AOE, optimal range around 20
    VoidSpark = 11924, // Gloom->self, 2.0s cast, range 7+R circle
    WordsOfWoe = 11920 // Boss->self, 3.0s cast, range 45+R width 6 rect
}

class EyeoftheFire(BossModule module) : Components.CastGaze(module, (uint)AID.EyeOfTheFire);
class RustingClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RustingClaw, new AOEShapeCone(11.45f, 60f.Degrees()));
class TheSpin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheSpin, 20f);
class VoidSpark(BossModule module) : Components.Voidzone(module, 8f, GetVoidzones)
{
    private static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.Gloom);
}
class WordsofWoe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WordsOfWoe, new AOEShapeRect(48.45f, 3f));

class DD50GozuStates : StateMachineBuilder
{
    public DD50GozuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EyeoftheFire>()
            .ActivateOnEnter<RustingClaw>()
            .ActivateOnEnter<TheSpin>()
            .ActivateOnEnter<VoidSpark>()
            .ActivateOnEnter<WordsofWoe>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 544, NameID = 7485)]
public class DD50Gozu(WorldState ws, Actor primary) : HoHArena2(ws, primary);
