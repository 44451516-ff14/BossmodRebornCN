namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class RuinfallTower(BossModule module) : Components.GenericTowers(module, (uint)AID.RuinfallTower)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var party = Module.Raid.WithSlot(true, true, true);
            var len = party.Length;
            BitMask nontanks = default;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (p.Item2.Role != Role.Tank)
                {
                    nontanks[p.Item1] = true;
                }
            }
            Towers.Add(new(spell.LocXZ, 6f, 2, 2, nontanks, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Towers.Clear();
    }
}

sealed class RuinfallKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RuinfallKnockback, 25f, kind: Kind.DirForward);
sealed class RuinfallAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RuinfallAOE, 6f);
