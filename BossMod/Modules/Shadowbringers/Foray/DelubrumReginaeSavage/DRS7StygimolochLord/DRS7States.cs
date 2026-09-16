namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

sealed class DRS7StygimolochLordStates : StateMachineBuilder
{
    public DRS7StygimolochLordStates(BossModule module) : base(module)
    {
        SimplePhase(0u, PhaseBeforeAdds, "Before adds")
            .ActivateOnEnter<RapidBoltsAOE>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
        SimplePhase(1u, PhaseAdds, "Adds")
            .ActivateOnEnter<Border>()
            .OnExit(() => (Module.Arena.Center, Module.Arena.Bounds) = DRS7StygimolochLord.BuildArena())
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.IsTargetable;
        DeathPhase(2u, PhaseAfterAdds)
            .ActivateOnEnter<RapidBoltsAOE>();
    }

    void PhaseBeforeAdds(uint id)
    {
        FoeSplitter(id, 8.2f);
        ViciousSwipe(id + 0x10000u, 8.2f);
        Whack(id + 0x20000u, 2.4f);
        ThousandTonzeSwing(id + 0x30000u, 4.7f);
        // TODO: rapid bolts x2 > repeat?
        SimpleState(id + 0xFF0000u, 100f, "???");
    }

    void PhaseAdds(uint id)
    {
        MemoryOfTheLabyrinth(id, 2u); // note: large variance
        LabyrinthineFateFatefulWords(id + 0x10000u, 24.4f);
        DevastatingBolt(id + 0x20000u, 2.6f);
        RendingBolt(id + 0x30000u, 2.8f);
        LabyrinthineFateDevastatingBoltRendingBoltFatefulWords(id + 0x40000u, 7.3f);
        RendingBoltDevastatingBolt(id + 0x50000u, 4.6f);
        LabyrinthineFateDevastatingBoltRendingBoltFatefulWords(id + 0x60000u, 7.3f);
        RendingBoltDevastatingBolt(id + 0x70000u, 4.6f);
        LabyrinthineFateDevastatingBoltRendingBoltFatefulWords(id + 0x80000u, 7.3f);
        RendingBoltDevastatingBolt(id + 0x90000u, 4.6f);
        SimpleState(id + 0xFF0000u, 100f, "???");
    }

    void PhaseAfterAdds(uint id)
    {
        ThunderousDischarge(id, 8.1f);
        RapidBolts(id + 0x10000u, 9.4f);
        ThousandTonzeSwing(id + 0x20000u, 6.1f);
        RapidBolts(id + 0x30000u, 6.1f);
        RapidBolts(id + 0x40000u, 2.1f);
        CrushingHoof(id + 0x50000u, 2.1f);
        Whack(id + 0x60000u, 2.3f);
        FoeSplitter(id + 0x70000u, 11.8f);
        ViciousSwipe(id + 0x80000u, 5.2f);
        Whack(id + 0x90000u, 2.3f);
        // TODO: repeat?
        SimpleState(id + 0xFF0000u, 100f, "???");
    }

    private void FoeSplitter(uint id, float delay)
    {
        Cast(id, AID.FoeSplitter, delay, 5f, "Tankbuster")
            .ActivateOnEnter<FoeSplitter>()
            .DeactivateOnExit<FoeSplitter>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void ViciousSwipe(uint id, float delay)
    {
        ComponentCondition<ViciousSwipe>(id, delay, static comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<ViciousSwipe>()
            .DeactivateOnExit<ViciousSwipe>();
    }

    private void ThunderousDischarge(uint id, float delay)
    {
        Cast(id, AID.ThunderousDischarge, delay, 5f);
        ComponentCondition<ThunderousDischarge>(id + 2u, 0.7f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<ThunderousDischarge>()
            .DeactivateOnExit<ThunderousDischarge>();
    }

    private void RapidBolts(uint id, float delay)
    {
        CastStart(id, AID.RapidBolts, delay)
            .ActivateOnEnter<RapidBoltsBait>();
        CastEnd(id + 1u, 5f, "Baited puddles")
            .DeactivateOnExit<RapidBoltsBait>();
    }

    private void ThousandTonzeSwing(uint id, float delay)
    {
        Cast(id, AID.ThousandTonzeSwing, delay, 6f, "Center aoe")
            .ActivateOnEnter<ThousandTonzeSwing>()
            .DeactivateOnExit<ThousandTonzeSwing>();
    }

    private void CrushingHoof(uint id, float delay)
    {
        Cast(id, AID.CrushingHoof, delay, 5f)
            .ActivateOnEnter<CrushingHoof>();
        ComponentCondition<CrushingHoof>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Baited proximity")
            .DeactivateOnExit<CrushingHoof>();
    }

    private void Whack(uint id, float delay)
    {
        Cast(id, AID.Whack, delay, 3f, "Cone 1")
            .ActivateOnEnter<Whack>();
        ComponentCondition<Whack>(id + 2u, 2f, static comp => comp.NumCasts >= 2, "Cone 2");
        ComponentCondition<Whack>(id + 3u, 2f, static comp => comp.NumCasts >= 3, "Cone 3")
            .DeactivateOnExit<Whack>();
    }

    private void MemoryOfTheLabyrinth(uint id, float delay)
    {
        Cast(id, AID.MemoryOfTheLabyrinth, delay, 3f);
        Condition(id + 0x10u, 0.9f, () =>
        {
            var monks = Module.Enemies((uint)OID.StygimolochMonk);
            var count = monks.Count;
            for (var i = 0; i < count; ++i)
            {
                if (monks[i].IsTargetable)
                {
                    return true;
                }
            }
            return false;
        }, "Adds appear");
    }

    private State FatefulWords(uint id, float delay)
    {
        Cast(id, AID.FatefulWords, delay, 5f)
            .ActivateOnEnter<FatefulWords>();
        return ComponentCondition<FatefulWords>(id + 2u, 0.5f, static comp => comp.NumCasts > 0, "Knockback/attract")
            .DeactivateOnExit<FatefulWords>();
    }

    private void LabyrinthineFateFatefulWords(uint id, float delay)
    {
        Cast(id, AID.LabyrinthineFate, delay, 3f);
        FatefulWords(id + 0x10u, 3.3f);
    }

    private State DevastatingBolt(uint id, float delay)
    {
        Cast(id, AID.DevastatingBolt, delay, 3f);
        return ComponentCondition<DevastatingBoltInner>(id + 0x10u, 4.5f, static comp => comp.NumCasts > 0, "Alcoves")
            .ActivateOnEnter<DevastatingBoltOuter>()
            .ActivateOnEnter<DevastatingBoltInner>()
            .DeactivateOnExit<DevastatingBoltOuter>()
            .DeactivateOnExit<DevastatingBoltInner>();
    }

    private void RendingBolt(uint id, float delay)
    {
        Cast(id, AID.RendingBolt, delay, 3f, "Puddles first")
            .ActivateOnEnter<Electrocution>();
        ComponentCondition<Electrocution>(id + 0x10u, 4f, static comp => comp.Casters.Count == 0, "Puddles last")
            .DeactivateOnExit<Electrocution>();
    }

    private void LabyrinthineFateDevastatingBoltRendingBoltFatefulWords(uint id, float delay)
    {
        Cast(id, AID.LabyrinthineFate, delay, 3f);
        DevastatingBolt(id + 0x100u, 3.3f);

        Cast(id + 0x200u, AID.RendingBolt, 2.8f, 3f, "Puddles first")
            .ActivateOnEnter<Electrocution>();
        FatefulWords(id + 0x210u, 3.3f)
            .DeactivateOnExit<Electrocution>(); // last puddles resolve ~0.7s into cast
    }

    private void RendingBoltDevastatingBolt(uint id, float delay)
    {
        Cast(id, AID.RendingBolt, delay, 3f, "Puddles first")
            .ActivateOnEnter<Electrocution>();
        DevastatingBolt(id + 0x100u, 1.3f)
            .DeactivateOnExit<Electrocution>(); // last puddles resolve ~0.3s before cast end
    }
}
