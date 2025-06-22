namespace BossMod.Heavensward.Alliance.A12IrminsulSawtooth;

class WhiteBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhiteBreath, new AOEShapeCone(28f, 60f.Degrees()));
class MeanThrash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeanThrash, new AOEShapeCone(12f, 60f.Degrees()));
class MeanThrashKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.MeanThrash, 10f, stopAtWall: true);
class MucusBomb(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MucusBomb, 10f);
class MucusSpray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MucusSpray2, new AOEShapeDonut(6f, 20f));
class Rootstorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.Rootstorm);
class Ambush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ambush, 9f);
class AmbushKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Ambush, 30f, stopAtWall: true, kind: Kind.TowardsOrigin);

class ShockwaveStomp(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.ShockwaveStomp, 70f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var boulders = Module.Enemies((uint)OID.Irminsul);
        var count = boulders.Count;
        if (count == 0)
            return [];
        var actors = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var b = boulders[i];
            if (!b.IsDead)
                actors.Add(b);
        }
        return CollectionsMarshal.AsSpan(actors);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Irminsul, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 120, NameID = 4623)]
public class A12IrminsulSawtooth(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 130), arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Donut(new(default, 130f), 8, 35)]);
    private Actor? _sawtooth;

    public Actor? Irminsul() => PrimaryActor;
    public Actor? Sawtooth() => _sawtooth;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _sawtooth ??= StateMachine.ActivePhaseIndex == 0 ? Enemies((uint)OID.Sawtooth)[0] : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_sawtooth);
        Arena.Actors(Enemies((uint)OID.ArkKed));
    }
}
