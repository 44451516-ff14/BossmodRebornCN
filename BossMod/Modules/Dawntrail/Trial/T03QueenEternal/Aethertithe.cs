namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class Aethertithe(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(100, 35.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x00)
            return;
        if (state == 0x04000100)
            AddAOE(-55);
        else if (state == 0x08000100)
            AddAOE(default);
        else if (state == 0x10000100)
            AddAOE(55);
    }

    private void AddAOE(float angle)
    {
        _aoe = new(cone, Module.PrimaryActor.Position, angle.Degrees(), WorldState.FutureTime(5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Aethertithe1 or AID.Aethertithe2 or AID.Aethertithe3)
            _aoe = null;
    }
}
