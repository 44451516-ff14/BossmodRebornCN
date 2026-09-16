namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

sealed class Ex4BarbaricciaStates : StateMachineBuilder
{
    public Ex4BarbaricciaStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<StiffBreeze>(); // note: while it is really only active during two mechanics, it lingers for quite some time, so it's simpler to keep it always active
    }

    private void SinglePhase(uint id)
    {
        VoidAeroRaidwide(id, 9.2f);
        RagingStorm(id + 0x10000u, 7.2f);
        SavageBarberyHairRaid(id + 0x20000u, 3.3f);
        RagingStorm(id + 0x30000u, 1.9f);
        SavageBarberyHairRaid(id + 0x40000u, 3.3f);
        VoidAeroRaidwide(id + 0x50000u, 2.9f);
        VoidAeroTankbuster(id + 0x60000u, 2.1f);
        RagingStorm(id + 0x70000u, 7.2f);
        TeasingTangles1(id + 0x80000u, 3.1f);
        VoidAeroRaidwide(id + 0x90000u, 0.9f);
        RagingStorm(id + 0xA0000u, 7.2f);
        CurlingIron(id + 0xB0000u, 3.4f);
        Catabasis(id + 0xC0000u, 6.2f);

        BrutalRush(id + 0x100000u, 3f);
        WindingGaleBoulderBreak(id + 0x110000u, 1.6f);
        BrutalRush(id + 0x120000u, 1.7f);
        KnuckleDrum(id + 0x130000u, 3f);
        BlowAwayImpactBoldBoulderTrample(id + 0x140000u, 2.1f);
        TeasingTangles2(id + 0x150000u, 4.7f);
        KnuckleDrum(id + 0x160000u, 3.3f);

        IronOut(id + 0x200000u, 11.7f);
        RagingStorm(id + 0x210000u, 6.1f);
        EntanglementSecretBreeze(id + 0x220000u, 3.1f);
        SavageBarberyHairRaid(id + 0x230000u, 3.4f);
        VoidAeroRaidwide(id + 0x240000u, 3.9f);
        VoidAeroTankbuster(id + 0x250000u, 2.2f);
        RagingStorm(id + 0x260000u, 7.2f);
        EntanglementUpbraid(id + 0x270000u, 3.1f);
        SavageBarberyHairRaid(id + 0x280000u, 1.6f, true);
        VoidAeroRaidwide(id + 0x290000u, 4.3f);
        RagingStorm(id + 0x2A0000u, 7.2f);
        CurlingIron(id + 0x2B0000u, 3.2f);

        BrutalRush(id + 0x300000u, 4f);
        KnuckleDrum(id + 0x310000u, 3f);
        BlowAwayBoulders(id + 0x320000u, 2.1f);
        TornadoChainImpactHairSpray(id + 0x330000u, 1.5f);
        BrutalRushDryBlowsBoulderBreakWindingGale(id + 0x340000u, 0.6f);
        KnuckleDrum(id + 0x350000u, 6.2f);

        IronOut(id + 0x400000u, 10.5f);
        RagingStorm(id + 0x410000u, 6.1f);
        EntanglementSecretBreeze(id + 0x420000u, 3.1f);
        SavageBarberyHairRaid(id + 0x430000u, 3.3f);
        VoidAeroRaidwide(id + 0x440000u, 3.9f);
        RagingStorm(id + 0x450000u, 2.1f);
        Cast(id + 0x460000u, AID.Maelstrom, 3.4f, 9f, "Enrage");
    }

    private void VoidAeroTankbuster(uint id, float delay)
    {
        Cast(id, AID.VoidAeroTankbuster, delay, 5f, "Tankbuster")
            .ActivateOnEnter<VoidAeroTankbuster>()
            .DeactivateOnExit<VoidAeroTankbuster>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void VoidAeroRaidwide(uint id, float delay)
    {
        Cast(id, AID.VoidAeroRaidwide, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RagingStorm(uint id, float delay)
    {
        ComponentCondition<RagingStorm>(id, delay, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<RagingStorm>()
            .DeactivateOnExit<RagingStorm>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void CurlingIron(uint id, float delay)
    {
        Cast(id, AID.CurlingIron, delay, 5f);
        ComponentCondition<CurlingIron>(id + 0x10u, 8.2f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<CurlingIron>()
            .DeactivateOnExit<CurlingIron>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void IronOut(uint id, float delay)
    {
        ComponentCondition<IronOut>(id, delay, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<IronOut>()
            .DeactivateOnExit<IronOut>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Catabasis(uint id, float delay)
    {
        Targetable(id, false, delay, "Disappear");
        ComponentCondition<Catabasis>(id + 1u, 11f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Catabasis>()
            .DeactivateOnExit<Catabasis>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 2u, true, 0.1f, "Reappear");
    }

    private void KnuckleDrum(uint id, float delay)
    {
        ComponentCondition<KnuckleDrum>(id, delay, static comp => comp.NumCasts > 0, "Raidwide first hit")
            .ActivateOnEnter<KnuckleDrum>()
            .DeactivateOnExit<KnuckleDrum>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<KnuckleDrumLast>(id + 0x100u, 7.7f, static comp => comp.NumCasts > 0, "Raidwide last hit")
            .ActivateOnEnter<KnuckleDrumLast>()
            .DeactivateOnExit<KnuckleDrumLast>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void BrutalRush(uint id, float delay)
    {
        ComponentCondition<BrutalRush>(id, delay, static comp => comp.HavePendingRushes)
            .ActivateOnEnter<BrutalRush>();
        ComponentCondition<BrutalRush>(id + 1u, 3.4f, static comp => comp.NumCasts >= 1, "Charge 1");
        ComponentCondition<BrutalRush>(id + 2u, 1.7f, static comp => comp.NumCasts >= 2, "Charge 2");
        ComponentCondition<BrutalRush>(id + 3u, 1.7f, static comp => comp.NumCasts >= 3, "Charge 3");
        ComponentCondition<BrutalRush>(id + 4u, 1.7f, static comp => comp.NumCasts >= 4, "Charge 4")
            .DeactivateOnExit<BrutalRush>();
    }

    private void SavageBarberyHairRaid(uint id, float delay, bool fast = false)
    {
        CastMulti(id, [AID.SavageBarberyDonut1, AID.SavageBarberyDonut2, AID.SavageBarberyDonut3, AID.SavageBarberyDonut4, AID.SavageBarberyRect1, AID.SavageBarberyRect2], delay, 6f)
            .ActivateOnEnter<SavageBarbery>();
        ComponentCondition<SavageBarbery>(id + 0x10u, 1f, static comp => comp.NumActiveCasts < 2, "Donut/rect");
        ComponentCondition<SavageBarbery>(id + 0x20u, 2.1f, static comp => comp.NumActiveCasts == 0, "Sword")
            .DeactivateOnExit<SavageBarbery>();

        CastMulti(id + 0x1000u, [AID.HairRaidCone, AID.HairRaidDonut], fast ? 1 : 4.2f, 6f)
            .ActivateOnEnter<HairRaid>()
            .ActivateOnEnter<HairSprayDeadlyTwist>();
        ComponentCondition<HairRaid>(id + 0x1010u, 2f, static comp => comp.NumActiveCasts == 0, "Donut/cone")
            .DeactivateOnExit<HairRaid>();
        ComponentCondition<HairSprayDeadlyTwist>(id + 0x1020u, fast ? 1.9f : 2.3f, static comp => !comp.Active, "Stack/spread")
            .DeactivateOnExit<HairSprayDeadlyTwist>();
    }

    private void WindingGaleBoulderBreak(uint id, float delay)
    {
        ComponentCondition<WarningGale>(id, delay, static comp => comp.ActiveCasters.Length != 0)
            .ActivateOnEnter<WarningGale>();
        ComponentCondition<BoulderBreak>(id + 1u, 0.8f, static comp => comp.Active)
            .ActivateOnEnter<BoulderBreak>();
        ComponentCondition<WarningGale>(id + 2u, 4.2f, static comp => comp.NumCasts != 0, "Spirals")
            .ActivateOnEnter<WindingGale>()
            .DeactivateOnExit<WindingGale>()
            .DeactivateOnExit<WarningGale>();
        ComponentCondition<BoulderBreak>(id + 3u, 0.8f, static comp => comp.NumCasts != 0, "Shared tankbuster")
            .DeactivateOnExit<BoulderBreak>();

        ComponentCondition<WindingGaleCharge>(id + 0x10u, 2.8f, static comp => comp.Casters.Count != 0)
            .ActivateOnEnter<WindingGaleCharge>();
        ComponentCondition<WindingGaleCharge>(id + 0x11u, 2f, static comp => comp.Casters.Count > 6);
        ComponentCondition<WindingGaleCharge>(id + 0x12u, 0.5f, static comp => comp.Casters.Count <= 6);
        ComponentCondition<WindingGaleCharge>(id + 0x13u, 2f, static comp => comp.Casters.Count == 0)
            .DeactivateOnExit<WindingGaleCharge>();

        ComponentCondition<WarningGale>(id + 0x20u, 1.5f, static comp => comp.ActiveCasters.Length != 0)
            .ActivateOnEnter<WarningGale>();
        ComponentCondition<Boulder>(id + 0x21u, 1.1f, static comp => comp.Casters.Count != 0, "Bait")
            .ActivateOnEnter<WindingGale>()
            .ActivateOnEnter<Boulder>();
        ComponentCondition<BrittleBoulder>(id + 0x22u, 3f, static comp => comp.NumFinishedSpreads != 0, "Spread")
            .ActivateOnEnter<BrittleBoulder>()
            .DeactivateOnExit<BrittleBoulder>();

        ComponentCondition<HairFlayUpbraid>(id + 0x30u, 0.2f, static comp => comp.Active)
            .ActivateOnEnter<HairFlayUpbraid>();
        ComponentCondition<TornadoChainInner>(id + 0x31u, 0.4f, static comp => comp.ActiveCasters.Length != 0)
            .ActivateOnEnter<TornadoChainInner>();
        ComponentCondition<WarningGale>(id + 0x32u, 0.2f, static comp => comp.NumCasts != 0, "Spirals")
            .DeactivateOnExit<WarningGale>()
            .DeactivateOnExit<WindingGale>();
        ComponentCondition<Boulder>(id + 0x33u, 0.2f, static comp => comp.NumCasts != 0)
            .DeactivateOnExit<Boulder>();

        ComponentCondition<TornadoChainInner>(id + 0x40u, 3.6f, static comp => comp.NumCasts != 0, "Out")
            .DeactivateOnExit<TornadoChainInner>();
        ComponentCondition<TornadoChainOuter>(id + 0x41u, 2.5f, static comp => comp.NumCasts != 0, "In")
            .ActivateOnEnter<TornadoChainOuter>()
            .DeactivateOnExit<TornadoChainOuter>();

        ComponentCondition<HairFlayUpbraid>(id + 0x50u, 1.1f, static comp => !comp.Active, "Stack in pairs")
            .DeactivateOnExit<HairFlayUpbraid>();
    }

    private void BlowAwayImpactBoldBoulderTrample(uint id, float delay)
    {
        ComponentCondition<BlowAwayRaidwide>(id, delay, static comp => comp.NumCasts != 0)
            .ActivateOnEnter<BlowAwayRaidwide>()
            .DeactivateOnExit<BlowAwayRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BlowAwayPuddle>(id + 1u, 1.7f, static comp => comp.ActiveCasters.Length != 0, "Bait 1")
            .ActivateOnEnter<BlowAwayPuddle>();
        ComponentCondition<BrutalRush>(id + 2u, 0.7f, static comp => comp.HavePendingRushes)
            .ActivateOnEnter<BrutalRush>();
        // +1.3s: puddles 2 bait
        // +1.6s: rush 1 start
        // +3.2s: rush 2 start
        // +3.3s: puddles 1 finish + 3 bait
        // +3.6s: rush 1 finish
        // +4.8s: rush 3 start
        // +5.2s: rush 2 finish
        // +5.3s: puddles 2 finish + 4 bait
        // +6.4s: rush 4 start
        // +6.8s: rush 3 finish
        // +7.3s: puddles 3 finish
        // +8.4s: rush 4 finish
        // +9.3s: puddles 4 finish

        ComponentCondition<ImpactAOE>(id + 0x100u, 7.7f, static comp => comp.ActiveCasters.Length != 0)
            .ActivateOnEnter<ImpactAOE>();
        ComponentCondition<BrutalRush>(id + 0x110u, 0.7f, static comp => comp.NumCasts >= 4, "Charges")
            .ActivateOnEnter<ImpactKnockback>()
            .DeactivateOnExit<BrutalRush>();
        ComponentCondition<BoldBoulderTrample>(id + 0x120u, 0.3f, static comp => comp.Stacks.Count != 0)
            .ActivateOnEnter<BoldBoulderTrample>();
        ComponentCondition<BlowAwayPuddle>(id + 0x130u, 0.6f, static comp => comp.ActiveCasters.Length == 0)
            .DeactivateOnExit<BlowAwayPuddle>();
        ComponentCondition<ImpactAOE>(id + 0x200u, 4.7f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<ImpactAOE>()
            .DeactivateOnExit<ImpactKnockback>();
        ComponentCondition<BoldBoulderTrample>(id + 0x201u, 1.8f, static comp => comp.Spreads.Count == 0, "Flare");
        ComponentCondition<BoldBoulderTrample>(id + 0x202u, 0.3f, static comp => comp.Stacks.Count == 0, "Stack")
            .DeactivateOnExit<BoldBoulderTrample>();
    }

    private void BlowAwayBoulders(uint id, float delay)
    {
        ComponentCondition<BlowAwayRaidwide>(id, delay, static comp => comp.NumCasts != 0)
            .ActivateOnEnter<BlowAwayRaidwide>()
            .DeactivateOnExit<BlowAwayRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BlowAwayPuddle>(id + 1u, 1.7f, static comp => comp.ActiveCasters.Length != 0, "Bait 1")
            .ActivateOnEnter<BlowAwayPuddle>();
        // +2.0s: puddles 2 bait
        // +4.0s: puddles 1 finish + 3 bait
        // +4.3s: icons (for puddles that are to be baited into center)
        // +6.0s: puddles 2 finish + 4 bait
        // +8.0s: puddles 3 finish
        // +10.0s: puddles 4 finish

        ComponentCondition<BrutalRush>(id + 0x100u, 8.4f, static comp => comp.HavePendingRushes)
            .ActivateOnEnter<BrutalRush>();
        ComponentCondition<Boulder>(id + 0x110u, 1.1f, static comp => comp.Casters.Count != 0, "Bait center")
            .ActivateOnEnter<Boulder>();
        ComponentCondition<BrutalRush>(id + 0x120u, 2.5f, static comp => comp.NumCasts >= 1, "Charge 1")
            .ActivateOnEnter<BrittleBoulder>()
            .DeactivateOnExit<BlowAwayPuddle>();
        ComponentCondition<BrittleBoulder>(id + 0x130u, 0.5f, static comp => !comp.Active, "Spread")
            .DeactivateOnExit<BrittleBoulder>();
        ComponentCondition<Boulder>(id + 0x140u, 1, static comp => comp.NumCasts != 0)
            .DeactivateOnExit<Boulder>();
        ComponentCondition<BrutalRush>(id + 0x150u, 3.6f, static comp => comp.NumCasts >= 4, "Charge 4")
            .DeactivateOnExit<BrutalRush>();
    }

    private void TeasingTangles1(uint id, float delay)
    {
        Cast(id, AID.TeasingTangles1, delay, 4f)
            .ActivateOnEnter<Tangle>();
        ComponentCondition<Tangle>(id + 2u, 0.6f, static comp => comp.NumCasts != 0, "Tangles 1 start");
        ComponentCondition<Tangle>(id + 0x10u, 0.6f, static comp => comp.NumTethers != 0);
        ComponentCondition<HairFlayUpbraid>(id + 0x20u, 2.8f, static comp => comp.Active)
            .ActivateOnEnter<HairFlayUpbraid>();
        Cast(id + 0x30u, AID.SecretBreeze, 4.5f, 3)
            .ActivateOnEnter<SecretBreezeCones>();
        ComponentCondition<HairFlayUpbraid>(id + 0x40u, 0.5f, static comp => !comp.Active, "Stack/spread")
            .DeactivateOnExit<HairFlayUpbraid>();
        ComponentCondition<SecretBreezeCones>(id + 0x50u, 0.5f, static comp => comp.NumCasts != 0, "Cones")
            .DeactivateOnExit<SecretBreezeCones>();
        ComponentCondition<SecretBreezeProteans>(id + 0x60u, 2f, static comp => comp.NumCasts != 0, "Proteans")
            .ActivateOnEnter<SecretBreezeProteans>()
            .DeactivateOnExit<SecretBreezeProteans>();
        ComponentCondition<Tangle>(id + 0x70u, 3.2f, static comp => comp.NumTethers == 0, "Tangles 1 end")
            .DeactivateOnExit<Tangle>();
    }

    private void TeasingTangles2(uint id, float delay)
    {
        ComponentCondition<BrutalRush>(id, delay, static comp => comp.HavePendingRushes)
            .ActivateOnEnter<BrutalRush>();
        ComponentCondition<BrutalRush>(id + 1u, 3.4f, static comp => comp.NumCasts >= 1, "Charge 1");
        ComponentCondition<BrutalRush>(id + 2u, 1.7f, static comp => comp.NumCasts >= 2);
        ComponentCondition<BrutalRush>(id + 3u, 1.7f, static comp => comp.NumCasts >= 3)
            .ActivateOnEnter<Tangle>(); // activates ~0.1s after second charge
        ComponentCondition<BrutalRush>(id + 4u, 1.7f, static comp => comp.NumCasts >= 4, "Charge 4")
            .DeactivateOnExit<BrutalRush>();

        ComponentCondition<BlusteryRuler>(id + 0x10u, 0.4f, static comp => comp.ActiveCasters.Length != 0)
            .ActivateOnEnter<BlusteryRuler>();
        ComponentCondition<Tangle>(id + 0x20u, 0.8f, static comp => comp.NumCasts != 0, "Tangles 2 start");
        ComponentCondition<Tangle>(id + 0x21u, 0.5f, static comp => comp.NumTethers != 0);
        ComponentCondition<BlusteryRuler>(id + 0x30u, 3.7f, static comp => comp.ActiveCasters.Length == 0)
            .DeactivateOnExit<BlusteryRuler>();

        ComponentCondition<DryBlowsRaidwide>(id + 0x40u, 2.8f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<DryBlowsRaidwide>()
            .DeactivateOnExit<DryBlowsRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<TornadoChainInner>(id + 0x50u, 7.6f, static comp => comp.ActiveCasters.Length != 0)
            .ActivateOnEnter<TornadoChainInner>()
            .ActivateOnEnter<DryBlowsPuddle>();
        ComponentCondition<HairFlayUpbraid>(id + 0x51u, 0.3f, static comp => comp.Active)
            .ActivateOnEnter<HairFlayUpbraid>();
        ComponentCondition<TornadoChainInner>(id + 0x52u, 3.7f, static comp => comp.NumCasts != 0, "Out")
            .DeactivateOnExit<TornadoChainInner>();
        ComponentCondition<TornadoChainOuter>(id + 0x53u, 2.5f, static comp => comp.NumCasts != 0, "In")
            .ActivateOnEnter<TornadoChainOuter>()
            .DeactivateOnExit<TornadoChainOuter>();
        ComponentCondition<HairFlayUpbraid>(id + 0x54u, 1.8f, static comp => !comp.Active, "Stack in pairs")
            .DeactivateOnExit<HairFlayUpbraid>()
            .DeactivateOnExit<Tangle>()
            .DeactivateOnExit<DryBlowsPuddle>();
    }

    private void EntanglementSecretBreeze(uint id, float delay)
    {
        // TODO: component?..
        Cast(id, AID.Entanglement, delay, 4f, "Playstation");
        // +1.2s: tethers appear

        Cast(id + 0x10u, AID.SecretBreeze, 6.5f, 3f)
            .ActivateOnEnter<SecretBreezeCones>();
        ComponentCondition<SecretBreezeCones>(id + 0x12u, 1f, static comp => comp.NumCasts != 0, "Cones")
            .DeactivateOnExit<SecretBreezeCones>();
        ComponentCondition<SecretBreezeProteans>(id + 0x13u, 2f, static comp => comp.NumCasts != 0, "Proteans")
            .ActivateOnEnter<SecretBreezeProteans>()
            .DeactivateOnExit<SecretBreezeProteans>();
    }

    private void EntanglementUpbraid(uint id, float delay)
    {
        // TODO: component?..
        Cast(id, AID.Entanglement, delay, 4f, "Playstation");
        // +1.2s: tethers appear

        ComponentCondition<HairFlayUpbraid>(id + 0x10u, 6f, static comp => comp.Active)
            .ActivateOnEnter<HairFlayUpbraid>();
        ComponentCondition<HairFlayUpbraid>(id + 0x11u, 8f, static comp => !comp.Active, "Stack in pairs")
            .DeactivateOnExit<HairFlayUpbraid>();
    }

    private void TornadoChainImpactHairSpray(uint id, float delay)
    {
        ComponentCondition<TornadoChainInner>(id, delay, static comp => comp.ActiveCasters.Length != 0)
            .ActivateOnEnter<TornadoChainInner>();
        ComponentCondition<HairSprayDeadlyTwist>(id + 1u, 3.9f, static comp => comp.Active)
            .ActivateOnEnter<HairSprayDeadlyTwist>();
        ComponentCondition<TornadoChainInner>(id + 2u, 0.1f, static comp => comp.NumCasts != 0, "Out")
            .DeactivateOnExit<TornadoChainInner>();
        ComponentCondition<TornadoChainOuter>(id + 0x10u, 2.5f, static comp => comp.NumCasts != 0, "In")
            .ActivateOnEnter<TornadoChainOuter>()
            .ActivateOnEnter<ImpactAOE>() // starts ~0.2s after out finishes
            .ActivateOnEnter<ImpactKnockback>()
            .DeactivateOnExit<TornadoChainOuter>();
        ComponentCondition<ImpactAOE>(id + 0x20u, 3.9f, static comp => comp.NumCasts != 0, "Knockback")
            .DeactivateOnExit<ImpactAOE>()
            .DeactivateOnExit<ImpactKnockback>();
        ComponentCondition<HairSprayDeadlyTwist>(id + 0x21u, 1.5f, static comp => !comp.Active, "Spread")
            .DeactivateOnExit<HairSprayDeadlyTwist>();
    }

    private void BrutalRushDryBlowsBoulderBreakWindingGale(uint id, float delay)
    {
        ComponentCondition<BrutalRush>(id, delay, static comp => comp.HavePendingRushes)
            .ActivateOnEnter<BrutalRush>();
        ComponentCondition<BrutalRush>(id + 1u, 3.4f, static comp => comp.NumCasts >= 1, "Charge 1");
        ComponentCondition<BrutalRush>(id + 2u, 1.7f, static comp => comp.NumCasts >= 2)
            .ActivateOnEnter<BlusteryRuler>(); // activates ~1.2s after first charge
        ComponentCondition<BrutalRush>(id + 3u, 1.7f, static comp => comp.NumCasts >= 3);
        ComponentCondition<BrutalRush>(id + 4u, 1.7f, static comp => comp.NumCasts >= 4, "Charge 4")
            .DeactivateOnExit<BrutalRush>();
        ComponentCondition<BlusteryRuler>(id + 5u, 1, static comp => comp.NumCasts != 0)
            .DeactivateOnExit<BlusteryRuler>();

        ComponentCondition<DryBlowsRaidwide>(id + 0x10u, 3.1f, static comp => comp.NumCasts != 0)
            .ActivateOnEnter<DryBlowsRaidwide>()
            .DeactivateOnExit<DryBlowsRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<TornadoChainInner>(id + 0x20u, 5.6f, static comp => comp.ActiveCasters.Length != 0)
            .ActivateOnEnter<TornadoChainInner>()
            .ActivateOnEnter<DryBlowsPuddle>();
        ComponentCondition<TornadoChainInner>(id + 0x21u, 4f, static comp => comp.NumCasts != 0, "Out")
            .DeactivateOnExit<TornadoChainInner>();
        ComponentCondition<TornadoChainOuter>(id + 0x22u, 2.5f, static comp => comp.NumCasts != 0, "In")
            .ActivateOnEnter<BoulderBreak>() // <0.1s after out
            .ActivateOnEnter<TornadoChainOuter>()
            .DeactivateOnExit<TornadoChainOuter>();
        ComponentCondition<BoulderBreak>(id + 0x23u, 2.5f, static comp => comp.NumCasts != 0, "Shared tankbuster")
            .DeactivateOnExit<BoulderBreak>()
            .DeactivateOnExit<DryBlowsPuddle>();

        ComponentCondition<WarningGale>(id + 0x30u, 0.3f, static comp => comp.ActiveCasters.Length != 0)
            .ActivateOnEnter<WarningGale>();
        ComponentCondition<WarningGale>(id + 0x31u, 5f, static comp => comp.NumCasts != 0, "Spirals")
            .ActivateOnEnter<WindingGale>()
            .DeactivateOnExit<WindingGale>()
            .DeactivateOnExit<WarningGale>();

        ComponentCondition<WindingGaleCharge>(id + 0x40u, 3.7f, static comp => comp.Casters.Count != 0)
            .ActivateOnEnter<WindingGaleCharge>(); // tornado chain starts at the same time
        ComponentCondition<WindingGaleCharge>(id + 0x41u, 2f, static comp => comp.Casters.Count > 6)
            .ActivateOnEnter<TornadoChainInner>();
        ComponentCondition<WindingGaleCharge>(id + 0x42u, 0.5f, static comp => comp.Casters.Count <= 6);
        ComponentCondition<TornadoChainInner>(id + 0x43u, 1.5f, static comp => comp.NumCasts != 0, "Out")
            .DeactivateOnExit<TornadoChainInner>();
        ComponentCondition<WindingGaleCharge>(id + 0x44u, 0.5f, static comp => comp.Casters.Count == 0)
            .ActivateOnEnter<TornadoChainOuter>()
            .DeactivateOnExit<WindingGaleCharge>();

        ComponentCondition<WarningGale>(id + 0x50u, 1.8f, static comp => comp.ActiveCasters.Length != 0)
            .ActivateOnEnter<WarningGale>();
        ComponentCondition<TornadoChainOuter>(id + 0x51u, 0.2f, static comp => comp.NumCasts != 0, "In")
            .ActivateOnEnter<WindingGale>()
            .DeactivateOnExit<TornadoChainOuter>();
        ComponentCondition<BoldBoulderTrample>(id + 0x52u, 1.6f, static comp => comp.Stacks.Count != 0)
            .ActivateOnEnter<BoldBoulderTrample>();
        ComponentCondition<WarningGale>(id + 0x53u, 3.2f, static comp => comp.NumCasts != 0, "Spirals")
            .DeactivateOnExit<WarningGale>()
            .DeactivateOnExit<WindingGale>();
        ComponentCondition<BoldBoulderTrample>(id + 0x54u, 2.7f, static comp => comp.Stacks.Count == 0, "Stack");
        ComponentCondition<BoldBoulderTrample>(id + 0x55u, 1f, static comp => comp.Spreads.Count == 0, "Flare")
            .DeactivateOnExit<BoldBoulderTrample>();
    }
}
