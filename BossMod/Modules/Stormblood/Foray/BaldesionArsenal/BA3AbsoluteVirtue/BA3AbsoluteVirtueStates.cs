namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA3AbsoluteVirtue;

sealed class BA3AbsoluteVirtueStates : StateMachineBuilder
{
    public BA3AbsoluteVirtueStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<MedusaJavelin>()
            .ActivateOnEnter<AuroralWind>()
            .ActivateOnEnter<ExplosiveImpulse>()
            .ActivateOnEnter<AernsWynavExplosion>()
            .ActivateOnEnter<BrightDarkAurora>()
            .ActivateOnEnter<AstralUmbralRays>()
            .ActivateOnEnter<BrightDarkAuroraExplosion>()
            .ActivateOnEnter<DarkAuroraTether>()
            .ActivateOnEnter<BrightAuroraTether>();
    }
    // some timings have noticeable variations of over 1s
    private void SinglePhase(uint id)
    {
        Meteor(id, 11.1f);
        Eidos(id + 0x10000u, 4.2f);
        HostileAspect1(id + 0x20000u, 4.1f);
        MedusaJavelin(id + 0x30000u, 2.7f);
        Eidos(id + 0x40000u, 5.2f);
        ImpactStreamBoss(id + 0x50000u, 4f);
        AuroralWind(id + 0x60000u, 6.4f);
        Eidos(id + 0x70000u, 12f);
        HostileAspect1(id + 0x80000u, 3.8f);
        Meteor(id + 0x90000u, 14f);
        DarkBrightAuroraTowers(id + 0xA0000u, 10f);
        MedusaJavelin(id + 0xB0000u, 5.8f);
        AuroralWind(id + 0xC0000u, 3.5f);
        Meteor(id + 0xD0000u, 4.1f);
        Meteor(id + 0xE0000u, 5.1f);
        RelativeVirtues(id + 0xF0000u, 10f);
        MedusaJavelin(id + 0x100000u, 3.2f);
        AuroralWind(id + 0x110000u, 7f);
        Meteor(id + 0x120000u, 4.2f);
        CallWyvern(id + 0x130000u, 6.5f);
        DarkBrightAuroraTowers(id + 0x140000u, 6.4f);
        MedusaJavelin(id + 0x150000u, 5f);
        AuroralWind(id + 0x160000u, 3.1f);
        Meteor(id + 0x170000u, 13f);
        Eidos(id + 0x180000u, 4.2f);
        HostileAspect2(id + 0x190000u, 5f);
        MedusaJavelin(id + 0x1A0000u, 8.2f);
        Meteor(id + 0x1B0000u, 3.2f);
        CallWyvern(id + 0x1C0000u, 12.3f);
        DarkBrightAuroraTowers(id + 0x1D0000u, 6.2f);
        MedusaJavelin(id + 0x1E0000u, 5f);
        AuroralWind(id + 0x1F0000u, 3.2f);
        Meteor(id + 0x200000u, 13.2f);
        Eidos(id + 0x210000u, 4.2f);
        HostileAspect2(id + 0x220000u, 3.5f);
        MedusaJavelin(id + 0x230000u, 8f);
        Meteor(id + 0x240000u, 3f);
        MeteorEnrage(id + 0x250000u);
    }

    private void Meteor(uint id, float delay)
    {
        Cast(id, (uint)AID.Meteor, delay, 4f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MeteorEnrage(uint id)
    {
        Cast(id, (uint)AID.MeteorEnrage, 3.1f, 10f, "Enrage")
            .ActivateOnEnter<MeteorEnrageCounter>();
        ComponentCondition<MeteorEnrageCounter>(id + 0x10u, 3f, comp => comp.NumCasts == 1, "Enrage repeat 1");
        ComponentCondition<MeteorEnrageCounter>(id + 0x20u, 5f, comp => comp.NumCasts == 2, "Enrage repeat 2");
    }

    private void Eidos(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.EidosAstral, (uint)AID.EidosUmbral], delay, 2, "Change element");
    }

    private void HostileAspect1(uint id, float delay)
    {
        Cast(id, (uint)AID.HostileAspect, delay, 8, "Circle AOEs");
    }

    private void HostileAspect2(uint id, float delay)
    {
        CastStart(id, (uint)AID.HostileAspect, delay, "Circle AOEs appear");
        ComponentCondition<ExplosiveImpulse>(id + 0x10u, 7.4f, comp => comp.Casters.Count != 0, "Proximity AOE appears")
            .ActivateOnEnter<BrightDarkAuroraCounter>();
        CastEnd(id + 0x20u, 2f, "Circle AOEs resolve");
        ComponentCondition<ExplosiveImpulse>(id + 0x30u, 4.4f, comp => comp.Casters.Count == 0, "Proximity AOE resolves");
        CastMulti(id + 0x40u, [(uint)AID.EidosAstral, (uint)AID.EidosUmbral], 6.3f, 2, "Change element");
        ComponentCondition<BrightDarkAuroraCounter>(id + 0x50u, 1.9f, comp => comp.NumCasts == 2, "Half room cleaves 1");
        ComponentCondition<BrightDarkAuroraCounter>(id + 0x60u, 4.8f, comp => comp.NumCasts == 4, "Half room cleaves 2")
            .DeactivateOnExit<BrightDarkAuroraCounter>();
    }

    private void MedusaJavelin(uint id, float delay)
    {
        Cast(id, (uint)AID.MedusaJavelin, delay, 3f, "Cone AOE");
    }

    private void AuroralWind(uint id, float delay)
    {
        Cast(id, (uint)AID.AuroralWind, delay, 5f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void ImpactStreamBoss(uint id, float delay)
    {
        Cast(id, (uint)AID.ImpactStream1, delay, 3f, "Half room cleaves");
    }

    private void DarkBrightAuroraTowers(uint id, float delay)
    {
        ComponentCondition<DarkAuroraTether>(id, delay, comp => comp.Towers.Count != 0, "Towers + tethers appear");
    }

    private void RelativeVirtues(uint id, float delay)
    {
        ComponentCondition<ExplosiveImpulse>(id, delay, comp => comp.Casters.Count != 0, "Proximity AOEs appear")
            .ActivateOnEnter<BrightDarkAuroraCounter>();
        ComponentCondition<ExplosiveImpulse>(id + 0x10u, 5f, comp => comp.Casters.Count == 0, "Proximity AOEs resolve");
        ComponentCondition<BrightDarkAuroraCounter>(id + 0x20u, 8f, comp => comp.NumCasts == 2, "Half room cleaves 1");
        ComponentCondition<BrightDarkAuroraCounter>(id + 0x30u, 5.6f, comp => comp.NumCasts == 4, "Half room cleaves 2");
        ComponentCondition<BrightDarkAuroraCounter>(id + 0x40u, 4.4f, comp => comp.NumCasts == 6, "Half room cleaves 3")
            .DeactivateOnExit<BrightDarkAuroraCounter>();
        CastStart(id + 0x50u, (uint)AID.ExplosiveImpulse2, 1.3f, "Proximity AOE appears");
        CastEnd(id + 0x60u, 5f, "Proximity AOE resolves");
    }

    private void CallWyvern(uint id, float delay)
    {
        Cast(id, (uint)AID.CallWyvern, delay, 3, "Adds spawn");
    }
}
