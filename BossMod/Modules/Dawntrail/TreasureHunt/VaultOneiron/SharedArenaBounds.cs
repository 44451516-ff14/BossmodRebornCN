namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron;

public abstract class SharedBoundsBoss(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(19.5f));