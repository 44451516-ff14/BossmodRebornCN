namespace BossMod.Components;

// generic protean mechanic is a bunch of aoes baited in some manner by players that have to hit that player only
// TODO: combine with BaitAway
public abstract class GenericProtean(BossModule module, uint aid, AOEShape shape, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false) : CastCounter(module, aid)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    public readonly AOEShape Shape = shape;

    public abstract IEnumerable<(Actor source, Actor target)> ActiveAOEs();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        foreach (var (source, target) in ActiveAOEs())
        {
            if (target != actor && IsPlayerClipped(source, target, actor))
            {
                hints.Add("GTFO from protean!");
                break;
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        Actor? playerProteanSource = null;
        if (player != pc && ArenaProjectionLayerApplies(pc, ArenaProjectionLayer, RestrictToArenaProjectionLayer) && ArenaProjectionLayerParticipantApplies(player, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            foreach (var (source, target) in ActiveAOEs())
            {
                if (target == player)
                {
                    playerProteanSource = source;
                    break;
                }
            }
        }

        return playerProteanSource == null ? PlayerPriority.Irrelevant : IsPlayerClipped(playerProteanSource, player, pc) ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        using var projection = Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer);
        // draw own protean (if any) and clipping proteans (if any)
        foreach (var (source, target) in ActiveAOEs())
        {
            if (ArenaProjectionLayerParticipantApplies(target, ArenaProjectionLayer, RestrictToArenaProjectionLayer) && (target == pc || IsPlayerClipped(source, target, pc)))
            {
                Shape.Draw(Arena, source.Position, Angle.FromDirection(target.Position - source.Position));
            }
        }
    }

    public bool IsPlayerClipped(Actor source, Actor target, Actor player) => ArenaProjectionLayerParticipantApplies(target, ArenaProjectionLayer, RestrictToArenaProjectionLayer)
        && ArenaProjectionLayerParticipantApplies(player, ArenaProjectionLayer, RestrictToArenaProjectionLayer) && Shape.Check(player.Position, source.Position, Angle.FromDirection(target.Position - source.Position));
}

// typical protean will originate from primary actor and hit all alive players
public class SimpleProtean(BossModule module, uint aid, AOEShape shape, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false) : GenericProtean(module, aid, shape, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    public override IEnumerable<(Actor source, Actor target)> ActiveAOEs()
    {
        foreach (var p in Raid.WithoutSlot())
        {
            if (ArenaProjectionLayerParticipantApplies(p, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                yield return (Module.PrimaryActor, p);
            }
        }
    }
}
