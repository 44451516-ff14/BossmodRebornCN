namespace BossMod.Stormblood.Quest.Job.Samurai.TheBattleOnBekko;

public enum OID : uint
{
    Boss = 0x1BF8,
    UgetsuSlayerOfAThousandSouls = 0x1BF9, // R0.5
    Voidzone = 0x1E8EA9, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    HissatsuKyuten = 8433, // Boss->self, 3.0s cast, range 5+R circle
    TenkaGoken = 9145, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    ShinGetsubaku = 8437, // 1BF9->location, 3.0s cast, range 6 circle
    MijinGiri = 8435, // 1BF9->self, 2.5s cast, range 80+R width 10 rect
    Ugetsuzan1 = 8439, // 1BF9->self, 2.5s cast, range 2-7 180-degree donut sector
    Ugetsuzan2 = 8440, // 1BF9->self, 2.5s cast, range 7-12 180-degree donut sector
    Ugetsuzan3 = 8441, // 1BF9->self, 2.5s cast, range 12-17 180-degree donut sector
    Ugetsuzan4 = 8442, // UgetsuSlayerOfAThousandSouls->self, 2.5s cast, range 17-22 180-degree donut sector
    KuruiYukikaze = 8446, // UgetsuSlayerOfAThousandSouls->self, 2.5s cast, range 44+R width 4 rect
    KuruiGekko1 = 8447, // UgetsuSlayerOfAThousandSouls->self, 2.0s cast, range 30 circle
    KuruiKasha1 = 8448, // UgetsuSlayerOfAThousandSouls->self, 2.5s cast, range 8+R ?-degree cone
}

class KuruiGekko(BossModule module) : Components.RaidwideCast(module, (uint)AID.KuruiGekko1);
class KuruiKasha(BossModule module) : Components.SimpleAOEs(module, (uint)AID.KuruiKasha1, new AOEShapeDonutSector(4.5f, 8.5f, 45.Degrees()));
class KuruiYukikaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.KuruiYukikaze, new AOEShapeRect(44, 2), 8);
class HissatsuKyuten(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HissatsuKyuten, 5.5f);
class TenkaGoken(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TenkaGoken, new AOEShapeCone(8.5f, 60.Degrees()));
class ShinGetsubaku(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShinGetsubaku, 6);
class ShinGetsubakuVoidzone(BossModule module) : Components.Voidzone(module, 4, m => m.Enemies((uint)OID.Voidzone).Where(e => e.EventState != 7));
class MijinGiri(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MijinGiri, new AOEShapeRect(80.5f, 5));

class Ugetsuzan(BossModule module) : Components.ConcentricAOEs(module, sectors)
{
    private static readonly Angle a90 = 90f.Degrees();
    private static readonly AOEShapeDonutSector[] sectors = [new(2f, 7f, a90), new(7f, 12f, a90), new(12f, 17f, a90), new(17f, 22f, a90)];
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Ugetsuzan1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell), spell.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.Ugetsuzan1 => 0,
            (uint)AID.Ugetsuzan2 => 1,
            (uint)AID.Ugetsuzan3 => 2,
            (uint)AID.Ugetsuzan4 => 3,
            _ => -1
        };
        AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2.5d), spell.Rotation);
    }
}

class UgetsuSlayerOfAThousandSoulsStates : StateMachineBuilder
{
    public UgetsuSlayerOfAThousandSoulsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HissatsuKyuten>()
            .ActivateOnEnter<TenkaGoken>()
            .ActivateOnEnter<ShinGetsubaku>()
            .ActivateOnEnter<ShinGetsubakuVoidzone>()
            .ActivateOnEnter<MijinGiri>()
            .ActivateOnEnter<Ugetsuzan>()
            .ActivateOnEnter<KuruiYukikaze>()
            .ActivateOnEnter<KuruiGekko>()
            .ActivateOnEnter<KuruiKasha>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68106, NameID = 6096)]
public class UgetsuSlayerOfAThousandSouls(WorldState ws, Actor primary) : BossModule(ws, primary, new(808.8f, 69.5f), new ArenaBoundsSquare(14));

