namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

// phase 1 is a fixed timeline (two kills on 2026-10-18 matched within 0.3s), taken from the first earthen fury cast.
// two slots are random: levinbolt or hypernova at 60s, hellfire or judgment bolt at 102s. dark matter is an hp push
// (seen at 266s, 302s and 345s) and ends the phase. everything after ~300s comes from a single partial pull.
// phases 2 and 3 are simple enough to run with everything active and no timeline.
class Un2ShinryuStates : StateMachineBuilder
{
    private float _t = -10.2f; // timeline cursor: when the previous state ends, in seconds from the first earthen fury cast, which comes 10.2s after the pull

    public Un2ShinryuStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "阶段 1：地板机制")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.CastInfo?.IsSpell(AID.DarkMatter) == true;
        DeathPhase(1, id => AllComponents(SimpleState(id, 10000f, "阶段 2 + 3")));
    }

    private void Phase1(uint id)
    {
        AllComponents(BossCast(id, AID.EarthenFuryVisual, 0f, 10f, "大地之怒（全屏攻击，地板开裂）")).SetHint(StateMachine.StateHint.Raidwide);
        At(id + 0x10, 21.0f, "四组连线");
        BossCast(id + 0x20, AID.TidalWaveVisual, 23.2f, 10f, "海潮（击退）").SetHint(StateMachine.StateHint.Knockback);
        At(id + 0x30, 38.2f, "尾部标记");
        WingCast(id + 0x40, OID.LeftWing, AID.SummonIcicle, 46.3f, 4f, "召唤冰柱");
        At(id + 0x50, 56.1f, "尾部拍击 + 冰刺");
        WingCastMulti(id + 0x60, OID.RightWing, [AID.LevinboltVisual, AID.HypernovaVisual], 60.4f, 8f, "雷电（分散）或超新星（集合）");
        WingCast(id + 0x70, OID.LeftWing, AID.IceStormVisual, 77.4f, 6f, "冰风暴（全屏攻击）").SetHint(StateMachine.StateHint.Raidwide);
        At(id + 0x80, 83.5f, "龙拳（中央）");
        BossCast(id + 0x90, AID.AkhMorn, 92.6f, 4f, "阿克·莫恩（坦克三连分摊）").SetHint(StateMachine.StateHint.Tankbuster);
        BossCastMulti(id + 0xA0, [AID.HellfireVisual, AID.JudgmentBoltVisual], 101.9f, 10f, "地狱火（站水中）或审判之雷（离开水面），随后冰刺").SetHint(StateMachine.StateHint.Raidwide);
        At(id + 0xB0, 127.0f, "尾部标记");
        WingCast(id + 0xC0, OID.RightWing, AID.LevinboltVisual, 130.1f, 8f, "雷电（分散）");
        At(id + 0xD0, 145.0f, "尾部拍击");
        WingCast(id + 0xE0, OID.LeftWing, AID.IceStormVisual, 147.2f, 6f, "冰风暴（全屏攻击）").SetHint(StateMachine.StateHint.Raidwide);
        At(id + 0xF0, 154.1f, "两组连线（坦克 + 治疗）");
        BossCast(id + 0x100, AID.EarthBreath, 163.3f, 6f, "大地吐息（两次诱导）");
        BossCast(id + 0x110, AID.AkhMorn, 172.7f, 4f, "阿克·莫恩（坦克三连分摊）").SetHint(StateMachine.StateHint.Tankbuster);
        WingCast(id + 0x120, OID.LeftWing, AID.IceStormVisual, 176.7f, 6f, "冰风暴（全屏攻击）").SetHint(StateMachine.StateHint.Raidwide);
        BossCast(id + 0x130, AID.DiamondDustVisual, 187.0f, 10f, "钻石星尘（全屏攻击、薄冰、治疗连线）").SetHint(StateMachine.StateHint.Raidwide);
        At(id + 0x140, 214.3f, "雷龙出现（治疗雷龙）");
        At(id + 0x150, 222.2f, "尾部标记，龙诗开始");
        At(id + 0x160, 240.1f, "尾部拍击");
        At(id + 0x170, 251.5f, "龙诗结束");
        WingCast(id + 0x180, OID.LeftWing, AID.SummonIcicle, 253.4f, 4f, "召唤冰柱");
        BossCast(id + 0x190, AID.AkhMorn, 257.4f, 4f, "阿克·莫恩（坦克三连分摊）").SetHint(StateMachine.StateHint.Tankbuster);
        BossCast(id + 0x1A0, AID.AerialBlastVisual, 270.7f, 10f, "空爆（击退）+ 超级旋风").SetHint(StateMachine.StateHint.Knockback);
        BossCast(id + 0x1B0, AID.EarthBreath, 301.6f, 6f, "大地吐息（两次诱导）");
        At(id + 0x1C0, 324.6f, "回旋冲锋");
        WingCast(id + 0x1D0, OID.RightWing, AID.HypernovaVisual, 331.6f, 8f, "超新星（集合分摊）");
        SimpleState(id + 0x1E0, 60f, "阿克·莫恩、雷电、海潮、黑暗物质（未验证）");
    }

    private State BossCast(uint id, AID aid, float at, float castTime, string name)
    {
        var s = Cast(id, aid, at - _t, castTime, name);
        _t = at + castTime;
        return s;
    }

    private State BossCastMulti(uint id, AID[] aids, float at, float castTime, string name)
    {
        var s = CastMulti(id, aids, at - _t, castTime, name);
        _t = at + castTime;
        return s;
    }

    private State WingCast(uint id, OID wing, AID aid, float at, float castTime, string name)
    {
        var s = ActorCast(id, () => Module.Enemies((uint)wing).FirstOrDefault(), aid, at - _t, castTime, false, name);
        _t = at + castTime;
        return s;
    }

    private State WingCastMulti(uint id, OID wing, AID[] aids, float at, float castTime, string name)
    {
        var s = ActorCastMulti(id, () => Module.Enemies((uint)wing).FirstOrDefault(), aids, at - _t, castTime, false, name);
        _t = at + castTime;
        return s;
    }

    private State At(uint id, float at, string name)
    {
        var s = Timeout(id, at - _t, name);
        _t = at;
        return s;
    }

    private static State AllComponents(State s) => s
        .ActivateOnEnter<ArenaChanges>()
        .ActivateOnEnter<EarthenFury>()
        .ActivateOnEnter<EarthenFurySmash>()
        .ActivateOnEnter<Hellfire>()
        .ActivateOnEnter<JudgmentBolt>()
        .ActivateOnEnter<DiamondDust>()
        .ActivateOnEnter<IceStorm>()
        .ActivateOnEnter<DarkMatter>()
        .ActivateOnEnter<Protostar>()
        .ActivateOnEnter<TailSpit>()
        .ActivateOnEnter<Enrage>()
        .ActivateOnEnter<TidalWave>()
        .ActivateOnEnter<AerialBlast>()
        .ActivateOnEnter<SuperCyclone>()
        .ActivateOnEnter<Levinbolt>()
        .ActivateOnEnter<LevinboltAOE>()
        .ActivateOnEnter<Hypernova>()
        .ActivateOnEnter<IcicleAdds>()
        .ActivateOnEnter<IcicleImpact>()
        .ActivateOnEnter<Spikesicle>()
        .ActivateOnEnter<SpikesicleKnockback>()
        .ActivateOnEnter<AkhMorn>()
        .ActivateOnEnter<EarthBreath>()
        .ActivateOnEnter<Dragonfist>()
        .ActivateOnEnter<GyreCharge>()
        .ActivateOnEnter<BurningChains>()
        .ActivateOnEnter<TailMarker>()
        .ActivateOnEnter<TailSlap>()
        .ActivateOnEnter<ThinIce>()
        .ActivateOnEnter<Dragonflight>()
        .ActivateOnEnter<Cocoons>()
        .ActivateOnEnter<DragonAdds>()
        .ActivateOnEnter<MeteorImpact>()
        .ActivateOnEnter<MeteorImpact2>()
        .ActivateOnEnter<Touchdown>()
        .ActivateOnEnter<Fireball>()
        .ActivateOnEnter<BlazingTrail>()
        .ActivateOnEnter<SpikedTail>()
        .ActivateOnEnter<DeathSentence>()
        .ActivateOnEnter<CocoonSpread>()
        .ActivateOnEnter<TeraSlash>()
        .ActivateOnEnter<AtomicRay>()
        .ActivateOnEnter<AetherialRay>()
        .ActivateOnEnter<Wormwail>()
        .ActivateOnEnter<BenightingBreath>();
}
