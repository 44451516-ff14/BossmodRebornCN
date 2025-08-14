namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P4MornAfah(BossModule module) : Components.UniformStackSpread(module, 4f, default, 8)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MornAfahUsurper)
        {
            var target = WorldState.Actors.Find(caster.TargetID);
            if (target != null)
                AddStack(target, Module.CastFinishAt(spell, 0.9f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MornAfahAOE)
            Stacks.Clear();
    }
}

sealed class P4MornAfahHPCheck(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var usurpers = Module.Enemies((uint)OID.UsurperOfFrostP4);
        var oracles = Module.Enemies((uint)OID.OracleOfDarknessP4);
        var usurper = usurpers.Count != 0 ? usurpers[0] : null;
        var oracle = oracles.Count != 0 ? oracles[0] : null;
        if (usurper != null && oracle != null)
        {
            var diff = (int)(usurper.HPMP.CurHP - oracle.HPMP.CurHP) * 100.0f / usurper.HPMP.MaxHP;
            hints.Add($"Usurper HP: {(diff > 0 ? "+" : "")}{diff:f1}%");
        }
    }
}
