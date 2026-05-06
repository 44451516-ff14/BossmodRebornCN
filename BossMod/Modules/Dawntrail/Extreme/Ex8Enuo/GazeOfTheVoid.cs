using BossMod.Shadowbringers.Dungeon.D05MtGulg.D055ForgivenObscenity;

namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

sealed class GazeOfTheVoidAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GazeOfTheVoid2, new AOEShapeCone(40f, 22.5f.Degrees()), 7); // This is the easy part!

sealed class GazeOfTheVoidSoaks(BossModule module) : BossComponent(module)
{
    //TODO: We could probably filter these by distance to the boss?
    public static List<Actor> GetSmallOrbs(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.VoidSoakSmall);
        var count = orbs.Count;
        return count == 0 ? [] : orbs;
    }
    public static List<Actor> GetBigOrbs(BossModule module)
    {
        var orbs = module.Enemies((uint)OID.VoidSoakLarge);
        var count = orbs.Count;
        return count == 0 ? [] : orbs;
    }
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (GetSmallOrbs(Module).Count != 0 || GetBigOrbs(Module).Count != 0)
            hints.Add("Soak the orbs in pairs!");
    }

    //TODO: These AI hints are pretty basic -- They'll point us in the correct direction but we're not waiting for pairs or prioritizing the closest to the boss.
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var orbs = GetSmallOrbs(Module);
        var count = orbs.Count;
        var bigorbs = GetBigOrbs(Module);
        var bigcount = bigorbs.Count;
        if (actor.Role != Role.Tank)
        {
            if (count != 0)
            {
                var orbz = new ShapeDistance[count];
                hints.ActionsToExecute.Push(ActionDefinitions.IDSprint, actor, ActionQueue.Priority.High);
                for (var i = 0; i < count; ++i)
                {
                    var o = orbs[i];
                    orbz[i] = new SDInvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(default, 1f), 0.5f, 0.5f, 0.5f);
                }
                hints.AddForbiddenZone(new SDIntersection(orbz), DateTime.MaxValue);
            }
        }
        if (actor.Role == Role.Tank)
        {
            if (bigcount != 0)
            {
                var orbz = new ShapeDistance[bigcount];
                hints.ActionsToExecute.Push(ActionDefinitions.IDSprint, actor, ActionQueue.Priority.High);
                for (var i = 0; i < bigcount; ++i)
                {
                    var o = bigorbs[i];
                    orbz[i] = new SDInvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(default, 1f), 0.5f, 0.5f, 0.5f);
                }
                hints.AddForbiddenZone(new SDIntersection(orbz), DateTime.MaxValue);
            }
        }
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var orbs = GetSmallOrbs(Module);
        var count = orbs.Count;
        var bigorbs = GetBigOrbs(Module);
        var bigcount = bigorbs.Count;
        if (pc.Role != Role.Tank)
        {

            for (var i = 0; i < count; ++i)
            {
                Arena.AddCircle(orbs[i].Position, 1f, Colors.Safe);
            }
            for (var i = 0; i < bigcount; ++i)
            {
                Arena.AddCircle(bigorbs[i].Position, 1f, Colors.Danger);
            }
        }
        if (pc.Role == Role.Tank)
        {
            for (var i = 0; i < bigcount; ++i)
            {
                Arena.AddCircle(bigorbs[i].Position, 1f, Colors.Safe);
            }
            for (var i = 0; i < count; ++i)
            {
                Arena.AddCircle(orbs[i].Position, 1f, Colors.Danger);
            }
        }
    }
}
