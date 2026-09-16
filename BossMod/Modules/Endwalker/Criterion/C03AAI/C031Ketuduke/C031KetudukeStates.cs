namespace BossMod.Endwalker.VariantCriterion.C03AAI.C031Ketuduke;

abstract class C031KetudukeStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C031KetudukeStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        TidalRoar(id, 9.2f);
        FlukeGale(id + 0x10000u, 8.5f);
        BlowingBubbles(id + 0x20000u, 5.0f);
        StrewnBubbles(id + 0x30000u, 1.9f);
        Roar(id + 0x40000u, 5.6f);
        AngrySeas(id + 0x50000u, 10.2f);
        FlukeGale(id + 0x60000u, 5.7f);
        StrewnBubbles(id + 0x70000u, 4.9f);
        TidalRoar(id + 0x80000u, 5.5f);
        Cast(id + 0x90000u, AID.EnrageVisual, 7f, 10f, "Enrage");
    }

    private void TidalRoar(uint id, float delay)
    {
        Cast(id, AID.TidalRoar, delay, 5f);
        ComponentCondition<TidalRoar>(id + 0x10, 1, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<NTidalRoar>(!_savage)
            .ActivateOnEnter<STidalRoar>(_savage)
            .DeactivateOnExit<TidalRoar>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State BubbleNet(uint id, float delay, bool variant2)
    {
        Cast(id, variant2 ? AID.BubbleNet2 : AID.BubbleNet1, delay, 4.1f);
        return ComponentCondition<BubbleNet>(id + 2u, 0.9f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<NBubbleNet1>(!variant2 && !_savage)
            .ActivateOnEnter<SBubbleNet1>(!variant2 && _savage)
            .ActivateOnEnter<NBubbleNet2>(variant2 && !_savage)
            .ActivateOnEnter<SBubbleNet2>(variant2 && _savage)
            .DeactivateOnExit<BubbleNet>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void FlukeGale(uint id, float delay)
    {
        Cast(id, AID.SpringCrystals, delay, 2.2f)
            .ActivateOnEnter<SpringCrystalsRectMove>();
        BubbleNet(id + 0x10u, 3f, false)
            .ActivateOnEnter<FlukeGale>();
        CastMulti(id + 0x20u, [AID.Hydrofall, AID.Hydrobullet], 2.2f, 4f)
            .ActivateOnEnter<HydrofallHydrobullet>();
        Cast(id + 0x30u, AID.FlukeGale, 6.2f, 3f);
        ComponentCondition<FlukeGale>(id + 0x40, 2.1f, static comp => comp.Gales.Count > 0)
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<FlukeGale>(id + 0x50u, 8f, static comp => comp.NumCasts >= 2, "Knockbacks 1");
        ComponentCondition<FlukeGale>(id + 0x51u, 2f, static comp => comp.NumCasts >= 4, "Knockbacks 2")
            .ExecOnEnter<HydrofallHydrobullet>(static comp => comp.Activate(0)) // TODO: consider activating earlier?..
            .DeactivateOnExit<FlukeGale>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
        ComponentCondition<SpringCrystalsRect>(id + 0x60u, 3.1f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<SpringCrystalsRect>();
        ComponentCondition<HydrofallHydrobullet>(id + 0x61u, 0.2f, static comp => comp.ActiveMechanic > 0, "Stack/spread")
            .DeactivateOnExit<HydrofallHydrobullet>();
    }

    private void BlowingBubbles(uint id, float delay)
    {
        CastMulti(id, [AID.Hydrofall, AID.Hydrobullet], delay, 4f)
            .ActivateOnEnter<HydrofallHydrobullet>()
            .ExecOnEnter<HydrofallHydrobullet>(static comp => comp.Activate(0));
        ComponentCondition<HydrofallHydrobullet>(id + 0x10u, 3.1f, static comp => comp.Mechanics.Count > 1);
        Cast(id + 0x20u, AID.BlowingBubbles, 3.1f, 4.2f)
            .ActivateOnEnter<BlowingBubbles>();
        Cast(id + 0x30u, AID.Hydrobomb, 5.0f, 2.2f);
        ComponentCondition<Hydrobomb>(id + 0x40u, 1.1f, static comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<NHydrobomb>(!_savage)
            .ActivateOnEnter<SHydrobomb>(_savage)
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<HydrofallHydrobullet>(id + 0x50u, 3.8f, static comp => comp.ActiveMechanic > 0, "Spread/stack");
        ComponentCondition<HydrofallHydrobullet>(id + 0x60u, 6.1f, static comp => comp.ActiveMechanic > 1, "Stack/spread")
            .DeactivateOnExit<HydrofallHydrobullet>();
        ComponentCondition<Hydrobomb>(id + 0x70u, 5.2f, static comp => comp.Casters.Count == 0, "Puddles/exaflares resolve")
            .DeactivateOnExit<Hydrobomb>()
            .DeactivateOnExit<BlowingBubbles>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void StrewnBubbles(uint id, float delay)
    {
        Cast(id, AID.Hydrofall, delay, 4f)
            .ActivateOnEnter<HydrofallHydrobullet>()
            .ExecOnEnter<HydrofallHydrobullet>(static comp => comp.Activate(0));
        Cast(id + 0x10u, AID.StrewnBubbles, 3.2f, 2.2f)
            .ActivateOnEnter<StrewnBubbles>(); // first set appears ~1.4s after cast end
        CastStartMulti(id + 0x20u, [_savage ? AID.SRecedingTwintides : AID.NRecedingTwintides, _savage ? AID.SEncroachingTwintides : AID.NEncroachingTwintides], 6.5f)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 0x21u, 5f, "In/out")
            .ActivateOnEnter<RecedingEncroachingTwintides>();
        ComponentCondition<StrewnBubbles>(id + 0x22u, 0.6f, static comp => comp.NumCasts > 0);
        ComponentCondition<RecedingEncroachingTwintides>(id + 0x30u, 2.5f, static comp => comp.NumCasts > 1, "Out/in")
            .DeactivateOnExit<RecedingEncroachingTwintides>();
        ComponentCondition<StrewnBubbles>(id + 0x31u, 0.5f, static comp => comp.NumCasts > 4)
            .DeactivateOnExit<StrewnBubbles>();
        ComponentCondition<HydrofallHydrobullet>(id + 0x32u, 0.1f, static comp => comp.ActiveMechanic > 0, "Stack")
            .DeactivateOnExit<HydrofallHydrobullet>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void Roar(uint id, float delay)
    {
        Cast(id, AID.Hydrobullet, delay, 4f)
            .ActivateOnEnter<HydrofallHydrobullet>()
            .ExecOnEnter<HydrofallHydrobullet>(static comp => comp.Activate(0));
        Cast(id + 0x10u, AID.Roar, 3.2f, 3f)
            .ActivateOnEnter<Roar>(); // zaratans spawn ~1.2s after cast ends
        Cast(id + 0x20u, AID.SpringCrystals, 2.6f, 2.2f)
            .ActivateOnEnter<SpringCrystalsRectStay>();
        BubbleNet(id + 0x30u, 6.0f, true)
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<HydrofallHydrobullet>(id + 0x40u, 11.5f, static comp => comp.ActiveMechanic > 0, "Spread")
            .DeactivateOnExit<HydrofallHydrobullet>();
        ComponentCondition<SpringCrystalsRect>(id + 0x41u, 0.6f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<SpringCrystalsRect>();
        Cast(id + 0x50u, AID.Updraft, 1.0f, 4.2f)
            .ExecOnEnter<Roar>(static comp => comp.Active = true);
        ComponentCondition<Roar>(id + 0x60u, 2.6f, static comp => comp.NumCasts > 0, "Bait resolve")
            .DeactivateOnExit<Roar>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void AngrySeas(uint id, float delay)
    {
        CastMulti(id, [AID.Hydrofall, AID.Hydrobullet], delay, 4)
            .ActivateOnEnter<HydrofallHydrobullet>()
            .ExecOnEnter<HydrofallHydrobullet>(static comp => comp.Activate(0));
        ComponentCondition<HydrofallHydrobullet>(id + 0x10u, 3.1f, static comp => comp.Mechanics.Count > 1);
        Cast(id + 0x20u, AID.AngrySeas, 3.1f, 4.2f)
            .ActivateOnEnter<AngrySeasAOE>()
            .ActivateOnEnter<AngrySeasKnockback>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<AngrySeasKnockback>(id + 0x22u, 0.8f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<AngrySeasKnockback>();
        ComponentCondition<HydrofallHydrobullet>(id + 0x30u, 1.2f, static comp => comp.ActiveMechanic > 0, "Stack/spread");

        Cast(id + 0x40u, AID.SpringCrystals, 0.9f, 2.2f)
            .ActivateOnEnter<SpringCrystalsSphere>();
        ComponentCondition<HydrofallHydrobullet>(id + 0x50u, 2.0f, static comp => comp.ActiveMechanic > 1, "Spread/stack")
            .DeactivateOnExit<HydrofallHydrobullet>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
        BubbleNet(id + 0x60u, 0.9f, false)
            .ActivateOnEnter<FlukeTyphoonBurst>();

        Cast(id + 0x100u, AID.FlukeTyphoon, 2.2f, 3, "Bubbles");
        ComponentCondition<FlukeTyphoon>(id + 0x110u, 6.1f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<FlukeTyphoon>()
            .DeactivateOnExit<FlukeTyphoon>();
        ComponentCondition<SpringCrystalsSphere>(id + 0x120u, 2.6f, static comp => comp.NumCasts > 0, "Circles")
            .DeactivateOnExit<SpringCrystalsSphere>();
        ComponentCondition<FlukeTyphoonBurst>(id + 0x130u, 2.4f, static comp => comp.NumCasts > 0, "Towers")
            .DeactivateOnExit<FlukeTyphoonBurst>()
            .DeactivateOnExit<AngrySeasAOE>();
    }
}

sealed class C031NKetudukeStates(BossModule module) : C031KetudukeStates(module, false);
sealed class C031SKetudukeStates(BossModule module) : C031KetudukeStates(module, true);
