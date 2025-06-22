namespace BossMod.Stormblood.Ultimate.UWU;

class P1Slipstream(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Slipstream, new AOEShapeCone(11.7f, 45f.Degrees()));
class P1Downburst(BossModule module) : Components.Cleave(module, (uint)AID.Downburst, new AOEShapeCone(11.7f, 45f.Degrees()));
class P1EyeOfTheStorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EyeOfTheStorm, new AOEShapeDonut(12f, 25f));
class P1Gigastorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gigastorm, 6.5f);
class P2RadiantPlume(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RadiantPlumeAOE, 8f);
class P2Incinerate(BossModule module) : Components.Cleave(module, (uint)AID.Incinerate, new AOEShapeCone(15f, 60f.Degrees()), [(uint)OID.Ifrit]);
class P3RockBuster(BossModule module) : Components.Cleave(module, (uint)AID.RockBuster, new AOEShapeCone(10.55f, 60f.Degrees()), [(uint)OID.Titan]); // TODO: verify angle
class P3MountainBuster(BossModule module) : Components.Cleave(module, (uint)AID.MountainBuster, new AOEShapeCone(15.55f, 45f.Degrees()), [(uint)OID.Titan]); // TODO: verify angle
class P3WeightOfTheLand(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WeightOfTheLandAOE, 6f);
class P3Upheaval(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Upheaval, 24f, true);
class P3Tumult(BossModule module) : Components.CastCounter(module, (uint)AID.Tumult);
class P4Blight(BossModule module) : Components.CastCounter(module, (uint)AID.Blight);
class P4HomingLasers(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HomingLasers, 4f);
class P4DiffractiveLaser(BossModule module) : Components.Cleave(module, (uint)AID.DiffractiveLaser, new AOEShapeCone(18f, 45f.Degrees()), [(uint)OID.UltimaWeapon]); // TODO: verify angle
class P5MistralSongCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MistralSongCone, new AOEShapeCone(21.7f, 75f.Degrees()));

class P5AetherochemicalLaser(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AetherochemicalLaserCenter, (uint)AID.AetherochemicalLaserRight, (uint)AID.AetherochemicalLaserLeft], new AOEShapeRect(46f, 4f));

class P5LightPillar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightPillarAOE, 3); // TODO: consider showing circle around baiter
class P5AethericBoom(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AethericBoom, 10);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Garuda, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 539, PlanLevel = 70)]
public class UWU : BossModule
{
    private readonly List<Actor> _titan;
    private readonly List<Actor> _lahabrea;
    private readonly List<Actor> _ultima;
    private Actor? _mainIfrit;

    public List<Actor> Ifrits;

    public Actor? Garuda() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Ifrit() => _mainIfrit;
    public Actor? Titan() => _titan.Count != 0 ? _titan[0] : null;
    public Actor? Lahabrea() => _lahabrea.Count != 0 ? _lahabrea[0] : null;
    public Actor? Ultima() => _ultima.Count != 0 ? _ultima[0] : null;

    public UWU(WorldState ws, Actor primary) : base(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
    {
        Ifrits = Enemies((uint)OID.Ifrit);
        _titan = Enemies((uint)OID.Titan);
        _lahabrea = Enemies((uint)OID.Lahabrea);
        _ultima = Enemies((uint)OID.UltimaWeapon);
    }

    public override bool ShouldPrioritizeAllEnemies => true;

    protected override void UpdateModule()
    {
        if (_mainIfrit == null && StateMachine.ActivePhaseIndex == 1)
        {
            var count = Ifrits.Count;
            for (var i = 0; i < count; ++i)
            {
                var ifrit = Ifrits[i];
                if (ifrit.IsTargetable)
                {
                    _mainIfrit = ifrit;
                    return;
                }
            }
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Garuda());
        Arena.Actor(Ifrit());
        Arena.Actor(Titan());
        Arena.Actor(Lahabrea());
        Arena.Actor(Ultima());
    }
}
