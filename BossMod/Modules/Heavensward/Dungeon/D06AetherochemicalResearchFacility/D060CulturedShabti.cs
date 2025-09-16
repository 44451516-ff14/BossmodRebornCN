namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D060CulturedShabti;

public enum OID : uint
{
    Boss = 0xF58 // R2.2
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    Spellsword = 1259, // Boss->self, 3.0s cast, range 6+R 120-degree cone
    DeathsDoor = 1260 // Boss->self, 2.0s cast, range 20+R width 2 rect
}

class Spellsword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spellsword, new AOEShapeCone(8.2f, 60f.Degrees()));
class DeathsDoor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathsDoor, new AOEShapeRect(22.2f, 1f));

class D060CulturedShabtiStates : StateMachineBuilder
{
    public D060CulturedShabtiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Spellsword>()
            .ActivateOnEnter<DeathsDoor>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.Boss);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3838, SortOrder = 9)]
public class D060CulturedShabti(WorldState ws, Actor primary) : BossModule(ws, primary, IsArena1(primary) ? arena1.Center : ShabtiConjurer2Bounds.Arena.Center, IsArena1(primary) ? arena1 : ShabtiConjurer2Bounds.Arena)
{
    private static bool IsArena1(Actor primary) => primary.PosRot.Z > 200f;
    private static readonly WPos[] vertices1 = [new(226.49f, 196.43f), new(238.31f, 203.26f), new(238.76f, 203.54f), new(239.63f, 204.05f), new(239.86f, 218.33f),
    new(239.83f, 219.46f), new(239.52f, 219.88f), new(235.07f, 222.4f), new(234.6f, 222.66f), new(234.1f, 222.82f),
    new(233.63f, 223.09f), new(233.23f, 223.41f), new(232.96f, 223.83f), new(233.29f, 224.28f), new(238.73f, 227.52f),
    new(239.2f, 227.82f), new(239.63f, 228.09f), new(239.89f, 228.65f), new(239.9f, 231.48f), new(239.91f, 232),
    new(240.29f, 232.35f), new(240.76f, 232.17f), new(243.52f, 230.58f), new(244.05f, 230.57f), new(245.91f, 231.66f),
    new(246.35f, 231.92f), new(246.82f, 232.15f), new(246.83f, 235.98f), new(247.08f, 236.47f), new(246.49f, 237.38f),
    new(243.93f, 240.88f), new(243.6f, 241.31f), new(243.27f, 241.71f), new(242.85f, 242.04f), new(241.03f, 243.09f),
    new(240.58f, 243.32f), new(240.07f, 243.37f), new(238.69f, 242.57f), new(238.28f, 242.29f), new(237.34f, 241.75f),
    new(237.44f, 240.16f), new(237.45f, 239.64f), new(237.43f, 239.11f), new(237.38f, 238.59f), new(237.3f, 238.08f),
    new(237.06f, 237.62f), new(236.55f, 237.66f), new(233.45f, 239.44f), new(232.96f, 239.26f), new(228.29f, 236.57f),
    new(227.86f, 236.25f), new(227.44f, 235.96f), new(226.96f, 235.75f), new(226.47f, 235.56f), new(226.04f, 235.87f),
    new(226.02f, 236.68f), new(226, 243.71f), new(216.15f, 249.4f), new(215.71f, 249.67f), new(213.5f, 250.94f),
    new(213.04f, 251.16f), new(212.6f, 251.41f), new(212.12f, 251.21f), new(199.2f, 243.76f), new(199.1f, 230.55f),
    new(199.13f, 228.17f), new(204.9f, 224.8f), new(205.36f, 224.55f), new(205.8f, 224.3f), new(206.18f, 223.94f),
    new(212.16f, 220.48f), new(212.63f, 220.26f), new(212.9f, 211.95f), new(212.95f, 204.34f), new(213.29f, 203.95f),
    new(226.02f, 196.6f)];
    private static readonly WPos[] vertices1h1 = [new(211.37f, 231.27f), new(211.85f, 231.42f), new(212.3f, 231.66f), new(213.35f, 231.72f), new(213.87f, 231.87f),
    new(214.34f, 232.06f), new(214.8f, 232.26f), new(215.3f, 232.3f), new(215.83f, 232.36f), new(216.14f, 232.76f),
    new(216.4f, 233.24f), new(217.24f, 234.58f), new(217.23f, 235.12f), new(217.07f, 235.61f), new(216.87f, 236.08f),
    new(216.84f, 236.61f), new(216.75f, 237.11f), new(216.37f, 238.06f), new(216.15f, 238.51f), new(216.29f, 239.01f),
    new(216.02f, 239.47f), new(215.58f, 239.77f), new(215.11f, 239.97f), new(214.24f, 240.52f), new(213.76f, 240.74f),
    new(213.29f, 240.56f), new(212.83f, 240.34f), new(211.8f, 240.27f), new(211.3f, 240.13f), new(210.32f, 239.74f),
    new(209.82f, 239.72f), new(209.32f, 239.64f), new(208.89f, 239.36f), new(208.7f, 238.84f), new(208.47f, 238.39f),
    new(207.93f, 237.54f), new(207.84f, 237.03f), new(208.01f, 236.54f), new(208.23f, 236.05f), new(208.28f, 235.53f),
    new(208.31f, 235.01f), new(208.69f, 234.03f), new(208.96f, 233.59f), new(208.81f, 233.09f), new(209.05f, 232.63f),
    new(210.82f, 231.51f), new(211.28f, 231.26f)];
    private static readonly WPos[] vertices1h2 = [new(226.96f, 206.41f), new(227.45f, 206.57f), new(229.4f, 207.38f), new(229.68f, 207.82f), new(229.75f, 208.33f),
    new(229.96f, 208.82f), new(230.23f, 209.27f), new(230.48f, 209.72f), new(230.72f, 210.73f), new(230.77f, 211.23f),
    new(231.18f, 211.54f), new(231.23f, 212.07f), new(231.21f, 212.58f), new(230.99f, 213.04f), new(230.62f, 213.42f),
    new(230.42f, 213.9f), new(230.21f, 214.36f), new(229.73f, 214.56f), new(229.23f, 214.65f), new(228.75f, 214.87f),
    new(228.29f, 215.16f), new(227.79f, 215.34f), new(226.79f, 215.58f), new(226.33f, 215.83f), new(225.87f, 216.12f),
    new(225.37f, 215.94f), new(223.45f, 215.15f), new(223.06f, 214.8f), new(222.72f, 214.4f), new(222.44f, 213.96f),
    new(222.42f, 213.43f), new(222.35f, 212.94f), new(222.13f, 211.93f), new(222.05f, 211.41f), new(221.68f, 211.05f),
    new(221.57f, 210.53f), new(221.6f, 210.01f), new(221.83f, 209.54f), new(222.16f, 209.14f), new(222.55f, 208.2f),
    new(223.03f, 207.97f), new(223.55f, 207.95f), new(224.44f, 207.4f), new(224.89f, 207.17f), new(225.39f, 207.07f),
    new(225.91f, 206.95f), new(226.4f, 206.86f), new(226.71f, 206.46f)];
    private static readonly ArenaBoundsCustom arena1 = new([new PolygonCustom(vertices1)], [new PolygonCustom(vertices1h1), new PolygonCustom(vertices1h2)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}
