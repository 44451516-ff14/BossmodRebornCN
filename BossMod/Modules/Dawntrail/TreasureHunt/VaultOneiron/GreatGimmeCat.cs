namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.GreatGimmeCat;

public enum OID : uint
{
    GreatGimmeCat = 0x48AC, // R3.3
    GoldyCat = 0x48B7, // R1.87
    CatCollar = 0x48AD, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // GreatGimmeCat->player, no cast, single-target

    PreeningPrismVisual = 43686, // GreatGimmeCat->self, 3.0s cast, single-target
    PreeningPrism1 = 43687, // Helper->self, 6.0s cast, range 20 90-degree triangle
    PreeningPrism2 = 43688, // Helper->self, 8.0s cast, range 20 90-degree triangle

    GlitterboxAOEVisual = 43691, // GreatGimmeCat->self, 3.0s cast, single-target
    GlitterboxAOE = 43692, // Helper->location, 3.0s cast, range 5 circle
    GlitterboxSpreadVisual = 43695, // GreatGimmeCat->self, 5.0s cast, single-target
    GlitterboxSpread = 43696, // Helper->player, 5.0s cast, range 5 circle

    AllTogetherMeow = 43699, // GreatGimmeCat->self, 3.0s cast, single-target
    PreciousMeowRing = 43689, // GreatGimmeCat->self, 3.0s cast, single-target
    BaskingBeam = 43690, // CatCollar->self, 3.0s cast, range 42 width 5 rect

    SparklingIntensifiesVisual = 43693, // GreatGimmeCat->self, 4.5+0,5s cast, single-target
    SparklingIntensifies = 43694, // Helper->self, 5.0s cast, range 40 circle, raidwide
    TormentingTwinkleVisual = 43697, // GreatGimmeCat->self, 5.0s cast, single-target
    TormentingTwinkle = 43698, // Helper->player, 5.0s cast, range 5 circle, tankbuster
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class PreeningPrism(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PreeningPrism1, (uint)AID.PreeningPrism2], new AOEShapeTriCone(20f, 45f.Degrees()), 4, 8);
sealed class GlitterboxAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GlitterboxAOE, 5f);
sealed class GlitterboxSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.GlitterboxSpread, 5f);
sealed class SparklingIntensifies(BossModule module) : Components.RaidwideCast(module, (uint)AID.SparklingIntensifies);
sealed class TormentingTwinkle(BossModule module) : Components.BaitAwayCast(module, (uint)AID.TormentingTwinkle, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class BaskingBeam(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private readonly AOEShapeRect rect = new(42f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 4 ? 4 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.CatCollar)
        {
            _aoes.Add(new(rect, (actor.Position + new WDir(default, -21f)).Quantized(), actor.Rotation, WorldState.FutureTime(7.7d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BaskingBeam)
        {
            if ((++NumCasts & 3) == 0 && _aoes.Count >= 4)
            {
                _aoes.RemoveRange(0, 4);
            }
        }
    }
}

sealed class GreatGimmeCatStates : StateMachineBuilder
{
    public GreatGimmeCatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PreeningPrism>()
            .ActivateOnEnter<GlitterboxAOE>()
            .ActivateOnEnter<GlitterboxSpread>()
            .ActivateOnEnter<SparklingIntensifies>()
            .ActivateOnEnter<TormentingTwinkle>()
            .ActivateOnEnter<BaskingBeam>()
            .Raw.Update = () => AllDeadOrDestroyed(GreatGimmeCat.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.GreatGimmeCat, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 14003u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 2)]
public sealed class GreatGimmeCat(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    public static readonly uint[] All = [(uint)OID.GreatGimmeCat, (uint)OID.GoldyCat];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GoldyCat), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GoldyCat => 1,
                _ => 0
            };
        }
    }
}
