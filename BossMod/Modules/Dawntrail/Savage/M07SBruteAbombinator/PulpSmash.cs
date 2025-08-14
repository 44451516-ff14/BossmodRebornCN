namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

sealed class PulpSmash(BossModule module) : Components.StackWithIcon(module, (uint)IconID.PulpSmash, (uint)AID.PulpSmash, 6f, 5.1f, 8, 8);
sealed class ItCameFromTheDirt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ItCameFromTheDirt, 6f);

sealed class TheUnpotted(BossModule module) : Components.GenericBaitAway(module, (uint)AID.TheUnpotted)
{
    private static readonly AOEShapeCone cone = new(60f, 15f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ItCameFromTheDirt)
        {
            var act = Module.CastFinishAt(spell, 0.1d);
            var party = Raid.WithoutSlot(false, true, true);
            var source = Module.PrimaryActor;
            var len = party.Length;

            for (var i = 0; i < len; ++i)
            {
                CurrentBaits.Add(new(source, party[i], cone, act));
            }
        }
    }
}
