namespace BossMod.Components;

// component that forces the AI to stay in bounds or return inside (for example after a failed knockback)
public class StayInBounds(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Arena.InBounds(actor.Position))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 3));
    }
}
