namespace BossMod.Dawntrail.Hunt.RankA.Yehehetoauapyo;

public enum OID : uint
{
    Boss = 0x43DB // R6.250, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    WhirlingOmen1 = 38626, // Boss->self, 5.0s cast, single-target, left
    WhirlingOmen2 = 38627, // Boss->self, 5.0s cast, single-target, right
    WhirlingOmen3 = 38628, // Boss->self, 5.0s cast, single-target, left --> right
    WhirlingOmen4 = 38629, // Boss->self, 5.0s cast, single-target, left -> left
    WhirlingOmen5 = 38630, // Boss->self, 5.0s cast, single-target, right -> left
    WhirlingOmen6 = 38631, // Boss->self, 5.0s cast, single-target, right -> right
    WhirlingOmenRaidwide = 39878, // Boss->self, 5.0s cast, range 50 circle
    DactailToTurnspit = 38633, // Boss->self, 5.0s cast, single-target, back, turn, front
    TurntailToPteraspit = 38635, // Boss->self, 5.0s cast, single-target, turn, back, front
    TurnspitToDactail = 38634, // Boss->self, 5.0s cast, single-target, turn, front, back
    PteraspitToTurntail = 38632, // Boss->self, 5.0s cast, single-target, front, turn, back
    Dactail1 = 38637, // Boss->self, 0.8s cast, range 40 150-degree cone, front
    Dactail2 = 38639, // Boss->self, 0.8s cast, range 40 150-degree cone, turn right -> back
    Dactail3 = 38641, // Boss->self, 0.8s cast, range 40 150-degree cone, turn left -> back
    Pteraspit1 = 38638, // Boss->self, 0.8s cast, range 40 150-degree cone, turn left -> front
    Pteraspit2 = 38636, // Boss->self, 0.8s cast, range 40 150-degree cone, back
    Pteraspit3 = 38640, // Boss->self, 0.8s cast, range 40 150-degree cone, turn right -> front
}

class WhirlingOmenRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WhirlingOmenRaidwide), "Raidwide, no turn buffs this time!");

class WhirlingOmen(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private List<Angle> offsets = new(2);
    private static readonly Angle a90 = 90f.Degrees(), a180 = 180f.Degrees();
    private static readonly AOEShapeCone cone = new(40f, 75f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
            aoes[0].Color = Colors.Danger;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (offsets.Count == 0)
            offsets = spell.Action.ID switch
            {
                (uint)AID.WhirlingOmen1 => [a90],
                (uint)AID.WhirlingOmen2 => [-a90],
                (uint)AID.WhirlingOmen3 => [a90, -a90],
                (uint)AID.WhirlingOmen4 => [a90, a90],
                (uint)AID.WhirlingOmen5 => [-a90, a90],
                (uint)AID.WhirlingOmen6 => [-a90, -a90],
                _ => []
            };
        else // there might be no offsets if player joined fight late
        {
            var offset0 = offsets[0];
            switch (spell.Action.ID)
            {
                case (uint)AID.PteraspitToTurntail:
                    AddAOEs(default, offset0 + a180);
                    break;
                case (uint)AID.DactailToTurnspit:
                    AddAOEs(a180, offset0);
                    break;
                case (uint)AID.TurnspitToDactail:
                    AddAOEs(offset0, offset0 + a180);
                    break;
                case (uint)AID.TurntailToPteraspit:
                    AddAOEs(offset0 + a180, offset0);
                    break;
            }

            void AddAOEs(Angle offset1, Angle offset2)
            {
                AddAOE(offset1, 1.4f);
                AddAOE(offset2, 3.8f);
                offsets.RemoveAt(0);
            }
            void AddAOE(Angle offset, float delay)
            => _aoes.Add(new(cone, spell.LocXZ, spell.Rotation + offset, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.Dactail1:
                case (uint)AID.Dactail2:
                case (uint)AID.Dactail3:
                case (uint)AID.Pteraspit1:
                case (uint)AID.Pteraspit2:
                case (uint)AID.Pteraspit3:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class YehehetoauapyoStates : StateMachineBuilder
{
    public YehehetoauapyoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhirlingOmenRaidwide>()
            .ActivateOnEnter<WhirlingOmen>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin, Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13400)]
public class Yehehetoauapyo(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
