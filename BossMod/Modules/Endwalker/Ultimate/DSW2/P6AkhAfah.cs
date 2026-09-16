namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P6HPCheck(DSW2 module) : BossComponent(module)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (module._NidhoggP6 is Actor nidhogg && module._HraesvelgrP6 is Actor hraesvelgr)
        {
            var diff = (int)(nidhogg.HPMP.CurHP - hraesvelgr.HPMP.CurHP) * 100.0f / nidhogg.HPMP.MaxHP;
            hints.Add($"Nidhogg HP: {(diff > 0 ? "+" : "")}{diff:f1}%");
        }
    }
}

sealed class P6AkhAfah(BossModule module) : Components.UniformStackSpread(module, 4f, default, 4)
{
    public bool Done;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhAfahN)
            AddStacks(Raid.WithoutSlot(true, true, true).Where(p => p.Role == Role.Healer));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhAfahHAOE or (uint)AID.AkhAfahNAOE)
        {
            Stacks.Clear();
            Done = true;
        }
    }
}
