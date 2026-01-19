namespace BossMod.Dawntrail.Savage.M09SVampFatale;

sealed class KillerVoice(BossModule module) : Components.RaidwideCast(module, (uint)AID.KillerVoice);
sealed class Hardcore(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true, tankbuster: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Hardcore or (uint)AID.HardcoreBig && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
        {
            var shape = new AOEShapeCircle(spell.Action.ID == (uint)AID.Hardcore ? 6f : 15f);
            CurrentBaits.Add(new(caster, target, shape, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Hardcore or (uint)AID.HardcoreBig)
        {
            var count = CurrentBaits.Count;
            var id = caster.InstanceID;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Source.InstanceID == id)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
sealed class VampStomp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VampStomp, 10f);

sealed class BrutalRain(BossModule module) : Components.StackWithIcon(module, (uint)IconID.ShareMulti, (uint)AID.BrutalRain, 6f, 5.1d, 6, 8, 5)
{
    private int _satisfied = 0;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == StackIcon)
        {
            AddStack(actor, WorldState.FutureTime(ActivationDelay));
            var satisfied = Module.PrimaryActor.FindStatus((uint)SID.Satisfied);
            _satisfied = (satisfied.HasValue ? satisfied.Value.Extra : 0) / 4;
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == StackAction)
        {
            if (CastCounter >= 3 + _satisfied)
            {
                CastCounter = 0;
                Stacks.Clear();
            }
        }
    }
}

sealed class SadisticScreech(BossModule module) : Components.RaidwideCast(module, (uint)AID.SadisticScreech);
sealed class Coffinmaker(BossModule module) : Components.Adds(module, (uint)OID.Coffinmaker);
sealed class Coffinfiller(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CoffinfillerLong, (uint)AID.CoffinfillerMed, (uint)AID.CoffinfillerShort], new AOEShapeRect(32f, 2.5f), 2, riskyWithSecondsLeft: 2.5d);
sealed class DeadWake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadWake, new AOEShapeRect(10f, 10f), riskyWithSecondsLeft: 2.5d);
//sealed class HalfMoon(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HalfMoonLL, (uint)AID.HalfMoonRR, (uint)AID.HalfMoonBigLL, (uint)AID.HalfMoonBigRR, (uint)AID.HalfMoonLR, (uint)AID.HalfMoonRL, (uint)AID.HalfMoonBigLR, (uint)AID.HalfMoonBigRL], new AOEShapeCone(64f, 90.Degrees()), 1, 1);
// change risky time only during Arena1 and only for melee classes for AI melee uptime; if ranges are too far out and risky is short they can get hit
sealed class HalfMoon(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HalfMoonLL, (uint)AID.HalfMoonRR, (uint)AID.HalfMoonBigLL, (uint)AID.HalfMoonBigRR, (uint)AID.HalfMoonLR, (uint)AID.HalfMoonRL, (uint)AID.HalfMoonBigLR, (uint)AID.HalfMoonBigRL], new AOEShapeCone(64f, 90.Degrees()), 1, 1)
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(Casters);
        var max = count > MaxCasts ? MaxCasts : count;

        if (_arena.Active)
        {
            var isMelee = actor.Role is Role.Tank or Role.Melee;

            if (isMelee)
            {
                var time = WorldState.CurrentTime;

                for (var i = 0; i < max; ++i)
                {
                    ref var aoe = ref aoes[i];
                    aoe.Risky = aoe.Activation.AddSeconds(-2.5d) <= time;
                }
            }
        }
        return aoes[..max];
    }
}
sealed class CrowdKill(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.CrowdKillCast, (uint)AID.CrowdKill, 5.9f);
sealed class FinaleFatale(BossModule module) : Components.RaidwideCastsDelay(module, [(uint)AID.FinaleFataleCast, (uint)AID.FinaleFataleBigCast], [(uint)AID.FinaleFatale, (uint)AID.FinaleFataleBig], 6.2f);
sealed class PulpingPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PulpingPulse, 5f);
sealed class InsatiableThirst(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.InsatiableThirstCast, (uint)AID.InsatiableThirst, 3.2f);
sealed class FinaleFataleEnrage(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.FinaleFataleEnrageCast, (uint)AID.FinaleFataleEnrage, 10f);

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(M09SVampFataleStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.VampFatale,
Contributors = "gynorhino",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Savage,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1069u,
NameID = 14300u,
SortOrder = 1,
PlanLevel = 100)]
public sealed class M09SVampFatale(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, ArenaBounds)
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsSquare ArenaBounds = new(20f);
}
