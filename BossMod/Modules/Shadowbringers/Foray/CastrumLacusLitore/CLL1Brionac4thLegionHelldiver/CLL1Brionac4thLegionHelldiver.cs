namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver;

sealed class ElectricAnvil(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ElectricAnvil, arenaProjectionLayer: 1);

sealed class MagitekMissiles(BossModule module) : Components.SingleTargetCast(module, (uint)AID.MagitekMissiles, arenaProjectionLayer: 0);

sealed class MRVMissile(BossModule module) : Components.RaidwideCast(module, (uint)AID.MRVMissile, arenaProjectionLayer: 0);
sealed class LightningShower(BossModule module) : Components.RaidwideCast(module, (uint)AID.LightningShower, arenaProjectionLayer: 1);

sealed class FalseThunder(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FalseThunder1, (uint)AID.FalseThunder2], new AOEShapeCone(47f, 65f.Degrees()), arenaProjectionLayer: 1);

sealed class Voltstream(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Voltstream, new AOEShapeRect(40f, 5f), 3, arenaProjectionLayer: 1);

sealed class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile, 6f, arenaProjectionLayer: 0);

sealed class CommandSuppressiveFormation(BossModule module) : Components.ChargeAOEs(module, (uint)AID.CommandSuppressiveFormation, 3f, arenaProjectionLayer: 0);

sealed class BossHealths(CLL1Brionac4thLegionHelldiver module) : BossComponent(module)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        hints.Add($"Top: {Module.PrimaryActor.HPRatio * 100f:f1}%, Bottom: {module.BossHellDiver?.HPRatio * 100f:f1}%");
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CastrumLacusLitore, GroupID = 735u, NameID = 9436u)]
public sealed class CLL1Brionac4thLegionHelldiver : BossModule
{
    public CLL1Brionac4thLegionHelldiver(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private CLL1Brionac4thLegionHelldiver(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var bottom = new Rectangle(new(80f, -179.41f), 29.58f, 24.59f);
        var top = new Rectangle(new(80f, -222f), 29.5f, 14.5f);
        var combinedCenter = new WPos(80f, -195.66f);
        var polybottom = new RelSimplifiedComplexPolygon(bottom.Contour(combinedCenter));
        var polytop = new RelSimplifiedComplexPolygon(top.Contour(combinedCenter));
        var arena = new ArenaBoundsCustom([bottom, top], WorldProjectionLayers: [new(polybottom, 230f, borderY: 230f), new(polytop, 249.5f, borderY: 249.5f)]);
        return (arena.Center, arena);
    }

    public Actor? BossHellDiver;
    public Actor? TunnelArmor;

    protected override void UpdateModule()
    {
        BossHellDiver ??= GetActor((uint)OID.FourthLegionHelldiver1);
        TunnelArmor ??= GetActor((uint)OID.TunnelArmor);
    }

    protected override bool CheckPull() => base.CheckPull() || (BossHellDiver?.InCombat ?? false);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (ActorMatchesArenaProjectionLayer(pc, 1, true))
        {
            Arena.Actor(PrimaryActor);
        }
        else
        {
            Arena.Actors(Enemies((uint)OID.FourthLegionHelldiver3));
            Arena.Actor(BossHellDiver);
        }
        var skyarmors = Enemies((uint)OID.FourthLegionSkyArmor);
        var count = skyarmors.Count;
        for (var i = 0; i < count; ++i)
        {
            var skyarmor = skyarmors[i];
            if (ActorsMatchArenaProjectionLayer(pc, skyarmor))
            {
                Arena.Actor(skyarmor);
            }
        }
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        var potHints = CollectionsMarshal.AsSpan(hints.PotentialTargets);
        var isTop = ActorMatchesArenaProjectionLayer(actor, 1, true);
        for (var i = 0; i < count; ++i)
        {
            var h = potHints[i];
            var e = h.Actor;
            var enemyPrio = h.Priority;
            var oid = e.OID;
            if (isTop)
            {
                if (oid == (uint)OID.MagitekCore)
                {
                    enemyPrio = 1;
                }
                // if top boss got less than 20% hp, but hp difference to bottom boss is > 10%, forbid attacking
                else if (e == PrimaryActor && e.HPRatio is var ratio && ratio <= 0.2f && ratio - BossHellDiver?.HPRatio < -0.1f)
                {
                    enemyPrio = AIHints.Enemy.PriorityForbidden;
                }
                else if (oid == (uint)OID.FourthLegionSkyArmor && ActorMatchesArenaProjectionLayer(actor, 1, true))
                {
                    enemyPrio = 0;
                }
                else if (oid != (uint)OID.Boss)
                {
                    enemyPrio = AIHints.Enemy.PriorityInvincible;
                }
            }
            else
            {
                if (oid == (uint)OID.FourthLegionHelldiver3)
                {
                    enemyPrio = 1;
                }
                // if bottom boss got less than 20% hp, but hp difference to upper boss is > 10%, forbid attacking
                // unless tunnel armor is almost dead, then risk the enrage sequence
                else if (e == BossHellDiver && e.HPRatio is var ratio && ratio <= 0.2f && ratio - PrimaryActor.HPRatio < -0.1f && TunnelArmor?.HPRatio > 0.1f)
                {
                    enemyPrio = AIHints.Enemy.PriorityForbidden;
                }
                else if (oid == (uint)OID.FourthLegionSkyArmor && ActorMatchesArenaProjectionLayer(actor, 0, true))
                {
                    enemyPrio = 0;
                }
                else if (oid != (uint)OID.FourthLegionHelldiver1)
                {
                    enemyPrio = AIHints.Enemy.PriorityInvincible;
                }
            }
            h.Priority = enemyPrio;
        }
    }
}
