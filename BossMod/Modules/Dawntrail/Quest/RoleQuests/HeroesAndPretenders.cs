namespace BossMod.Dawntrail.Quest.RoleQuests.HeroesAndPretenders;

public enum OID : uint
{
    Boss = 0x428A, // R1.500, x?
    CultivatedOchu1 = 0x428E, // R1.92
    CultivatedOchu2 = 0x448B, // R1.92
    PerchOfTheApex = 0x428C, // R0.7
    CultivatedMorbolSeedling = 0x4207, // R0.9
    CultivatedMossFungus = 0x4208, // R1.32
    Helper = 0x233C, // R0.5
}

public enum AID : uint
{
    AutoAttack1 = 39521, // Boss->Tepeke, no cast, single-target
    AutoAttack2 = 39539, // CultivatedMorbolSeedling/CultivatedMossFungus->Tepeke, no cast, single-target
    AutoAttack3 = 39523, // CultivatedOchu1/CultivatedOchu2->Tepeke, no cast, single-target
    Teleport = 37456, // Boss->location, no cast, single-target

    PerchOfTheApex = 37457, // Boss->self, 3.0s cast, single-target
    FledglingFury = 37458, // Helper->location, 4.0s cast, range 4 circle
    UnboundArrow = 37459, // Boss->Tepeke, 5.0s cast, range 5 circle, tankbuster
    AquaVitae1 = 37465, // Boss->self, 3.0s cast, single-target
    AquaVitae2 = 37705, // Boss->self, no cast, single-target
    ArtOfNature = 37461, // Boss->self, 3.0s cast, single-target

    GoldDust = 37464, // CultivatedOchu1/CultivatedOchu2->Tepeke, 8.0s cast, range 8 circle, stack
    AcidRainVisual = 37462, // CultivatedOchu1/CultivatedOchu2->self, 8.0s cast, single-target
    AcidRain = 37463, // Helper->player/Tepeke, 8.0s cast, range 8 circle, spread
    PromisedFall = 37466, // Helper->location, 6.0s cast, range 35 circle, damage fall off aoe
    ForeseenFlurryFirst = 37467, // Helper->self, 7.0s cast, range 4 circle
    ForeseenFlurryRest = 37468, // Helper->self, no cast, range 4 circle
    PerchOfTheApexVisual1 = 37471, // PerchOfTheApex->self, no cast, single-target
    PerchOfTheApexVisual2 = 37474, // PerchOfTheApex->self, no cast, single-target
    ApexJudgment = 37472, // Helper->self, no cast, range 35 circle, raidwide
    Visual = 37473, // Boss->self, no cast, single-target
}

class FledglingFury(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FledglingFury), 4);
class PromisedFall(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PromisedFall), 13);
class GoldDust(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.GoldDust), 8, 2, 2);
class AcidRain(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.AcidRain), 8);
class UnboundArrow(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.UnboundArrow), new AOEShapeCircle(5), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class ForeseenFlurry(BossModule module) : Components.Exaflare(module, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ForeseenFlurryFirst)
            Lines.Add(new() { Next = caster.Position, Advance = 5 * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.1f, ExplosionsLeft = 8, MaxShownExplosions = 3 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ForeseenFlurryFirst or AID.ForeseenFlurryRest)
        {
            var index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class HeroesAndPretendersStates : StateMachineBuilder
{
    public HeroesAndPretendersStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PromisedFall>()
            .ActivateOnEnter<GoldDust>()
            .ActivateOnEnter<AcidRain>()
            .ActivateOnEnter<UnboundArrow>()
            .ActivateOnEnter<FledglingFury>()
            .ActivateOnEnter<ForeseenFlurry>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70383, NameID = 13176, SortOrder = 1)]
public class HeroesAndPretenders(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(676, 41), 14.5f, 20)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.PerchOfTheApex).Concat([PrimaryActor]).Concat(Enemies(OID.CultivatedMossFungus)).Concat(Enemies(OID.CultivatedMorbolSeedling))
        .Concat(Enemies(OID.CultivatedOchu1)).Concat(Enemies(OID.CultivatedOchu2)));
    }
}
