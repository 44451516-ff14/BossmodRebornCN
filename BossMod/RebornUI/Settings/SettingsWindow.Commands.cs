using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;

namespace BossMod.RebornUI;

internal sealed partial class SettingsWindow
{
    private static readonly (string, string)[] _availableAICommands =
    [
        ( "on", "启用 AI。" ),
        ( "off", "停用 AI。" ),
        ( "toggle", "切换 AI 的启用状态。" ),
        ( "targetmaster", "切换是否将跟随目标设为队长。" ),
        ( "follow slotX", "跟随指定队伍位置的成员，例如 Slot1。" ),
        ( "follow name", "按名称跟随指定队员。" ),
        ( "ui", "显示或隐藏 AI 界面。" ),
        ( "forbidactions", "切换是否禁止执行技能（仅适用于自动循环）。" ),
        ( "forbidactions on/off", "开启或关闭技能禁用（仅适用于自动循环）。" ),
        ( "forbidmovement", "切换是否禁止移动。" ),
        ( "forbidmovement on/off", "开启或关闭移动禁用。" ),
        ( "idlewhilemounted", "切换骑乘时是否停止行动。" ),
        ( "idlewhilemounted on/off", "开启或关闭骑乘时停止行动。" ),
        ( "followcombat", "切换战斗中是否继续跟随。" ),
        ( "followcombat on/off", "开启或关闭战斗中的跟随。" ),
        ( "followmodule", "切换激活战斗模块时是否跟随。" ),
        ( "followmodule on/off", "开启或关闭激活战斗模块时的跟随。" ),
        ( "followoutofcombat", "切换非战斗状态下是否跟随。" ),
        ( "followoutofcombat on/off", "开启或关闭非战斗状态下的跟随。" ),
        ( "followtarget", "切换战斗中是否跟随当前目标。" ),
        ( "followtarget on/off", "开启或关闭战斗中跟随当前目标。" ),
        ( "positional X", "跟随目标时切换至指定身位。（any、rear、flank、front）" ),
        ( "maxdistancetarget X", "设置与目标的最大距离。（默认值：2.6）" ),
        ( "maxdistanceslot X", "设置与队伍成员的最大距离。（默认值：1）" ),
        ( "mindistance X", "设置与目标判定框的最小距离。（默认值：0）" ),
        ( "prefdistance X", "设置与危险区域保持的期望距离。（默认值：0）" ),
        ( "movedelay X", "设置 AI 移动决策的延迟。（默认值：0）" ),
        ( "obstaclemaps", "切换是否加载障碍物地图。" ),
        ( "obstaclemaps on/off", "开启或关闭障碍物地图加载。" ),
        ( "setpresetname X", "设置 AI 使用的自动循环预设，例如 setpresetname vbm default。" )
    ];

    private static readonly (string, string)[] _autorotationCommands =
    [
        ( "ar clear", "清除当前预设；除非有计划正在生效，否则自动循环不会执行任何操作。" ),
        ( "ar disable", "强制停用自动循环；即使计划正在生效，也不会自动执行任何技能。" ),
        ( "ar set Preset", "开始执行指定预设。" ),
        ( "ar toggle", "若自动循环未停用则强制停用；否则清除覆盖设置。" ),
        ( "ar toggle Preset", "若指定预设尚未生效则开始执行；否则清除预设。" ),
        ( "ar ui", "显示或隐藏自动循环界面。" ),
    ];

    private static readonly (string, string)[] _availableOtherCommands =
    [
        ( "", "打开此窗口。" ),
        ( "restorerotation", "切换使用技能后是否恢复角色朝向。" ),
        ( "resetcolors", "将所有颜色恢复为默认值。" ),
        ( "d", "打开调试菜单。" ),
        ( "r", "打开回放菜单。" ),
        ( "r on/off", "开始或停止录制回放。" ),
        ( "gc", "触发垃圾回收。" ),
        ( "radar", "切换雷达显示状态。" ),
        ( "radar on/off", "开启或关闭雷达显示。" ),
        ( "radar reset", "将雷达窗口移回屏幕中央。" ),
        ( "cfg", "列出所有配置；使用 /bmr cfg <config> <field> <value> 修改设置。" )
    ];

    private static void DrawCommands()
    {
        DrawCommandCard("cmd_general", "常规", FontAwesomeIcon.Terminal, "/bmr", _availableOtherCommands, 0);
        DrawCommandCard("cmd_ai", "AI", FontAwesomeIcon.Robot, "/bmrai", _availableAICommands, 1);
        DrawCommandCard("cmd_ar", "自动循环", FontAwesomeIcon.Bolt, "/bmr", _autorotationCommands, 2);
    }

    private static void DrawCommandCard(string id, string title, FontAwesomeIcon icon, string prefix, (string, string)[] commands, int index)
    {
        using var card = M3Card.Begin(id, title, icon, M3.AccentFor(index), subtitle: $"命令前缀：{prefix}");
        using var scope = ImRaii.PushId(id);
        var button = M3Widgets.IconButtonSize;
        foreach (var (args, description) in commands)
        {
            var command = args.Length > 0 ? $"{prefix} {args}" : prefix;
            var row = M3SettingRow.Begin(command, description, new(button, button), M3.Scheme.Primary);
            ImGui.SetCursorScreenPos(row.ControlPosition);
            _ = M3Widgets.CopyIconButton($"##{command}", command, snackbar: $"已复制 {command}");
            M3SettingRow.End(row);
        }
    }
}
