namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

// TODO: improve hints (check player's class; for healers, hints for party members having incorrect buff)
sealed class SpellforgeSteelstingHint(BossModule module) : BossComponent(module)
{
    private string _hint = "";
    public bool Active => _hint.Length > 0;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Active)
            hints.Add(_hint);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var hint = tether.ID switch
        {
            (uint)TetherID.PhysicalVulnerabilityDown => "Spellforge",
            (uint)TetherID.MagicVulnerabilityDown => "Steelsting",
            _ => ""
        };
        if (hint.Length > 0)
            _hint = hint;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.PhysicalVulnerabilityDown or (uint)SID.MagicVulnerabilityDown)
            _hint = "";
    }
}
