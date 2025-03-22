namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE31MetalFoxChaos;

public enum OID : uint
{
    Boss = 0x2DB5, // R=8.0
    MagitekBit = 0x2DB6, // R=1.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 20192, // MagitekBit->location, no cast, ???

    DiffractiveLaser = 20138, // Boss->self, 7.0s cast, range 60 150-degree cone
    RefractedLaser = 20141, // MagitekBit->self, no cast, range 100 width 6 rect
    LaserShowerVisual = 20136, // Boss->self, 3.0s cast, single-target
    LaserShower = 20140, // Helper->location, 5.0s cast, range 10 circle
    Rush = 20139, // Boss->player, 3.0s cast, width 14 rect charge
    SatelliteLaser = 20137 // Boss->self, 10.0s cast, range 100 circle
}

class MagitekBitLasers(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime[] _times = [];
    private Angle startrotation;
    public enum Types { None, SatelliteLaser, DiffractiveLaser, LaserShower }
    public Types Type;
    private static readonly AOEShapeRect rect = new(100f, 3f);
    private static readonly Angle a90 = 90f.Degrees(), a180 = 180f.Degrees();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Type == Types.None)
            return [];

        var bits = Module.Enemies((uint)OID.MagitekBit);
        var count = bits.Count;
        var aoes = new AOEInstance[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var p = bits[i];
            var pos = WPos.ClampToGrid(p.Position);
            var rot = p.Rotation;
            var time = WorldState.CurrentTime > _times[0];
            if (Type == Types.SatelliteLaser && time)
            {
                aoes[index++] = new(rect, pos, rot, _times[1]);
            }
            else if (Type == Types.DiffractiveLaser && time || Type == Types.LaserShower)
            {
                if (NumCasts < 5)
                {
                    if (rot.AlmostEqual(startrotation, Angle.DegToRad))
                        aoes[index++] = new(rect, pos, rot, _times[1], Colors.Danger);
                    else if (rot.AlmostEqual(startrotation + a90, Angle.DegToRad) || rot.AlmostEqual(startrotation - a90, Angle.DegToRad))
                        aoes[index++] = new(rect, pos, rot, _times[2]);
                }
                else
                {
                    if (rot.AlmostEqual(startrotation + a180, Angle.DegToRad))
                        aoes[index++] = new(rect, pos, rot, _times[3]);
                    else if (NumCasts < 9 && (rot.AlmostEqual(startrotation + a90, Angle.DegToRad) || rot.AlmostEqual(startrotation - a90, Angle.DegToRad)))
                        aoes[index++] = new(rect, pos, rot, _times[2], Colors.Danger);
                }
            }
        }
        return aoes.AsSpan()[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var _time = WorldState.CurrentTime;
        if (spell.Action.ID == (uint)AID.SatelliteLaser)
        {
            Type = Types.SatelliteLaser;
            _times = [_time.AddSeconds(2.5d), _time.AddSeconds(12.3d)];
        }
        else if (spell.Action.ID == (uint)AID.DiffractiveLaser)
        {
            startrotation = spell.Rotation + 180f.Degrees();
            Type = Types.DiffractiveLaser;
            _times = [_time.AddSeconds(2d), _time.AddSeconds(8.8d), _time.AddSeconds(10.6d), _time.AddSeconds(12.4d)];
        }
        else if (spell.Action.ID == (uint)AID.LaserShower)
        {
            startrotation = caster.Rotation;
            Type = Types.LaserShower;
            _times = [_time, _time.AddSeconds(6.5d), _time.AddSeconds(8.3d), _time.AddSeconds(10.1d)];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RefractedLaser)
        {
            if (++NumCasts == 14)
            {
                NumCasts = 0;
                Type = Types.None;
            }
        }
    }
}

class Rush(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.Rush), 7f);
class LaserShower(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LaserShower), 10f);
class DiffractiveLaser(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DiffractiveLaser), new AOEShapeCone(60f, 75f.Degrees()));
class SatelliteLaser(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SatelliteLaser), "Raidwide + all lasers fire at the same time");

class CE31MetalFoxChaosStates : StateMachineBuilder
{
    public CE31MetalFoxChaosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SatelliteLaser>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<LaserShower>()
            .ActivateOnEnter<MagitekBitLasers>()
            .ActivateOnEnter<Rush>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 735, NameID = 13)] // bnpcname=9424
public class CE31MetalFoxChaos(WorldState ws, Actor primary) : BossModule(ws, primary, new(-234, 262), new ArenaBoundsSquare(30));
