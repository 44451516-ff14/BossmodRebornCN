namespace BossMod.Dawntrail.Savage.M02SHoneyBLovely;

sealed class M02SHoneyBLovelyStates : StateMachineBuilder
{
    public M02SHoneyBLovelyStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        CallMeHoney(id, 5.2f);
        DropSplashOfVenomHoneyBeelineTemptingTwist(id + 0x10000u, 2.1f, false);
        DropSplashOfVenomHoneyBeelineTemptingTwist(id + 0x20000u, 1.0f, true);
        StingingSlashKillerSting(id + 0x30000u, 3.0f);
        Beat1(id + 0x40000u, 10.5f);
        StingingSlashKillerSting(id + 0x50000u, 10.2f);
        AlarmPheromones1(id + 0x60000u, 4.2f);
        Beat2(id + 0x70000u, 7.8f);
        AlarmPheromones2(id + 0x80000u, 13.5f);
        Beat3(id + 0x90000u, 11.3f);
        StingingSlashKillerSting(id + 0xA0000u, 9.2f);
        RottenHeart(id + 0xB0000u, 11.4f);
        Cast(id + 0xC0000u, AID.SheerHeartAttack, 12.2f, 10f, "Enrage");
    }

    private State CallMeHoney(uint id, float delay, string name = "Raidwide")
    {
        return Cast(id, AID.CallMeHoney, delay, 5f, name)
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HoneyBeelineTemptingTwistResolve(uint id, float delay)
    {
        CastMulti(id, [AID.HoneyBeeline, AID.TemptingTwist], delay, 5.5f)
            .ActivateOnEnter<HoneyBeeline>()
            .ActivateOnEnter<TemptingTwist>();
        Condition(id + 2u, 0.7f, () => Module.FindComponent<HoneyBeeline>()?.NumCasts > 0 || Module.FindComponent<TemptingTwist>()?.NumCasts > 0, "In/out")
            .DeactivateOnExit<HoneyBeeline>()
            .DeactivateOnExit<TemptingTwist>();
        ComponentCondition<DropSplashOfVenom>(id + 3u, 4.5f, static comp => !comp.Active, "Pairs/spread")
            .ActivateOnEnter<PoisonCloudSplinter>()
            .DeactivateOnExit<DropSplashOfVenom>()
            .DeactivateOnExit<PoisonCloudSplinter>(); // resolves at the same time
    }

    private void HoneyBeelineTemptingTwistBeatResolve(uint id, float delay)
    {
        CastMulti(id, [AID.HoneyBeelineBeat, AID.TemptingTwistBeat], delay, 5.5f)
            .ActivateOnEnter<HoneyBeelineBeat>()
            .ActivateOnEnter<TemptingTwistBeat>();
        Condition(id + 2u, 0.7f, () => Module.FindComponent<HoneyBeelineBeat>()?.NumCasts > 0 || Module.FindComponent<TemptingTwistBeat>()?.NumCasts > 0, "In/out")
            .DeactivateOnExit<HoneyBeelineBeat>()
            .DeactivateOnExit<TemptingTwistBeat>();
        ComponentCondition<DropSplashOfVenom>(id + 3u, 4.5f, static comp => !comp.Active, "Pairs/spread")
            .ActivateOnEnter<SweetheartSplinter>()
            .DeactivateOnExit<DropSplashOfVenom>()
            .DeactivateOnExit<SweetheartSplinter>(); // resolves at the same time
    }

    private void DropSplashOfVenomHoneyBeelineTemptingTwist(uint id, float delay, bool shortInOutDelay)
    {
        CastMulti(id, [AID.SplashOfVenom, AID.DropOfVenom], delay, 5)
            .ActivateOnEnter<DropSplashOfVenom>();
        HoneyBeelineTemptingTwistResolve(id + 0x10, shortInOutDelay ? 2.1f : 5.5f);
    }

    private State StingingSlashKillerSting(uint id, float delay)
    {
        CastStartMulti(id, [AID.StingingSlash, AID.KillerSting], delay) // icons appear right before cast start
            .ActivateOnEnter<StingingSlash>()
            .ActivateOnEnter<KillerSting>();
        CastEnd(id + 1u, 4f);
        return Condition(id + 2u, 1f, () => Module.FindComponent<StingingSlash>()?.NumCasts > 0 || Module.FindComponent<KillerSting>()?.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<StingingSlash>()
            .DeactivateOnExit<KillerSting>() // note that killer sting happens ~0.3s later, not a huge deal
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void CenterOuterStageComboResolve(uint id, float delay, bool activateComponent)
    {
        CastEnd(id, delay)
            .ActivateOnEnter<StageCombo>(activateComponent);
        ComponentCondition<StageCombo>(id + 1u, 1.2f, static comp => comp.NumCasts > 0, "In/out");
        ComponentCondition<StageCombo>(id + 2u, 3.0f, static comp => comp.NumCasts > 5, "Cross");
        ComponentCondition<StageCombo>(id + 3u, 3.3f, static comp => comp.NumCasts > 6, "Out/in")
            .DeactivateOnExit<StageCombo>();
    }

    private void CenterOuterStageCombo(uint id, float delay)
    {
        CastStartMulti(id, [AID.CenterstageCombo, AID.OuterstageCombo], delay);
        CenterOuterStageComboResolve(id + 1u, 5f, true);
    }

    private void Beat1(uint id, float delay)
    {
        Cast(id, AID.HoneyBLiveBeat1, delay, 2f);
        ComponentCondition<HoneyBLiveBeat1>(id + 2u, 6.3f, static comp => comp.NumCasts > 0, "Beat 1 raidwide")
            .ActivateOnEnter<HoneyBLiveBeat1>()
            .DeactivateOnExit<HoneyBLiveBeat1>()
            .SetHint(StateMachine.StateHint.Raidwide);

        CenterOuterStageCombo(id + 0x100u, 2.9f);

        Cast(id + 0x200u, AID.LoveMeTender, 4.2f, 4f);
        ComponentCondition<Fracture>(id + 0x210u, 2.2f, static comp => comp.Towers.Count > 0)
            .ActivateOnEnter<HoneyBLiveHearts>()
            .ActivateOnEnter<Fracture1>();
        ComponentCondition<Fracture>(id + 0x220u, 8f, static comp => comp.NumCasts > 0, "First tower"); // one or two towers every 4s, 11 towers total
        ComponentCondition<Fracture>(id + 0x230u, 20f, static comp => comp.NumCasts >= 11, "Last tower")
            .DeactivateOnExit<Fracture>();

        Cast(id + 0x300u, AID.Loveseeker, 0.1f, 3f)
            .ActivateOnEnter<Loveseeker>();
        ComponentCondition<Loveseeker>(id + 0x302u, 1f, static comp => comp.NumCasts > 0, "Out")
            .ActivateOnEnter<SweetheartsS>() // adds activate right after resolve
            .DeactivateOnExit<Loveseeker>();

        Cast(id + 0x400u, AID.LoveMeTender, 9.2f, 4);
        ComponentCondition<Heartsick>(id + 0x410u, 1.1f, static comp => comp.Stacks.Count > 0)
            .ActivateOnEnter<Heartsick1>();
        ComponentCondition<Heartsick>(id + 0x420u, 7.1f, static comp => comp.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<Heartsick>()
            .DeactivateOnExit<HoneyBLiveHearts>();

        CenterOuterStageCombo(id + 0x500u, 5.2f);
        Cast(id + 0x600u, AID.HoneyBFinale, 6.2f, 5f, "Beat 1 end raidwide")
            .DeactivateOnExit<SweetheartsS>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void AlarmPheromones1(uint id, float delay)
    {
        Cast(id, AID.AlarmPheromones, delay, 3f);
        ComponentCondition<BlindingLoveBait>(id + 0x10u, 3.6f, static comp => comp.Casters.Count > 0) // every 1.2s, 16 total
            .ActivateOnEnter<BlindingLoveBait>();
        ComponentCondition<BlindingLoveBait>(id + 0x20u, 7f, static comp => comp.NumCasts > 0, "Baited charges start");
        ComponentCondition<BlindingLoveBait>(id + 0x30u, 18f, static comp => comp.NumCasts >= 16, "Baited charges end")
            .DeactivateOnExit<BlindingLoveBait>();
    }

    private void Beat2(uint id, float delay)
    {
        Cast(id, AID.HoneyBLiveBeat2, delay, 2f);
        ComponentCondition<HoneyBLiveBeat2>(id + 2u, 6.3f, static comp => comp.NumCasts > 0, "Beat 2 raidwide")
            .ActivateOnEnter<HoneyBLiveBeat2>()
            .DeactivateOnExit<HoneyBLiveBeat2>()
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x10u, [AID.SpreadLove, AID.DropOfLove], 2.8f, 5f)
            .ActivateOnEnter<DropSplashOfVenom>();

        Cast(id + 0x100u, AID.LoveMeTender, 6.2f, 4f);
        ComponentCondition<Heartsick>(id + 0x110u, 1.1f, static comp => comp.Stacks.Count > 0, "Puddles bait start")
            .ActivateOnEnter<HoneyBLiveHearts>()
            .ActivateOnEnter<Heartsick2>();
        ComponentCondition<Fracture>(id + 0x120u, 6.1f, static comp => comp.Towers.Count > 0)
            .ActivateOnEnter<HeartStruck>() // first puddles appear together with stack icons
            .ActivateOnEnter<Heartsore>() // spread markers appear just before towers
            .ActivateOnEnter<Fracture2>();
        ComponentCondition<Heartsick>(id + 0x130u, 1f, static comp => comp.NumFinishedStacks > 0, "Stacks")
            .DeactivateOnExit<Heartsick>();
        ComponentCondition<Heartsore>(id + 0x140u, 6f, static comp => comp.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<HeartStruck>() // last puddle resolves ~2s before spreads
            .DeactivateOnExit<Heartsore>();
        ComponentCondition<Fracture>(id + 0x150u, 1f, static comp => comp.NumCasts > 0, "Towers")
            .DeactivateOnExit<Fracture>()
            .DeactivateOnExit<HoneyBLiveHearts>();

        HoneyBeelineTemptingTwistBeatResolve(id + 0x200u, 3.7f);

        Cast(id + 0x300u, AID.HoneyBFinale, 4f, 5f, "Beat 2 end raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void AlarmPheromones2(uint id, float delay)
    {
        CastMulti(id, [AID.SplashOfVenom, AID.DropOfVenom], delay, 5f)
            .ActivateOnEnter<DropSplashOfVenom>();

        Cast(id + 0x100u, AID.AlarmPheromones, 2.1f, 3f);
        Cast(id + 0x110u, AID.PoisonSting, 3.2f, 4.7f)
            .ActivateOnEnter<PoisonStingBait>()
            .ActivateOnEnter<PoisonStingVoidzone>() // voidzones drop ~0.8s after puddle cast
            .ActivateOnEnter<BlindingLoveCharge1>() // first charges begin ~0.4s into cast
            .ActivateOnEnter<BlindingLoveCharge2>();
        ComponentCondition<PoisonStingBait>(id + 0x120u, 1.3f, static comp => comp.NumCasts > 0, "Puddle drop 1");
        // +0.4s: charges 1
        ComponentCondition<PoisonStingBait>(id + 0x130u, 5.0f, static comp => comp.NumCasts > 2, "Puddle drop 2");
        // +0.4s: charges 2
        ComponentCondition<PoisonStingBait>(id + 0x140u, 5.0f, static comp => comp.NumCasts > 4, "Puddle drop 3");
        // +0.4s: charges 3
        ComponentCondition<PoisonStingBait>(id + 0x150u, 5.0f, static comp => comp.NumCasts > 6, "Puddle drop 4")
            .DeactivateOnExit<PoisonStingBait>();
        // +0.4s: charges 4

        Cast(id + 0x200u, AID.BeeSting, 5f, 4f)
            .ActivateOnEnter<BeeSting>();
        ComponentCondition<BeeSting>(id + 0x210u, 1f, static comp => comp.NumFinishedStacks > 0, "Role stacks")
            .DeactivateOnExit<BeeSting>();
        // +0.2s: last charge

        StingingSlashKillerSting(id + 0x300u, 3.1f)
            .DeactivateOnExit<PoisonStingVoidzone>()
            .DeactivateOnExit<BlindingLoveCharge1>()
            .DeactivateOnExit<BlindingLoveCharge2>();

        HoneyBeelineTemptingTwistResolve(id + 0x400u, 6.8f);
    }

    private void Beat3(uint id, float delay)
    {
        Cast(id, AID.HoneyBLiveBeat3, delay, 2f);
        ComponentCondition<HoneyBLiveBeat3>(id + 2u, 6.3f, static comp => comp.NumCasts > 0, "Beat 3 raidwide")
            .ActivateOnEnter<HoneyBLiveBeat3>()
            .ActivateOnEnter<HoneyBLiveBeat3BigBurst>() // statuses appear right before raidwide
            .DeactivateOnExit<HoneyBLiveBeat3>()
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x10u, [AID.SpreadLove, AID.DropOfLove], 1.9f, 5f)
            .ActivateOnEnter<DropSplashOfVenom>();

        CenterOuterStageCombo(id + 0x100u, 2.1f);
        ComponentCondition<HoneyBLiveBeat3BigBurst>(id + 0x110u, 4.6f, static comp => comp.NumCasts > 0, "Defamations 1")
            .ActivateOnEnter<Fracture3>();
        CastStartMulti(id + 0x120u, [AID.CenterstageCombo, AID.OuterstageCombo], 2.6f);
        ComponentCondition<Fracture>(id + 0x121u, 0.8f, static comp => comp.NumCasts > 0, "Towers 1")
            .ActivateOnEnter<StageCombo>()
            .DeactivateOnExit<Fracture>();
        CenterOuterStageComboResolve(id + 0x122u, 4.2f, false);
        ComponentCondition<HoneyBLiveBeat3BigBurst>(id + 0x130u, 5.0f, static comp => comp.NumCasts > 4, "Defamations 2")
            .ActivateOnEnter<Fracture3>();
        ComponentCondition<Fracture>(id + 0x140u, 3.0f, static comp => comp.NumCasts > 0, "Towers 2")
            .DeactivateOnExit<Fracture>()
            .DeactivateOnExit<HoneyBLiveBeat3BigBurst>();

        HoneyBeelineTemptingTwistBeatResolve(id + 0x200u, 4.2f);

        Cast(id + 0x300u, AID.HoneyBFinale, 3f, 5f, "Beat 3 end raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RottenHeart(uint id, float delay)
    {
        Cast(id, AID.RottenHeart, delay, 1f);
        ComponentCondition<RottenHeart>(id + 2u, 3.6f, static comp => comp.NumCasts > 0, "Nisi start raidwide")
            .ActivateOnEnter<RottenHeart>()
            .ActivateOnEnter<RottenHeartBigBurst>() // statuses appear right before raidwide
            .DeactivateOnExit<RottenHeart>()
            .SetHint(StateMachine.StateHint.Raidwide);

        CallMeHoney(id + 0x10u, 11.5f, "Raidwide 1");
        CallMeHoney(id + 0x20u, 12.2f, "Raidwide 2");
        CallMeHoney(id + 0x30u, 12.2f, "Raidwide 3");
        CallMeHoney(id + 0x40u, 12.2f, "Raidwide 4")
            .DeactivateOnExit<RottenHeartBigBurst>();
    }
}
