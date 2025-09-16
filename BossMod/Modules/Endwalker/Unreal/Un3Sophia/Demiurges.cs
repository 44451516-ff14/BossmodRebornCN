namespace BossMod.Endwalker.Unreal.Un3Sophia;

// shows all three demiurges + handles directional parry from first; the reason is to simplify condition checks
class Demiurges(BossModule module) : Components.DirectionalParry(module, [(uint)OID.Demiurge1, (uint)OID.Demiurge2, (uint)OID.Demiurge3])
{

    public bool AddsActive => ActiveActors.Count != 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var sides = spell.Action.ID switch
        {
            (uint)AID.VerticalKenoma => Side.Front | Side.Back,
            (uint)AID.HorizontalKenoma => Side.Left | Side.Right,
            _ => Side.None
        };
        if (sides != Side.None)
            PredictParrySide(caster.InstanceID, sides);
    }
}

class DivineSpark(BossModule module) : Components.CastGaze(module, (uint)AID.DivineSpark);
class GnosticRant(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GnosticRant, new AOEShapeCone(40f, 135f.Degrees()));
class GnosticSpear(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GnosticSpear, new AOEShapeRect(20.75f, 2f));
class RingOfPain(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.RingOfPain, m => m.Enemies((uint)OID.RingOfPain).Where(z => z.EventState != 7), 1.7f);

class Infusion(BossModule module) : Components.GenericWildCharge(module, 5f, (uint)AID.Infusion)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Source = caster;
            foreach (var (slot, player) in Raid.WithSlot(false, true, true))
            {
                PlayerRoles[slot] = player.InstanceID == spell.TargetID ? PlayerRole.Target : PlayerRole.Share;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Source = null;
    }
}
