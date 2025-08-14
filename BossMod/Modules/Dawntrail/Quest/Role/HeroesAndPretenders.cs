namespace BossMod.Dawntrail.Quest.Role.HeroesAndPretenders;

public enum OID : uint
{
    Boss = 0x428A, // R1.500, x?
    CultivatedOchu1 = 0x428E, // R1.92
    CultivatedOchu2 = 0x448B, // R1.92
    PerchOfTheApex = 0x428C, // R0.7
    CultivatedMorbolSeedling = 0x4207, // R0.9
    CultivatedMossFungus = 0x4208, // R1.32
    Helper = 0x233C
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
    Visual = 37473 // Boss->self, no cast, single-target
}

sealed class FledglingFury(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FledglingFury, 4f);
sealed class PromisedFall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PromisedFall, 13f);
sealed class GoldDust(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.GoldDust, 8f, 2, 2);
sealed class AcidRain(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AcidRain, 8f);
sealed class UnboundArrow(BossModule module) : Components.BaitAwayCast(module, (uint)AID.UnboundArrow, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class ForeseenFlurry(BossModule module) : Components.Exaflare(module, 4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ForeseenFlurryFirst)
            Lines.Add(new(caster.Position, 5f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 1.1d, 8, 3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ForeseenFlurryFirst or (uint)AID.ForeseenFlurryRest)
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
                    break;
                }
            }
        }
    }
}

sealed class HeroesAndPretendersStates : StateMachineBuilder
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70383, NameID = 13176)]
public sealed class HeroesAndPretenders(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(676f, 41f), 14.5f, 20)]);

    private static readonly uint[] all = [(uint)OID.Boss, (uint)OID.PerchOfTheApex, (uint)OID.CultivatedMossFungus, (uint)OID.CultivatedOchu1, (uint)OID.CultivatedOchu2,
    (uint)OID.CultivatedMorbolSeedling];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(all));
    }
}
