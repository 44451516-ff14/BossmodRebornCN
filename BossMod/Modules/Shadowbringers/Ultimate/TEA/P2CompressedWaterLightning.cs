namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P2CompressedWaterLightning(BossModule module) : Components.GenericStackSpread(module)
{
    public bool ResolveImminent; // we want to show hints shortly before next resolve
    private BitMask _forbiddenWater;
    private BitMask _forbiddenLighting;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ResolveImminent)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ResolveImminent)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => ResolveImminent ? base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor) : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (ResolveImminent)
            base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.CompressedWater:
                Stacks.Add(new(actor, 8f, 3, 6, status.ExpireAt, _forbiddenWater));
                _forbiddenWater.Reset();
                break;
            case (uint)SID.CompressedLightning:
                Stacks.Add(new(actor, 8f, 2, 2, status.ExpireAt, _forbiddenLighting));
                _forbiddenLighting.Reset();
                break;
        }
    }

    // note: typical sequence is 'compressed' status loss > 'crashing' spells > 'resistance down' status gain > 'compressed' status gain
    // because of that, cast is the best point to remove previous stacks
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CrashingWave:
                Stacks.RemoveAll(s => CheckAndRecord(s, 6, ref _forbiddenWater));
                ResolveImminent = false; // auto disable on resolve
                break;
            case (uint)AID.CrashingThunder:
                Stacks.RemoveAll(s => CheckAndRecord(s, 2, ref _forbiddenLighting));
                ResolveImminent = false; // auto disable on resolve
                break;
        }
    }

    private bool CheckAndRecord(Stack s, int maxSize, ref BitMask mask)
    {
        if (s.MaxSize != maxSize)
            return false;
        mask.Set(Raid.FindSlot(s.Target.InstanceID));
        return true;
    }
}
