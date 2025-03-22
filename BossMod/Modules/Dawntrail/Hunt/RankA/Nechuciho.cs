namespace BossMod.Dawntrail.Hunt.RankA.Nechuciho;

public enum OID : uint
{
    Boss = 0x452C // R3.42
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    CastWordOfTheWood = 39872, // Boss->self, 5.0s cast, range 30 180-degree cone
    WordOfTheWoodVisual = 39496, // Boss->self, 7.0s cast, single-target
    WordOfTheWood1 = 39785, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood2 = 39786, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood3 = 39787, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood4 = 39788, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood5 = 42164, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood6 = 42165, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood7 = 42166, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood8 = 42167, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood9 = 42168, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood10 = 42169, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood11 = 42170, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWood12 = 42171, // Boss->self, no cast, range 30 180-degree cone

    SentinelRoar = 39491, // Boss->self, 5.0s cast, range 40 circle
    Level5DeathSentence = 39492, // Boss->self, 5.0s cast, range 30 circle
    WhisperOfTheWood1 = 39493, // Boss->self, 3.0s cast, single-target
    WhisperOfTheWood2 = 39494, // Boss->self, 3.0s cast, single-target
    WhisperOfTheWood3 = 39495 // Boss->self, 3.0s cast, single-target
}

class Level5DeathSentence(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.Level5DeathSentence), true, false, "Applies Doom!");
class SentinelRoar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SentinelRoar));
class WordOfTheWood(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CastWordOfTheWood), new AOEShapeCone(30f, 90f.Degrees()));

class WhispersOfTheWood(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle a180 = 180f.Degrees(), a90 = 90f.Degrees();
    private static readonly AOEShapeCone cone = new(30, a90);
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
            {
                if (aoes[0].Rotation.AlmostEqual(aoe.Rotation + a180, Angle.DegToRad))
                    aoe.Risky = false;
            }
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.WhisperOfTheWood1:
                AddAOEs([default, -a90, a90, a180]);
                break;
            case (uint)AID.WhisperOfTheWood2:
                AddAOEs([a90, default, a180, -a90]);
                break;
            case (uint)AID.WhisperOfTheWood3:
                AddAOEs([a180, a90, -a90, default]);
                break;
        }
        void AddAOEs(Angle[] angles)
        {
            for (var i = 0; i < 4; ++i)
            {
                var angle = (i == 0 ? spell.Rotation : _aoes[i - 1].Rotation) + angles[i];
                _aoes.Add(new(cone, spell.LocXZ, angle, Module.CastFinishAt(spell, 9.3f + 2f * i)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.WordOfTheWood1:
                case (uint)AID.WordOfTheWood2:
                case (uint)AID.WordOfTheWood3:
                case (uint)AID.WordOfTheWood4:
                case (uint)AID.WordOfTheWood5:
                case (uint)AID.WordOfTheWood6:
                case (uint)AID.WordOfTheWood7:
                case (uint)AID.WordOfTheWood8:
                case (uint)AID.WordOfTheWood9:
                case (uint)AID.WordOfTheWood10:
                case (uint)AID.WordOfTheWood11:
                case (uint)AID.WordOfTheWood12:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class NechucihoStates : StateMachineBuilder
{
    public NechucihoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Level5DeathSentence>()
            .ActivateOnEnter<SentinelRoar>()
            .ActivateOnEnter<WordOfTheWood>()
            .ActivateOnEnter<WhispersOfTheWood>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin, Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13362)]
public class Nechuciho(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
