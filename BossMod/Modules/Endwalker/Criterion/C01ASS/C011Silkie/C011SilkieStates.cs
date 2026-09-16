namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

abstract class C011SilkieStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C011SilkieStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<PuffTracker>();
    }

    private void SinglePhase(uint id)
    {
        FizzlingSuds(id, 8.2f);
        DustBluster(id + 0x10000u, 5.2f);
        SqueakyCleanSlipperySoap(id + 0x20000u, 2.3f);
        TotalWash(id + 0x30000u, 6.2f);
        Tethers1(id + 0x40000u, 2.1f);
        SudsSlipperySoap(id + 0x50000u, 2.1f, true);
        TotalWash(id + 0x60000u, 9.6f);
        EasternEwers(id + 0x70000u, 2.1f);
        ChillingSudsCarpetBeaterSlipperySoap(id + 0x80000u, 4.2f);
        BracingSudsSoapingSpree(id + 0x90000u, 9.5f);
        DustBluster(id + 0xA0000u, 6.2f);
        Tethers2(id + 0xB0000u, 2.3f);
        TotalWash(id + 0xC0000u, 2.1f);
        SudsSlipperySoap(id + 0xD0000u, 6.2f);
        SudsSlipperySoap(id + 0xE0000u, 2.5f);
        Cast(id + 0xF0000u, AID.Enrage, 3.2f, 10f, "Enrage");
    }

    private void CarpetBeater(uint id, float delay)
    {
        Cast(id, _savage ? AID.SCarpetBeater : AID.NCarpetBeater, delay, 5f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void TotalWash(uint id, float delay)
    {
        Cast(id, _savage ? AID.STotalWash : AID.NTotalWash, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State SoapingSpree(uint id, float delay, string name = "Puffs")
    {
        return Cast(id, _savage ? AID.SSoapingSpreeBoss : AID.NSoapingSpreeBoss, delay, 5f, name)
            .ActivateOnEnter<NChillingDusterPuff>(!_savage)
            .ActivateOnEnter<NBracingDusterPuff>(!_savage)
            .ActivateOnEnter<NFizzlingDusterPuff>(!_savage)
            .ActivateOnEnter<SChillingDusterPuff>(_savage)
            .ActivateOnEnter<SBracingDusterPuff>(_savage)
            .ActivateOnEnter<SFizzlingDusterPuff>(_savage)
            .DeactivateOnExit<ChillingDusterPuff>()
            .DeactivateOnExit<BracingDusterPuff>()
            .DeactivateOnExit<FizzlingDuster>();
    }

    private void FizzlingSuds(uint id, float delay)
    {
        Cast(id, _savage ? AID.SFizzlingSuds : AID.NFizzlingSuds, delay, 3f);
        Cast(id + 0x10u, _savage ? AID.SSoapsUp : AID.NSoapsUp, 2.1f, 4f)
            .ActivateOnEnter<NFizzlingDuster>(!_savage)
            .ActivateOnEnter<SFizzlingDuster>(_savage);
        ComponentCondition<FizzlingDuster>(id + 0x20u, 1f, static comp => comp.NumCasts > 0, "Cones")
            .DeactivateOnExit<FizzlingDuster>();
    }

    private void DustBluster(uint id, float delay)
    {
        Cast(id, _savage ? AID.SDustBluster : AID.NDustBluster, delay, 5f, "Knockback")
            .ActivateOnEnter<NDustBluster>(!_savage)
            .ActivateOnEnter<SDustBluster>(_savage)
            .DeactivateOnExit<DustBluster>();
    }

    private void SqueakyClean(uint id, float delay)
    {
        CastMulti(id, [_savage ? AID.SSqueakyCleanE : AID.NSqueakyCleanE, _savage ? AID.SSqueakyCleanW : AID.NSqueakyCleanW], delay, 4.5f)
            .ActivateOnEnter<NSqueakyCleanE>(!_savage)
            .ActivateOnEnter<NSqueakyCleanW>(!_savage)
            .ActivateOnEnter<SSqueakyCleanE>(_savage)
            .ActivateOnEnter<SSqueakyCleanW>(_savage);
        Condition(id + 2u, 4.7f, () => Module.FindComponent<SqueakyCleanE>()!.NumCasts + Module.FindComponent<SqueakyCleanW>()!.NumCasts > 0, "Recolor")
            .DeactivateOnExit<SqueakyCleanE>()
            .DeactivateOnExit<SqueakyCleanW>();
    }

    private void SlipperySoap(uint id, float delay, bool longCharge = false)
    {
        CastStart(id, _savage ? AID.SSlipperySoap : AID.NSlipperySoap, delay)
            .ActivateOnEnter<SlipperySoapCharge>(); // target selection and cast start happen at the same time
        CastEnd(id + 1u, 5f);
        ComponentCondition<SlipperySoapCharge>(id + 2u, longCharge ? 0.5f : 0.3f, static comp => !comp.ChargeImminent, "Charge")
            .DeactivateOnExit<SlipperySoapCharge>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SlipperySoapAOE>(id + 3u, 3.6f, static comp => !comp.Active, "AOE")
            .ActivateOnEnter<SlipperySoapAOE>()
            .ActivateOnEnter<SoapsudStatic>()
            .DeactivateOnExit<SlipperySoapAOE>()
            .DeactivateOnExit<SoapsudStatic>();
    }

    private void SudsSlipperySoap(uint id, float delay, bool longCharge = false)
    {
        CastMulti(id, [_savage ? AID.SBracingSuds : AID.NBracingSuds, _savage ? AID.SChillingSuds : AID.NChillingSuds, _savage ? AID.SFizzlingSuds : AID.NFizzlingSuds], delay, 3f);
        SlipperySoap(id + 0x100u, 2.1f, longCharge);
    }

    private void ChillingSudsCarpetBeaterSlipperySoap(uint id, float delay)
    {
        Cast(id, _savage ? AID.SChillingSuds : AID.NChillingSuds, delay, 3f);
        CarpetBeater(id + 0x100u, 2.2f);
        SlipperySoap(id + 0x200u, 2.1f);
    }

    private void SqueakyCleanSlipperySoap(uint id, float delay)
    {
        Cast(id, _savage ? AID.SFreshPuff : AID.NFreshPuff, delay, 4f);
        // +1.1s: debuffs on 3 puffs with offsets (0, -10), (12.5, 7), (-12.5, 7); no point showing anything for them, since they will be recolored
        Cast(id + 0x10u, _savage ? AID.SBracingSuds : AID.NBracingSuds, 2.1f, 3f);

        // TODO: as soon as next cast starts, we can determine desired position for bait: N if remaining puff is yellow, recolored side if remaining puff is blue
        SqueakyClean(id + 0x20u, 2.1f);

        SudsSlipperySoap(id + 0x1000u, 4.2f);
        // TODO: consider starting showing puff aoes now

        CarpetBeater(id + 0x2000u, 2.6f);
        SoapingSpree(id + 0x3000u, 4.1f);
    }

    private void EasternEwers(uint id, float delay)
    {
        Cast(id, _savage ? AID.SFreshPuff : AID.NFreshPuff, delay, 4f);
        Cast(id + 0x10u, _savage ? AID.SEasternEwers : AID.NEasternEwers, 2.1f, 4f);
        ComponentCondition<EasternEwers>(id + 0x20u, 1.2f, static comp => comp.Active)
            .ActivateOnEnter<EasternEwers>();
        // TODO: consider showing puff aoes early, as soon as exaflares start...
        SoapingSpree(id + 0x1000u, 10f, "Exaflare + Puffs")
            .DeactivateOnExit<EasternEwers>();
    }

    // TODO: consider grouping with prev and showing early blue puff hints
    private void BracingSudsSoapingSpree(uint id, float delay)
    {
        Cast(id, _savage ? AID.SBracingSuds : AID.NBracingSuds, delay, 3f);
        SoapingSpree(id + 0x100u, 2.2f);
    }

    private void Tethers1(uint id, float delay)
    {
        CastMulti(id, [_savage ? AID.SBracingSuds : AID.NBracingSuds, _savage ? AID.SChillingSuds : AID.NChillingSuds], delay, 3f)
            .ActivateOnEnter<PuffTethers1>();
        Cast(id + 0x10u, _savage ? AID.SFreshPuff : AID.NFreshPuff, 2.1f, 4f);
        // +1.1s: puffs appear
        // +3.2s: tethers appear
        // +9.4s/+10.1s: puff and tumble start
        // +10.9s/+11.7s: puff and tumble end

        SoapingSpree(id + 0x1000u, 12.3f, "Tethers + Puffs")
            .DeactivateOnExit<PuffTethers1>();
    }

    private void Tethers2(uint id, float delay)
    {
        Cast(id, _savage ? AID.SBracingSuds : AID.NBracingSuds, delay, 3f)
            .ActivateOnEnter<PuffTethers2>();
        Cast(id + 0x10u, _savage ? AID.SFreshPuff : AID.NFreshPuff, 2.1f, 4f);
        // +1.1s: puffs appear
        // +3.2s: tethers appear
        // +10.0s: puff and tumble start
        // +11.6s: puff and tumble end

        SqueakyClean(id + 0x20u, 2.1f);
        SoapingSpree(id + 0x30u, 4.1f, "Tethers + Puffs")
            .DeactivateOnExit<PuffTethers2>();
    }
}

sealed class C011NSilkieStates(BossModule module) : C011SilkieStates(module, false);
sealed class C011SSilkieStates(BossModule module) : C011SilkieStates(module, true);
