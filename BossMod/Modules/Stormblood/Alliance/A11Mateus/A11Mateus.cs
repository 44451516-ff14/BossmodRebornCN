namespace BossMod.Stormblood.Alliance.A11Mateus;

class HypothermalCombustion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HypothermalCombustion, 9.04f);
class DarkBlizzardIII(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkBlizzardIII, 5f);
class Chill(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Chill, new AOEShapeCone(41f, 10f.Degrees()));
class BlizzardIV(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlizzardIV, 15f);

class FlashFreeze(BossModule module) : Components.Cleave(module, (uint)AID.FlashFreeze, new AOEShapeCone(16.5f, 60f.Degrees()), activeWhileCasting: false)
{
    private bool active = true;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (active)
            base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Totema)
            active = true;
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.Totema)
            active = false;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 281, NameID = 6929)]
public class A11Mateus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-320f, 240f), new ArenaBoundsSquare(29.5f))
{
    private static readonly uint[] trash = [(uint)OID.IceAzer, (uint)OID.IceSlave, (uint)OID.FlumeToad, (uint)OID.AquaSphere, (uint)OID.AzureGuard, (uint)OID.BlizzardIII];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 0,
                _ => 1
            };
        }
    }
}
