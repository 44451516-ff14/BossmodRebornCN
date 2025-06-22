namespace BossMod.Shadowbringers.Quest.Role.ToHaveLovedAndLost;

public enum OID : uint
{
    Boss = 0x2927,
    Helper = 0x233C,
}

public enum AID : uint
{
    Bloodstain = 4747, // Boss->self, 2.5s cast, range 5 circle
    BrandOfSin = 16132, // Boss->self, 3.0s cast, range 80 circle
    BladeOfJustice = 16134, // Boss->players, 8.0s cast, range 6 circle
    SanctifiedHolyII = 17427, // Boss->self, 3.0s cast, range 5 circle
    SanctifiedHolyIII = 17430, // 2AB3/2AB2->location, 3.0s cast, range 6 circle
    HereticsFork = 17552, // 2779->self, 4.0s cast, range 40 width 6 cross
    SpiritsWithout = 4746, // Boss->self, 2.5s cast, range 3+R width 3 rect
    SeraphBlade = 16131, // Boss->self, 5.0s cast, range 40+R ?-degree cone
    Fracture1 = 15576, // 2612->location, 5.0s cast, range 3 circle
    Fracture2 = 13207, // 2612->location, 5.0s cast, range 3 circle
    Fracture3 = 13208, // 2612->location, 5.0s cast, range 3 circle
    Fracture4 = 13209, // 2612->location, 5.0s cast, range 3 circle
    Fracture5 = 15374, // 2612->location, 5.0s cast, range 3 circle
    Fracture6 = 16612, // 2612->location, 5.0s cast, range 3 circle

    HereticsQuoit = 17470, // 2968->self, 5.0s cast, range -15 donut
    SanctifiedHoly1 = 17431, // 2AB3/2AB2->players/2928, 5.0s cast, range 6 circle
}

class HereticsFork(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HereticsFork, new AOEShapeCross(40f, 3f));
class SpiritsWithout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpiritsWithout, new AOEShapeRect(3.5f, 1.5f));
class SeraphBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeraphBlade, new AOEShapeCone(40f, 90f.Degrees()));
class HereticsQuoit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HereticsQuoit, new AOEShapeDonut(5f, 15f));
class SanctifiedHoly(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SanctifiedHoly1, 6f);

class Fracture(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Fracture1:
            case (uint)AID.Fracture2:
            case (uint)AID.Fracture3:
            case (uint)AID.Fracture4:
            case (uint)AID.Fracture5:
            case (uint)AID.Fracture6:
                Towers.Add(new(spell.LocXZ, 3f, activation: Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Towers.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.Fracture1:
                case (uint)AID.Fracture2:
                case (uint)AID.Fracture3:
                case (uint)AID.Fracture4:
                case (uint)AID.Fracture5:
                case (uint)AID.Fracture6:
                    var count = Towers.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        var tower = Towers[i];
                        if (tower.Position == spell.LocXZ)
                        {
                            Towers.Remove(tower);
                            break;
                        }
                    }
                    break;
            }
    }
}
class Bloodstain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bloodstain, 5f);
class BrandOfSin(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BrandOfSin, 10f);
class BladeOfJustice(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.BladeOfJustice, 6f, minStackSize: 1);
class SanctifiedHolyII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SanctifiedHolyII, 5f);
class SanctifiedHolyIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SanctifiedHolyIII, 6f);

class DikaiosyneStates : StateMachineBuilder
{
    public DikaiosyneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bloodstain>()
            .ActivateOnEnter<BrandOfSin>()
            .ActivateOnEnter<BladeOfJustice>()
            .ActivateOnEnter<SanctifiedHoly>()
            .ActivateOnEnter<SanctifiedHolyII>()
            .ActivateOnEnter<SanctifiedHolyIII>()
            .ActivateOnEnter<Fracture>()
            .ActivateOnEnter<HereticsFork>()
            .ActivateOnEnter<HereticsQuoit>()
            .ActivateOnEnter<SpiritsWithout>()
            .ActivateOnEnter<SeraphBlade>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68784, NameID = 8922)]
public class Dikaiosyne(WorldState ws, Actor primary) : BossModule(ws, primary, new(-798.6f, -40.49f), new ArenaBoundsCircle(20f));
