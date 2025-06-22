namespace BossMod.Endwalker.Hunt.RankA.Gurangatch;

public enum OID : uint
{
    Boss = 0x361B // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    LeftHammerSlammer = 27493, // Boss->self, 5.0s cast, range 30 180-degree cone
    RightHammerSlammer = 27494, // Boss->self, 5.0s cast, range 30 180-degree cone
    LeftHammerSecond = 27495, // Boss->self, 1.0s cast, range 30 180-degree cone
    RightHammerSecond = 27496, // Boss->self, 1.0s cast, range 30 180-degree cone
    OctupleSlammerLCW = 27497, // Boss->self, 9.0s cast, range 30 180-degree cone
    OctupleSlammerRCW = 27498, // Boss->self, 9.0s cast, range 30 180-degree cone
    OctupleSlammerLCCW = 27521, // Boss->self, 9.0s cast, range 30 180-degree cone
    OctupleSlammerRCCW = 27522, // Boss->self, 9.0s cast, range 30 180-degree cone
    OctupleSlammerRestL = 27499, // Boss->self, 1.0s cast, range 30 180-degree cone
    OctupleSlammerRestR = 27500, // Boss->self, 1.0s cast, range 30 180-degree cone
    WildCharge = 27511, // Boss->players, no cast, width 8 rect charge
    BoneShaker = 27512 // Boss->self, 4.0s cast, range 30 circle
}

class Slammer(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(30f, 90f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddSequence(Angle increment, int casts = 8, int max = 2) => Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell), 3.6f, casts, max));
        switch (spell.Action.ID)
        {
            case (uint)AID.OctupleSlammerLCW:
            case (uint)AID.OctupleSlammerRCW:
                AddSequence(90f.Degrees());
                ImminentColor = Colors.Danger;
                break;
            case (uint)AID.OctupleSlammerLCCW:
            case (uint)AID.OctupleSlammerRCCW:
                AddSequence(-90f.Degrees());
                ImminentColor = Colors.Danger;
                break;
            case (uint)AID.LeftHammerSlammer:
            case (uint)AID.RightHammerSlammer:
                AddSequence(180f.Degrees(), 2, 1);
                ImminentColor = 0;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LeftHammerSlammer:
            case (uint)AID.RightHammerSlammer:
            case (uint)AID.LeftHammerSecond:
            case (uint)AID.RightHammerSecond:
            case (uint)AID.OctupleSlammerLCW:
            case (uint)AID.OctupleSlammerRCW:
            case (uint)AID.OctupleSlammerRestL:
            case (uint)AID.OctupleSlammerRestR:
            case (uint)AID.OctupleSlammerLCCW:
            case (uint)AID.OctupleSlammerRCCW:
                AdvanceSequence(0, WorldState.CurrentTime);
                break;
        }
    }
}

class BoneShaker(BossModule module) : Components.RaidwideCast(module, (uint)AID.BoneShaker);

class GurangatchStates : StateMachineBuilder
{
    public GurangatchStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Slammer>()
            .ActivateOnEnter<BoneShaker>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 10631)]
public class Gurangatch(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
