namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB1DemonTablet;

sealed class LacunateStream(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(34f, 15f, 3f);
    private int roateCounter;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.RotateLeft or (uint)AID.RotateRight)
        {
            if (++roateCounter == 2)
            {
                _aoe = [new(rect, spell.LocXZ, spell.Rotation + (id == (uint)AID.RotateLeft ? 1f : -1f) * 90f.Degrees(), Module.CastFinishAt(spell, 4.2d))];
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LacunateStreamRepeat)
        {
            ++NumCasts;
        }
    }
}
