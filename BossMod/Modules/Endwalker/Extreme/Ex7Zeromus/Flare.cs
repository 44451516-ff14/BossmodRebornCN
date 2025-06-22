namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

class FlareTowers(BossModule module) : Components.CastTowers(module, (uint)AID.FlareAOE, 5f, 4, 4);

class FlareScald(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FlareAOE:
                _aoes.Add(new(_shape, caster.Position, default, WorldState.FutureTime(2.1d)));
                break;
            case (uint)AID.FlareScald:
            case (uint)AID.FlareKill:
                ++NumCasts;
                break;
        }
    }
}

class ProminenceSpine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ProminenceSpine, new AOEShapeRect(60f, 5f));
class SparklingBrandingFlare(BossModule module) : Components.CastStackSpread(module, (uint)AID.BrandingFlareAOE, (uint)AID.SparkingFlareAOE, 4f, 4f);

class Nox : Components.StandardChasingAOEs
{
    public Nox(BossModule module) : base(module, new AOEShapeCircle(10f), (uint)AID.NoxAOEFirst, (uint)AID.NoxAOERest, 5.5f, 1.6f, 5)
    {
        ExcludedTargets = Raid.WithSlot(true, true, true).Mask();
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Nox)
            ExcludedTargets[Raid.FindSlot(actor.InstanceID)] = false;
    }
}
