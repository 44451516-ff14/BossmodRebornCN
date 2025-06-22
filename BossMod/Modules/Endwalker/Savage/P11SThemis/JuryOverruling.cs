namespace BossMod.Endwalker.Savage.P11SThemis;

class JuryOverrulingProtean(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, new AOEShapeRect(50f, 4f))
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.JuryOverrulingProteanLight or (uint)AID.JuryOverrulingProteanDark)
            ++NumCasts;
    }
}

class IllusoryGlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IllusoryGlare, 5f);
class IllusoryGloom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IllusoryGloom, new AOEShapeDonut(2f, 9f));
