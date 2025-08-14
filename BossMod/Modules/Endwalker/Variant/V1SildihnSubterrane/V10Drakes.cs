namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V10Drakes;

public enum OID : uint
{
    ForgottenDrakefather = 0x3CD8, // R7.2
    ForgottenDrakemother = 0x3CD9, // R5.4
    ForgottenDrakebrother = 0x3CDA, // R3.6
    ForgottenDrakesister = 0x3CDB, // R2.25
    ForgottenDrakeling = 0x3CDC // R1.35
}

public enum AID : uint
{
    AutoAttack = 6497, // ForgottenDrakeling/ForgottenDrakebrother/ForgottenDrakemother/ForgottenDrakefather/ForgottenDrakesister->player, no cast, single-target

    BurningCyclone = 30678 // ForgottenDrakemother/ForgottenDrakefather/ForgottenDrakeling/ForgottenDrakesister/ForgottenDrakebrother->self, 3.0s cast, range 8 120-degree cone
}

sealed class BurningCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningCyclone, new AOEShapeCone(8f, 60f.Degrees()))
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Order: Fater -> mother -> brother -> sister -> drakeling");
    }
}

sealed class V10DrakesStates : StateMachineBuilder
{
    public V10DrakesStates(V10Drakes module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurningCyclone>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && (module.DrakeMother?.IsDeadOrDestroyed ?? true) && (module.DrakeBrother?.IsDeadOrDestroyed ?? true) && (module.DrakeSister?.IsDeadOrDestroyed ?? true) && (module.Drakeling?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.ForgottenDrakefather, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11463, SortOrder = 1, Category = BossModuleInfo.Category.VariantCriterion, Expansion = BossModuleInfo.Expansion.Endwalker)]
public sealed class V10Drakes(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(
    [new(275.49f, 38.15f), new(276.14f, 38.32f), new(281.62f, 41.08f), new(282.62f, 42.12f), new(283.76f, 42.89f),
    new(286.03f, 46.16f), new(286.27f, 46.77f), new(286.76f, 48.75f), new(286.68f, 49.39f), new(286.8f, 50.05f),
    new(287.04f, 50.73f), new(287.72f, 51.78f), new(288.21f, 51.69f), new(288.46f, 52.24f), new(288.92f, 52.68f),
    new(289.42f, 54.54f), new(289.52f, 55.18f), new(289.4f, 55.72f), new(289.23f, 56.23f), new(289.01f, 56.71f),
    new(288.65f, 57.31f), new(287.26f, 58.86f), new(287.47f, 59.41f), new(288.59f, 59.78f), new(288.71f, 60.39f),
    new(288.03f, 63.55f), new(287.8f, 64.17f), new(286.5f, 66.53f), new(286.3f, 67.07f), new(286.17f, 67.73f),
    new(285.78f, 68.33f), new(285.2f, 68.76f), new(282.79f, 68.31f), new(282.3f, 67.77f), new(281.85f, 66.58f),
    new(281.59f, 65.97f), new(281.02f, 65.63f), new(280.41f, 65.72f), new(279.93f, 65.92f), new(276.85f, 66.54f),
    new(275.48f, 67.03f), new(275.21f, 67.51f), new(274.87f, 67.89f), new(273.67f, 68.41f), new(273.45f, 69.07f),
    new(273.13f, 69.57f), new(272.62f, 69.99f), new(271.39f, 70.35f), new(271.04f, 71f), new(270.9f, 71.63f),
    new(270.58f, 72.16f), new(270.08f, 72.03f), new(268.71f, 71.89f), new(262.63f, 69.96f), new(262.21f, 69.45f),
    new(261.74f, 68.37f), new(261.71f, 67.79f), new(260.32f, 67.62f), new(259.82f, 67.71f), new(258.62f, 66.12f),
    new(258.1f, 65.9f), new(257.46f, 65.88f), new(255.97f, 64.52f), new(255.81f, 64.03f), new(255.68f, 63.3f),
    new(255.17f, 57.85f), new(254.66f, 57.36f), new(257.03f, 56.11f), new(257.08f, 55.45f), new(256.25f, 53.56f),
    new(256.51f, 52.92f), new(257.12f, 52.56f), new(259.78f, 51.47f), new(260.96f, 50.69f), new(260.58f, 50.24f),
    new(258.38f, 48.54f), new(258.82f, 48.17f), new(258.93f, 46.96f), new(259.09f, 46.3f), new(259.94f, 45.31f),
    new(262.49f, 44.27f), new(263.19f, 44.26f), new(263.87f, 44.35f), new(264.5f, 44.24f), new(265f, 43.82f),
    new(265.23f, 43.26f), new(265.17f, 42.62f), new(265.41f, 42.01f), new(266.74f, 40.46f), new(268.39f, 39.27f),
    new(271.07f, 38.83f), new(271.7f, 38.64f), new(272.19f, 38.69f), new(273.58f, 38.99f), new(274.15f, 38.82f),
    new(274.64f, 38.35f), new(275.26f, 38.14f)])]);
    public Actor? DrakeMother;
    public Actor? DrakeBrother;
    public Actor? DrakeSister;
    public Actor? Drakeling;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (DrakeMother == null)
        {
            var b = Enemies((uint)OID.ForgottenDrakemother);
            DrakeMother = b.Count != 0 ? b[0] : null;
        }
        if (DrakeBrother == null)
        {
            var b = Enemies((uint)OID.ForgottenDrakebrother);
            DrakeBrother = b.Count != 0 ? b[0] : null;
        }
        if (DrakeSister == null)
        {
            var b = Enemies((uint)OID.ForgottenDrakesister);
            DrakeSister = b.Count != 0 ? b[0] : null;
        }
        if (Drakeling == null)
        {
            var b = Enemies((uint)OID.ForgottenDrakeling);
            Drakeling = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(DrakeMother);
        Arena.Actor(DrakeBrother);
        Arena.Actor(DrakeSister);
        Arena.Actor(Drakeling);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        var potHints = CollectionsMarshal.AsSpan(hints.PotentialTargets);

        for (var i = 0; i < count; ++i)
        {
            var h = potHints[i];
            var e = h.Actor;
            ref var p = ref h.Priority;

            if (e == PrimaryActor)
            {
                p = 1;
            }
            else if (e == DrakeMother)
            {
                p = PrimaryActor.IsDeadOrDestroyed ? 1 : AIHints.Enemy.PriorityUndesirable;
            }
            else if (e == DrakeBrother)
            {
                p = DrakeMother?.IsDeadOrDestroyed ?? true ? 1 : AIHints.Enemy.PriorityUndesirable;
            }
            else if (e == DrakeSister)
            {
                p = DrakeBrother?.IsDeadOrDestroyed ?? true ? 1 : AIHints.Enemy.PriorityUndesirable;
            }
            else if (e == Drakeling)
            {
                p = DrakeSister?.IsDeadOrDestroyed ?? true ? 1 : AIHints.Enemy.PriorityUndesirable;
            }
        }
    }
}
