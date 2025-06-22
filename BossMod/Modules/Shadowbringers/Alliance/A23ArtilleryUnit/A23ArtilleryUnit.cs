namespace BossMod.Shadowbringers.Alliance.A23ArtilleryUnit;

class ManeuverVoltArray(BossModule module) : Components.RaidwideCast(module, (uint)AID.ManeuverVoltArray);

class LowerLaser1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LowerLaser1, new AOEShapeCone(30f, 30f.Degrees()));

class EnergyBombardment2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EnergyBombardment2, 4);
class UnknownWeaponskill(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnknownWeaponskill, 8);
class ManeuverRevolvingLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManeuverRevolvingLaser, new AOEShapeDonut(4f, 60f));

class R010Laser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.R010Laser, new AOEShapeRect(60f, 6f));
class R030Hammer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.R030Hammer, 18f);

class UpperLaser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(9);
    private static readonly AOEShapeDonutSector _shapeUpperLaser1 = new(6f, 16f, 30f.Degrees());
    private static readonly AOEShapeDonutSector _shapeUpperLaser2 = new(16f, 23f, 30f.Degrees());
    private static readonly AOEShapeDonutSector _shapeUpperLaser3 = new(23f, 30f, 30f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 6 ? 6 : count;
        var aoes = new AOEInstance[max];

        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i < 3)
                aoes[i] = count > 3 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.UpperLaser1 => _shapeUpperLaser1,
            (uint)AID.UpperLaser2 => _shapeUpperLaser2,
            (uint)AID.UpperLaser3 => _shapeUpperLaser3,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count == 9)
                _aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.UpperLaser2 or (uint)AID.UpperLaser2 or (uint)AID.UpperLaser3)
            _aoes.RemoveAt(0);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9650)]
public class A23ArtilleryUnit(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    public static readonly ArenaBoundsComplex DefaultBounds = new([new Circle(new(200f, -100f), 29.5f)], [new Circle(new(200f, -100f), 6)]);
}
