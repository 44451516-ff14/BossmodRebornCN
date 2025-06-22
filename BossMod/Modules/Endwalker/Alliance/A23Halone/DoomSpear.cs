namespace BossMod.Endwalker.Alliance.A23Halone;

class DoomSpear(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DoomSpearAOE1 or (uint)AID.DoomSpearAOE2 or (uint)AID.DoomSpearAOE3)
        {
            Towers.Add(new(spell.LocXZ, 6f, 8, 8));
            if (Towers.Count == 3)
                Towers.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DoomSpearAOE1 or (uint)AID.DoomSpearAOE2 or (uint)AID.DoomSpearAOE3)
        {
            Towers.RemoveAt(0);
            ++NumCasts;
        }
    }
}
