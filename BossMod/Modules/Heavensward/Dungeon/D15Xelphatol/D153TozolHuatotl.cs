namespace BossMod.Heavensward.Dungeon.D15Xelphatol.D153TozolHuatotl;

public enum OID : uint
{
    Boss = 0x17A2, // R3.0
    AbalathianHornbill = 0x17A3, // R1.08
    Garuda = 0x17A4, // R2.89
    Helper = 0xD25
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    IxaliAero = 6611, // Boss->player, no cast, single-target
    IxaliAeroII = 6612, // Boss->self, 3.0s cast, range 40+R width 6 rect
    IxaliAeroIII = 6613, // Boss->self, 3.0s cast, range 50+R circle

    Hawk = 6614, // Boss->self, 5.0s cast, single-target
    Bill = 6618, // AbalathianHornbill->player, 5.0s cast, range 5 circle, spread
    IngurgitateVisual = 6616, // AbalathianHornbill->player, 5.0s cast, single-target
    Ingurgitate = 6617, // Helper->self, no cast, range 5 circle, stack

    SummonGaruda = 6615, // Boss->location, 4.0s cast, single-target
    EyeOfTheStorm = 6619, // Helper->self, 6.0s cast, range 10-20 donut
    MistralSong = 6620, // Garuda->self, 5.0s cast, range 30+R 120-degree cone
    WickedWheel = 6621, // Garuda->self, 6.0s cast, range 7 circle
    AerialBlast = 6622 // Garuda->self, 4.0s cast, range 50+R circle
}

public enum IconID : uint
{
    Stackmarker = 62 // player
}

class AerialBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.AerialBlast);
class IxaliAeroII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IxaliAeroII, new AOEShapeRect(43f, 3f));
class IxaliAeroIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.IxaliAeroIII);
class Bill(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Bill, 5f);
class Ingurgitate(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, (uint)AID.Ingurgitate, 5f, 5.5f, 4, 4);
class EyeOfTheStorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EyeOfTheStorm, new AOEShapeDonut(10f, 20f));
class WickedWheel(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WickedWheel, 7f);
class MistralSong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MistralSong, new AOEShapeCone(32.89f, 60f.Degrees()));

class D153TozolHuatotlStates : StateMachineBuilder
{
    public D153TozolHuatotlStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AerialBlast>()
            .ActivateOnEnter<IxaliAeroII>()
            .ActivateOnEnter<IxaliAeroIII>()
            .ActivateOnEnter<Bill>()
            .ActivateOnEnter<Ingurgitate>()
            .ActivateOnEnter<EyeOfTheStorm>()
            .ActivateOnEnter<WickedWheel>()
            .ActivateOnEnter<MistralSong>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5272, SortOrder = 7)]
public class D153TozolHuatotl(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(317.8f, -416.19f), 19.5f * CosPI.Pi48th, 48)], [new Rectangle(new(336.69f, -409.415f), 20f, 1f, 70f.Degrees())]);
}
