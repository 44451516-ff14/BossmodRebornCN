using ImGuiNET;
using System.Reflection;

namespace BossMod.Stormblood.Foray;

[AttributeUsage(AttributeTargets.Field)]
public sealed class NMAttribute(uint nameID, uint prepNameID, uint fateID = 0) : Attribute
{
    public uint NameID => nameID;
    public uint PrepNameID => prepNameID;
    public uint FateID => fateID;

    public string Label => $"{ModuleViewer.BNpcName(nameID)} ({ModuleViewer.BNpcName(prepNameID)})";
}

[ConfigDisplay(Name = "Eureka", Parent = typeof(StormbloodConfig))]
public class EurekaConfig : ConfigNode
{
    [PropertyDisplay("Max range to look for new mobs to pull")]
    [PropertySlider(20, 100, Speed = 0.1f)]
    public float MaxPullDistance = 30f;

    [PropertyDisplay("Max number of mobs to pull at once (0 for no limit)")]
    [PropertySlider(0, 30, Speed = 0.1f)]
    public int MaxPullCount = 10;

    [PropertyDisplay("Show auto farm window")]
    public bool ShowAutoFarmWindow = false;

    public bool AssistMode;
}

public abstract class EurekaZone<NM> : ZoneModule where NM : struct, Enum
{
    protected static readonly EurekaConfig globalConfig = Service.Config.Get<EurekaConfig>();

    public readonly string Zone;

    private readonly EventSubscriptions _subscriptions;

    private static NMAttribute? GetAttr(Enum nm) => nm.GetType().GetField(nm.ToString())?.GetCustomAttribute<NMAttribute>();
    private static uint GetFateID(Enum nm) => GetAttr(nm)?.FateID ?? 0;
    private static uint GetPrepID(Enum nm) => GetAttr(nm)?.PrepNameID ?? 0;

    protected abstract NM FarmTarget { get; set; }

    protected EurekaZone(WorldState ws, string zone) : base(ws)
    {
        _subscriptions = new(ws.Client.FateInfo.Subscribe(OnFateSpawn));
        Zone = zone;
    }

    private void OnFateSpawn(ClientState.OpFateInfo fate)
    {
        if (GetFateID(FarmTarget) == fate.FateId)
            FarmTarget = default;
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    public override bool WantDrawExtra() => globalConfig.ShowAutoFarmWindow;

    public override string WindowName() => $"{Zone}###Eureka module";

    protected virtual void AddAIHints(int playerSlot, Actor player, AIHints hints) { }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        AddAIHints(playerSlot, player, hints);

        hints.ForbiddenZones.RemoveAll(z => World.Actors.Find(z.Source) is Actor src && ShouldIgnore(src, player));

        var farmNameID = GetPrepID(FarmTarget);
        var farmMax = globalConfig.MaxPullCount;
        var farmRange = globalConfig.MaxPullDistance;

        if (farmMax > 0)
        {
            var count = 0;
            var countTargets = hints.PotentialTargets.Count;
            for (var i = 0; i < countTargets; ++i)
            {
                var e = hints.PotentialTargets[i];
                if (e.Priority >= 0)
                {
                    ++count;
                    if (count >= farmMax)
                        return;
                }
            }
        }
        if (farmNameID == 0)
            return;

        // only need to check "undesirable" targets, as mobs already attacking party members will be handled by autofarm
        static bool canTarget(AIHints.Enemy enemy) => enemy.Priority == AIHints.Enemy.PriorityUndesirable && (!globalConfig.AssistMode || enemy.Actor.InCombat);

        foreach (var e in hints.PotentialTargets.Where(t => t.Actor.NameID == farmNameID))
        {
            if (canTarget(e) && (e.Actor.Position - player.Position).LengthSq() <= farmRange * farmRange)
            {
                // only level 61+ wraiths spawn golde
                if (farmNameID == 8058 && e.Actor.ForayInfo.Level < 61)
                    continue;

                e.Priority = 0;

                var wePull = !e.Actor.InCombat;

                if (wePull && (hints.ForcedTarget == null || (hints.ForcedTarget.Position - player.Position).LengthSq() > (e.Actor.Position - player.Position).LengthSq()))
                    hints.ForcedTarget = e.Actor;
            }
        }
    }

    private bool ShouldIgnore(Actor caster, Actor player)
    {
        return caster.CastInfo != null
            && caster.CastInfo.Action.ID switch
            {
                15415 or 15416 => true,
                15449 or 15295 => caster.CastInfo.TargetID == player.InstanceID,
                _ => false,
            };
    }

    public override void DrawExtra()
    {
        var tar = FarmTarget;
        if (UICombo.Enum("Prep", ref tar, t => GetAttr(t)?.Label ?? t.ToString()))
            FarmTarget = tar;

        ImGui.Spacing();

        var modified = false;

        ImGui.SetNextItemWidth(200);
        modified |= ImGui.DragFloat("Max distance to look for new mobs", ref globalConfig.MaxPullDistance, 1, 20, 80);
        ImGui.SetNextItemWidth(200);
        modified |= ImGui.DragInt("Max mobs to pull (set to 0 for no limit)", ref globalConfig.MaxPullCount, 1, 0, 30);
        modified |= ImGui.Checkbox("Assist mode (only attack mobs that are already in combat)", ref globalConfig.AssistMode);

        if (modified)
            globalConfig.Modified.Fire();
    }
}
