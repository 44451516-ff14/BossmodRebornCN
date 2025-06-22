namespace BossMod.Heavensward.Extreme.Ex3Thordan;

class HiemalStormSpread(BossModule module) : Components.UniformStackSpread(module, default, 6f, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HiemalStorm)
            AddSpread(actor, WorldState.FutureTime(3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HiemalStormAOE)
            Spreads.Clear();
    }
}

class HiemalStormVoidzone(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.HiemalStorm);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
class SpiralPierce(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50f, 6f), (uint)TetherID.SpiralPierce, (uint)AID.SpiralPierce);
class DimensionalCollapse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DimensionalCollapseAOE, 9f);

class FaithUnmoving(BossModule module) : Components.GenericKnockback(module, (uint)AID.FaithUnmoving, true)
{
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var grinnauxs = Module.Enemies((uint)OID.SerGrinnaux);
        var grinnaux = grinnauxs.Count != 0 ? grinnauxs[0] : null;
        if (grinnaux != default)
            return new Knockback[1] { new(grinnaux.Position, 16f) };
        return [];
    }
}

class CometCircle(BossModule module) : Components.Adds(module, (uint)OID.CometCircle);
class MeteorCircle(BossModule module) : Components.Adds(module, (uint)OID.MeteorCircle);

class HeavyImpact(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly Angle a135 = 135f.Degrees();
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(6.5f, a135), new AOEShapeDonutSector(6.5f, 12.5f, a135), new AOEShapeDonutSector(12.5f, 18.5f, a135), new AOEShapeDonutSector(18.5f, 27.5f, a135)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavyImpactAOE1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell), spell.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.HeavyImpactAOE1 => 0,
            (uint)AID.HeavyImpactAOE2 => 1,
            (uint)AID.HeavyImpactAOE3 => 2,
            (uint)AID.HeavyImpactAOE4 => 3,
            _ => -1
        };
        if (!AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d), caster.Rotation))
            ReportError($"Unexpected ring {order}");
    }
}
