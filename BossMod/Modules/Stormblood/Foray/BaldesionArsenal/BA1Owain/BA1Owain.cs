namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Owain;

sealed class Thricecull(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Thricecull);
sealed class AcallamNaSenorach(BossModule module) : Components.RaidwideCast(module, (uint)AID.AcallamNaSenorach);
sealed class LegendaryImbas(BossModule module) : Components.RaidwideCast(module, (uint)AID.LegendaryImbas); // applies dorito stacks, seems to get skipped if less than 4 people alive?
sealed class Pitfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pitfall, 20f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.BaldesionArsenal, GroupID = 639, NameID = 7970, PlanLevel = 70, SortOrder = 2)]
public sealed class BA1Owain(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(128.98f, 748f), 29.5f, 64)], [new Rectangle(new(129f, 718f), 20f, 0.8f), new Rectangle(new(129f, 778f), 20f, 0.825f),
    new Polygon(new(123.5f, 778f), 1.5f, 8), new Polygon(new(134.5f, 778f), 1.5f, 8), new Polygon(new(123.5f, 718f), 1.5f, 8), new Polygon(new(134.5f, 718f), 1.5f, 8)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.IvoryPalm));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.IvoryPalm => 1,
                _ => 0
            };
        }
    }
}
