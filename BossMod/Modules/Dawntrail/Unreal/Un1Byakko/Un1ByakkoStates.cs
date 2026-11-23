namespace BossMod.Dawntrail.Unreal.Un1Byakko;

sealed class Un1ByakkoStates : StateMachineBuilder
{
    private readonly Un1Byakko _module;

    public Un1ByakkoStates(Un1Byakko module) : base(module)
    {
        _module = module;
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<UnrelentingAnguish>(); // these orbs linger after mechanic ends
    }

    private void SinglePhase(uint id)
    {
        StormPulse(id, 6.2f);
        HeavenlyStrike(id + 0x10000u, 2.1f);
        StateOfShock(id + 0x20000u, 6.1f);
        UnrelentingAnguish1(id + 0x30000u, 11.1f);
        Hakutei1(id + 0x40000u, 13.4f);
        Intermission(id + 0x50000u, 2.1f);
        HeavenlyStrike(id + 0x60000u, 7.2f);
        HundredfoldHavoc1(id + 0x70000u, 6.2f);
        UnrelentingAnguish2(id + 0x80000u, 11.5f);
        Hakutei2(id + 0x90000u, 8.9f); // note: variance is quite large
        StormPulseDouble(id + 0xA0000u, 11.2f);
        HundredfoldHavoc2(id + 0xB0000u, 8f);
        HeavenlyStrike(id + 0xC0000u, 10.2f);
        StormPulseDouble(id + 0xD0000u, 6.2f);
        DistantClap(id + 0xE0000u, 6.1f);
        HeavenlyStrike(id + 0xF0000u, 2.1f);
        UnrelentingAnguish2(id + 0x100000u, 9.5f);
        StormPulseDouble(id + 0x110000u, 10.5f);
        HundredfoldHavoc1(id + 0x120000u, 6.1f);
        StormPulseQuadruple(id + 0x130000u, 11.5f);
        Cast(id + 0x140000u, AID.StormPulseEnrage, 2.1f, 8f, "狂暴");
    }

    private State StormPulse(uint id, float delay, string name = "全屏攻击")
    {
        return Cast(id, AID.StormPulse, delay, 4f, name)
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StormPulseDouble(uint id, float delay)
    {
        StormPulse(id, delay, "第 1 次全屏攻击");
        ComponentCondition<StormPulseRepeat>(id + 2u, 2.2f, static comp => comp.NumCasts > 0, "第 2 次全屏攻击")
            .ActivateOnEnter<StormPulseRepeat>()
            .DeactivateOnExit<StormPulseRepeat>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StormPulseQuadruple(uint id, float delay)
    {
        StormPulse(id, delay, "第 1 次全屏攻击");
        ComponentCondition<StormPulseRepeat>(id + 2u, 2.2f, static comp => comp.NumCasts > 0, "第 2 次全屏攻击")
            .ActivateOnEnter<StormPulseRepeat>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<StormPulseRepeat>(id + 3u, 2.1f, static comp => comp.NumCasts > 1, "第 3 次全屏攻击")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<StormPulseRepeat>(id + 4u, 2.1f, static comp => comp.NumCasts > 2, "第 4 次全屏攻击")
            .DeactivateOnExit<StormPulseRepeat>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State HeavenlyStrike(uint id, float delay)
    {
        return Cast(id, AID.HeavenlyStrike, delay, 4f, "死刑")
            .ActivateOnEnter<HeavenlyStrike>()
            .DeactivateOnExit<HeavenlyStrike>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void DistantClap(uint id, float delay)
    {
        Cast(id, AID.DistantClap, delay, 5f, "月环")
            .ActivateOnEnter<DistantClap>()
            .DeactivateOnExit<DistantClap>();
    }

    private void StateOfShock(uint id, float delay)
    {
        Cast(id, AID.StateOfShock, delay, 4f);
        ComponentCondition<StateOfShock>(id + 0x10u, 0.9f, static comp => comp.NumStuns > 0, "抓住坦克")
            .ActivateOnEnter<StateOfShock>()
            .ActivateOnEnter<HighestStakes>();
        Cast(id + 0x20u, AID.HighestStakes, 1.5f, 5);
        ComponentCondition<HighestStakes>(id + 0x30u, 0.8f, static comp => comp.NumCasts > 0, "第 1 次踩塔")
            .DeactivateOnExit<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x40u, 2f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x41u, 0.9f, static comp => comp.NumStuns > 0, "抓住坦克");
        Cast(id + 0x50u, AID.HighestStakes, 1.2f, 5);
        ComponentCondition<HighestStakes>(id + 0x60u, 0.8f, static comp => comp.NumCasts > 1, "第 2 次踩塔")
            .DeactivateOnExit<StateOfShock>()
            .DeactivateOnExit<HighestStakes>();
    }

    private void UnrelentingAnguish1(uint id, float delay)
    {
        Cast(id, AID.UnrelentingAnguish, delay, 3f, "光球出现");
        StormPulse(id + 0x10u, 2.2f);
        ComponentCondition<OminousWind>(id + 0x20u, 2.9f, static comp => comp.Targets.Any())
            .ActivateOnEnter<OminousWind>();
        Cast(id + 0x30u, AID.FireAndLightningBoss, 4.5f, 4f, "直线范围")
            .ActivateOnEnter<FireAndLightningBoss>()
            .DeactivateOnExit<FireAndLightningBoss>();
        ComponentCondition<OminousWind>(id + 0x40u, 1.5f, static comp => comp.Targets.None(), "光球结束")
            .DeactivateOnExit<OminousWind>();
    }

    private void Hakutei1(uint id, float delay)
    {
        ActorTargetable(id, _module.Hakutei, true, delay, "白帝出现")
            .ActivateOnEnter<AratamaPuddleBait>(); // icons appear ~2s before tiger
        ComponentCondition<AratamaPuddleBait>(id + 0x10, 3.1f, static comp => comp.NumFinishedSpreads > 0, "开始诱导地面范围")
            .ActivateOnEnter<AratamaPuddleVoidzone>();
        StormPulse(id + 0x20u, 4f)
            .ActivateOnEnter<SteelClaw>()
            .DeactivateOnExit<AratamaPuddleBait>();
        ComponentCondition<SteelClaw>(id + 0x30u, 0.1f, static comp => comp.NumCasts > 0, "第 1 次顺劈");
        CastStart(id + 0x40u, AID.HeavenlyStrike, 6);
        ComponentCondition<SteelClaw>(id + 0x41u, 0.1f, static comp => comp.NumCasts > 1, "第 2 次顺劈")
            .ActivateOnEnter<HeavenlyStrike>()
            .DeactivateOnExit<SteelClaw>();
        CastEnd(id + 0x42u, 3.9f, "死刑")
            .DeactivateOnExit<HeavenlyStrike>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ActorTargetable(id + 0x50u, _module.Hakutei, false, 3.1f, "白帝消失");

        ComponentCondition<WhiteHerald>(id + 0x60u, 0.7f, static comp => comp.Active)
            .ActivateOnEnter<WhiteHerald>();
        CastStart(id + 0x61u, AID.DistantClap, 2.4f);
        ComponentCondition<WhiteHerald>(id + 0x62u, 2.7f, static comp => comp.NumFinishedSpreads > 0, "距离衰减点名")
            .ActivateOnEnter<DistantClap>()
            .DeactivateOnExit<WhiteHerald>();
        ActorTargetable(id + 0x63u, _module.Hakutei, true, 2f, "白帝重新出现");
        CastEnd(id + 0x64u, 0.3f, "月环")
            .DeactivateOnExit<DistantClap>();
        ComponentCondition<FireAndLightningAdd>(id + 0x65u, 3.9f, static comp => comp.NumCasts > 0, "直线范围") // note: pretty large variance here
            .ActivateOnEnter<FireAndLightningAdd>()
            .DeactivateOnExit<FireAndLightningAdd>();
        StormPulse(id + 0x70u, 0.2f)
            .DeactivateOnExit<AratamaPuddleVoidzone>();
    }

    private void Intermission(uint id, float delay)
    {
        ActorTargetable(id, _module.Boss, false, delay, "首领消失");
        ActorCast(id + 0x10u, _module.Hakutei, AID.RoarOfThunder, 4.4f, 20f, true, "小怪狂暴") // note: pretty large variance here
            .ActivateOnEnter<VoiceOfThunder>()
            .DeactivateOnExit<VoiceOfThunder>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart);
        ComponentCondition<Intermission>(id + 0x12u, 5.7f, static comp => comp.Active)
            .ActivateOnEnter<Intermission>()
            .OnExit(() => Module.Arena.Bounds = Un1Byakko.GetIntermissionBounds());
        ComponentCondition<IntermissionSweepTheLeg>(id + 0x20u, 36.5f, static comp => comp.NumCasts > 0, "第 1 次月环")
            .ActivateOnEnter<IntermissionOrbAratama>()
            .ActivateOnEnter<IntermissionSweepTheLeg>();
        ComponentCondition<ImperialGuard>(id + 0x21u, 5.7f, static comp => comp.NumCasts > 0, "第 1 次直线范围")
            .ActivateOnEnter<ImperialGuard>();
        ComponentCondition<ImperialGuard>(id + 0x22u, 12f, static comp => comp.NumCasts > 1, "第 2 次直线范围");
        ComponentCondition<IntermissionSweepTheLeg>(id + 0x23, 13.6f, static comp => comp.NumCasts > 1, "第 2 次月环")
            .DeactivateOnExit<IntermissionOrbAratama>()
            .DeactivateOnExit<IntermissionSweepTheLeg>();
        ComponentCondition<ImperialGuard>(id + 0x24u, 3.4f, static comp => comp.NumCasts > 2, "第 3 次直线范围")
            .DeactivateOnExit<ImperialGuard>();
        ComponentCondition<Intermission>(id + 0x25u, 7.5f, static comp => !comp.Active)
            .DeactivateOnExit<Intermission>()
            .OnExit(() => Module.Arena.Bounds = Un1Byakko.BuildArena().arena);
        ComponentCondition<FellSwoop>(id + 0x26u, 20.2f, static comp => comp.NumCasts > 0, "全屏攻击")
            .ActivateOnEnter<FellSwoop>()
            .DeactivateOnExit<FellSwoop>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x30u, _module.Boss, true, 7.9f, "首领重新出现")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void HundredfoldHavoc1(uint id, float delay)
    {
        ComponentCondition<HundredfoldHavoc>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<HundredfoldHavoc>();
        CastStart(id + 1u, AID.StateOfShock, 2.1f);
        ComponentCondition<HundredfoldHavoc>(id + 2u, 2.9f, static comp => comp.NumCasts > 0, "移动爆炸开始");
        CastEnd(id + 3u, 1.1f);

        ComponentCondition<StateOfShock>(id + 0x10u, 0.9f, static comp => comp.NumStuns > 0, "抓住坦克")
            .ActivateOnEnter<StateOfShock>()
            .ActivateOnEnter<HighestStakes>();
        Cast(id + 0x20u, AID.HighestStakes, 1.5f, 5f)
            .DeactivateOnExit<HundredfoldHavoc>();
        ComponentCondition<HighestStakes>(id + 0x30u, 0.8f, static comp => comp.NumCasts > 0, "第 1 次踩塔")
            .DeactivateOnExit<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x40u, 2f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x41u, 0.9f, static comp => comp.NumStuns > 0, "抓住坦克");
        Cast(id + 0x50u, AID.HighestStakes, 1.3f, 5f);
        ComponentCondition<HighestStakes>(id + 0x60u, 0.8f, static comp => comp.NumCasts > 1, "第 2 次踩塔")
            .DeactivateOnExit<StateOfShock>()
            .DeactivateOnExit<HighestStakes>();

        Cast(id + 0x1000u, AID.SweepTheLegBoss, 1.9f, 4f, "大扇形范围")
            .ActivateOnEnter<SweepTheLegBoss>()
            .DeactivateOnExit<SweepTheLegBoss>();
    }

    private void UnrelentingAnguish2(uint id, float delay)
    {
        Cast(id, AID.UnrelentingAnguish, delay, 3f, "光球出现");
        StormPulseDouble(id + 0x10u, 2.1f);

        ComponentCondition<GaleForce>(id + 0x20u, 2f, static comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<GaleForce>();
        ComponentCondition<OminousWind>(id + 0x21u, 6.9f, static comp => comp.Targets.Any())
            .ActivateOnEnter<VacuumClaw>()
            .ActivateOnEnter<OminousWind>();
        ComponentCondition<GaleForce>(id + 0x22u, 1.2f, static comp => comp.NumCasts > 0, "诱导")
            .DeactivateOnExit<GaleForce>();

        Cast(id + 0x30u, AID.FireAndLightningBoss, 3.4f, 4f, "直线范围")
            .ActivateOnEnter<FireAndLightningBoss>()
            .DeactivateOnExit<FireAndLightningBoss>();
        ComponentCondition<OminousWind>(id + 0x40u, 1.5f, static comp => comp.Targets.None(), "光球结束")
            .DeactivateOnExit<OminousWind>();

        Cast(id + 0x50u, AID.FireAndLightningBoss, 3.4f, 4f, "直线范围")
            .ActivateOnEnter<FireAndLightningBoss>()
            .DeactivateOnExit<FireAndLightningBoss>();
        ComponentCondition<VacuumClaw>(id + 0x60u, 0.7f, static comp => !comp.Active, "地面危险区消失")
            .DeactivateOnExit<VacuumClaw>();
    }

    private void Hakutei2(uint id, float delay)
    {
        ActorTargetable(id, _module.Hakutei, true, delay, "白帝出现")
            .ActivateOnEnter<AratamaPuddleBait>(); // icons appear ~2s before tiger

        ComponentCondition<AratamaPuddleBait>(id + 0x10u, 3.1f, static comp => comp.NumFinishedSpreads > 0, "开始诱导地面范围")
            .ActivateOnEnter<AratamaPuddleVoidzone>();
        ActorTargetable(id + 0x20u, _module.Hakutei, false, 6f, "白帝消失");

        ComponentCondition<WhiteHerald>(id + 0x30u, 0.7f, static comp => comp.Active)
            .ActivateOnEnter<WhiteHerald>();
        CastStart(id + 0x31u, AID.DistantClap, 2.4f);
        ComponentCondition<WhiteHerald>(id + 0x32u, 2.7f, static comp => comp.NumFinishedSpreads > 0, "距离衰减点名")
            .ActivateOnEnter<DistantClap>()
            .DeactivateOnExit<WhiteHerald>();
        ActorTargetable(id + 0x33u, _module.Hakutei, true, 2f, "白帝重新出现");
        CastEnd(id + 0x34u, 0.3f, "第 1 次月环")
            .DeactivateOnExit<DistantClap>();

        CastStart(id + 0x40u, AID.DistantClap, 3.2f)
            .ActivateOnEnter<FireAndLightningAdd>();
        ComponentCondition<FireAndLightningAdd>(id + 0x41u, 0.7f, static comp => comp.NumCasts > 0, "第 1 次直线范围")
            .ActivateOnEnter<DistantClap>()
            .DeactivateOnExit<FireAndLightningAdd>();
        CastEnd(id + 0x42u, 4.3f, "第 2 次月环")
            .DeactivateOnExit<DistantClap>();
        ComponentCondition<FireAndLightningAdd>(id + 0x50u, 4f, static comp => comp.NumCasts > 0, "第 2 次直线范围")
            .ActivateOnEnter<FireAndLightningAdd>()
            .DeactivateOnExit<FireAndLightningAdd>();

        HeavenlyStrike(id + 0x100u, 3.2f)
            .ActivateOnEnter<SteelClaw>()
            .DeactivateOnExit<SteelClaw>();

        ActorCastStart(id + 0x200u, _module.Hakutei, AID.RoarOfThunder, 10.6f, false)
            .ActivateOnEnter<VoiceOfThunder>()
            .DeactivateOnExit<AratamaPuddleVoidzone>();
        StormPulseDouble(id + 0x210u, 5.6f);
        ActorCastEnd(id + 0x220u, _module.Hakutei, 8.2f, false, "小怪狂暴")
            .DeactivateOnExit<VoiceOfThunder>()
            .SetHint(StateMachine.StateHint.Raidwide);

        Cast(id + 0x300u, AID.FireAndLightningBoss, 8.3f, 4f, "直线范围")
            .ActivateOnEnter<FireAndLightningBoss>()
            .DeactivateOnExit<FireAndLightningBoss>();
    }

    private void HundredfoldHavoc2(uint id, float delay)
    {
        ComponentCondition<GaleForce>(id, delay, static comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<GaleForce>();

        ComponentCondition<HundredfoldHavoc>(id + 0x10u, 2.2f, static comp => comp.Active)
            .ActivateOnEnter<HundredfoldHavoc>();
        CastStart(id + 0x11u, AID.StateOfShock, 4.6f);
        ComponentCondition<HundredfoldHavoc>(id + 0x12u, 0.4f, static comp => comp.NumCasts > 0, "移动爆炸开始");
        ComponentCondition<GaleForce>(id + 0x13u, 0.9f, static comp => comp.NumCasts > 0, "诱导")
            .ActivateOnEnter<VacuumClaw>()
            .DeactivateOnExit<GaleForce>();
        CastEnd(id + 0x14u, 2.7f);

        ComponentCondition<StateOfShock>(id + 0x20u, 0.9f, static comp => comp.NumStuns > 0, "抓住坦克")
            .ActivateOnEnter<StateOfShock>()
            .ActivateOnEnter<HighestStakes>();
        Cast(id + 0x30u, AID.HighestStakes, 1.3f, 5f)
            .DeactivateOnExit<HundredfoldHavoc>();
        ComponentCondition<HighestStakes>(id + 0x40u, 0.8f, static comp => comp.NumCasts > 0, "第 1 次踩塔")
            .DeactivateOnExit<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x50u, 2f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<StateOfShock>();
        ComponentCondition<StateOfShock>(id + 0x51u, 0.9f, static comp => comp.NumStuns > 0, "抓住坦克");
        Cast(id + 0x60u, AID.HighestStakes, 1.2f, 5f)
            .DeactivateOnExit<VacuumClaw>();
        ComponentCondition<HighestStakes>(id + 0x70u, 0.8f, static comp => comp.NumCasts > 1, "第 2 次踩塔")
            .DeactivateOnExit<StateOfShock>()
            .DeactivateOnExit<HighestStakes>();

        Cast(id + 0x1000u, AID.SweepTheLegBoss, 1.9f, 4f, "大扇形范围")
            .ActivateOnEnter<SweepTheLegBoss>()
            .DeactivateOnExit<SweepTheLegBoss>();
    }
}
