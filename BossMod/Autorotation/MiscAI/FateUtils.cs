namespace BossMod.Autorotation.MiscAI;

public sealed class FateUtils(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Handin, Collect, Sync, Chocobo }
    public enum Flag { Enabled, Disabled }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("FATE helper", "Utilities for completing FATEs", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000, 1);

        res.Define(Track.Handin).As<Flag>("Hand-in")
            .AddOption(Flag.Enabled, "Automatically hand in FATE items at 10+")
            .AddOption(Flag.Disabled, "Do nothing");

        res.Define(Track.Collect).As<Flag>("Collect")
            .AddOption(Flag.Enabled, "Try to collect FATE items instead of engaging in combat")
            .AddOption(Flag.Disabled, "Do nothing");

        res.Define(Track.Sync).As<AIHints.FateSync>("Sync")
            .AddOption(AIHints.FateSync.None, "Do nothing")
            .AddOption(AIHints.FateSync.Enable, "Always enable level sync if possible")
            .AddOption(AIHints.FateSync.Disable, "Always disable level sync if possible");

        res.Define(Track.Chocobo).As<Flag>("Chocobo")
            .AddOption(Flag.Enabled, "Resummon chocobo if <60s on timer")
            .AddOption(Flag.Disabled, "Do nothing");

        return res;
    }

    public const int TurnInGoldReq = 10;

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Hints.WantFateSync = strategy.Option(Track.Sync).As<AIHints.FateSync>();

        if (!Utils.IsPlayerSyncedToFate(World))
            return;

        if (strategy.Option(Track.Chocobo).As<Flag>() == Flag.Enabled && World.Client.GetInventoryItemQuantity(ActionDefinitions.IDMiscItemGreens.ID) > 0 && World.Client.ActiveCompanion is { TimeLeft: < 60, Stabled: false })
            Hints.ActionsToExecute.Push(ActionDefinitions.IDMiscItemGreens, Player, ActionQueue.Priority.VeryHigh);

        var fateID = World.Client.ActiveFate.ID;
        if (primaryTarget is { FateID: > 0 } target && target.FateID == fateID)
            AddLOSGoalForTarget(target);

        if (strategy.Option(Track.Handin).As<Flag>() != Flag.Enabled)
            return;

        var item = Utils.GetFateItem(fateID);
        if (item == 0)
            return;

        // already turned in enough, fate is ending, do nothing
        if (World.Client.ActiveFate.HandInCount >= TurnInGoldReq && World.Client.ActiveFate.Progress >= 100)
            return;

        // until fate is completed, hand in batches of 10; if other people complete the fate, we stop doing stuff
        if (World.Client.GetInventoryItemQuantity(item) >= TurnInGoldReq)
        {
            Hints.InteractWithTarget = World.Actors.Find(World.Client.ActiveFate.ObjectiveNpc);
            return;
        }

        // otherwise, pick up stuff
        if (strategy.Option(Track.Collect).As<Flag>() == Flag.Enabled && !Player.InCombat)
            Hints.InteractWithTarget = World.Actors.Where(a => a.FateID == fateID && a.IsTargetable && a.Type == ActorType.EventObj).MinBy(Player.DistanceToHitbox);

    }

    private void AddLOSGoalForTarget(Actor target)
    {
        if (Hints.PathfindMapObstacles.Bitmap is not { } bitmap)
            return;

        var rect = Hints.PathfindMapObstacles.Rect;
        var mapCenter = Hints.PathfindMapCenter;
        var targetPos = target.Position;
        var targetRange = (Player.Role is Role.Melee or Role.Tank ? 3.5f : 24.5f) + target.HitboxRadius;

        // blacklist current tile if no LoS
        Hints.GoalZones.Add(p => HasLineOfSight(bitmap, rect, mapCenter, p, targetPos) ? 0 : -100);
        Hints.GoalZones.Add(p =>
        {
            if (!HasLineOfSight(bitmap, rect, mapCenter, p, targetPos))
                return 0;
            return (p - targetPos).LengthSq() <= targetRange * targetRange ? 20 : 8;
        });
    }

    private static bool HasLineOfSight(Bitmap map, Bitmap.Rect rect, WPos mapCenter, WPos from, WPos to)
    {
        if (!TryWorldToBitmapCell(map, rect, mapCenter, from, out var x0, out var y0) || !TryWorldToBitmapCell(map, rect, mapCenter, to, out var x1, out var y1))
            return true; // if mapping fails, don't block movement

        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var err = dx + dy;
        var x = x0;
        var y = y0;

        while (true)
        {
            if ((uint)x < map.Width && (uint)y < map.Height && map[x, y])
                return false;
            if (x == x1 && y == y1)
                return true;
            var e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    private static bool TryWorldToBitmapCell(Bitmap map, Bitmap.Rect rect, WPos mapCenter, WPos pos, out int x, out int y)
    {
        var centerCellX = (rect.Left + rect.Right) * 0.5f;
        var centerCellY = (rect.Top + rect.Bottom) * 0.5f;
        var invRes = 1.0f / map.PixelSize;
        var delta = (pos - mapCenter) * invRes;

        x = (int)MathF.Round(centerCellX + delta.X);
        y = (int)MathF.Round(centerCellY + delta.Z);
        return (uint)x < map.Width && (uint)y < map.Height;
    }
}
