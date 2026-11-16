using BossMod.AI;
using BossMod.Autorotation;
using BossMod.ClassShared;
using BossMod.Pathfinding;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using RebornMaterial;

namespace BossMod.RebornUI;

internal static class SettingsLayout
{
    public static readonly Action<ConfigNode, UITree, WorldState> NoCustom = static (_, _, _) => { };

    public sealed record class Node
    {
        public string? Name { get; init; }
        public FontAwesomeIcon Icon { get; init; }
        public string? Description { get; init; }
        public string[] Tags { get; init; } = [];
        public (M3Severity Severity, FontAwesomeIcon Icon, string Text)? Notice { get; init; }
        public Section[]? Sections { get; init; }
        public Dictionary<string, Field> Fields { get; init; } = [];
        public Action<ConfigNode, UITree, WorldState>? Custom { get; init; }
        public Action<ConfigNode, string, UITree, WorldState>? SectionExtras { get; init; }
    }

    public sealed record class Section(string Label, FontAwesomeIcon Icon, string[] Fields, string? StripPrefix = null);

    public sealed record class Field(string? Label = null, string? Tooltip = null, string? Depends = null, string? Format = null, string[]? Options = null, float? Min = null, float? Max = null,
        M3ColorChoice[]? Choices = null);

    private static readonly string[] _maturityOptions = ["占位（仅供开发）", "开发中", "已贡献", "已验证", "AI 支持"];

    private static readonly M3ColorChoice[] _accentChoices =
    [
        new("Reborn 青绿（默认）", M3ColorMath.FromRgb(0x5BA7B7)),
        new("蓝宝石蓝", M3ColorMath.FromRgb(0x2F6DB5)),
        new("紫罗兰", M3ColorMath.FromRgb(0x7E57C2)),
        new("玫红", M3ColorMath.FromRgb(0xC2185B)),
        new("深红", M3ColorMath.FromRgb(0xC62828)),
        new("琥珀", M3ColorMath.FromRgb(0xC98A00)),
        new("森林绿", M3ColorMath.FromRgb(0x3E8E41)),
    ];

    public static Node? For(Type type) => _nodes.GetValueOrDefault(type);

    private static readonly Dictionary<Type, Node> _nodes = new()
    {
        [typeof(InterfaceConfig)] = new()
        {
            Icon = FontAwesomeIcon.PaintBrush,
            Description = "调整 BossMod Reborn 窗口的强调色、尺寸和动画",
            Sections =
            [
                new("主题", FontAwesomeIcon.Palette, [nameof(InterfaceConfig.AccentColor)]),
                new("尺寸", FontAwesomeIcon.TextHeight,
                [
                    nameof(InterfaceConfig.TextScale), nameof(InterfaceConfig.ElementScale), nameof(InterfaceConfig.PaddingScale), nameof(InterfaceConfig.MotionScale),
                    nameof(InterfaceConfig.CompactSettings),
                ]),
            ],
            Fields = new()
            {
                [nameof(InterfaceConfig.AccentColor)] = new(Choices: _accentChoices),
                [nameof(InterfaceConfig.TextScale)] = new(Format: "%.2fx"),
                [nameof(InterfaceConfig.ElementScale)] = new(Format: "%.2fx"),
                [nameof(InterfaceConfig.PaddingScale)] = new(Format: "%.2fx"),
                [nameof(InterfaceConfig.MotionScale)] = new(Format: "%.2fx"),
            },
        },

        [typeof(BossModuleConfig)] = new()
        {
            Name = "战斗雷达与提示",
            Icon = FontAwesomeIcon.Bullseye,
            Description = "配置战斗雷达窗口、显示的提示以及战斗模块可执行的操作",
            Tags = ["boss modules and radar", "radar", "hints", "arena", "3d"],
            Sections =
            [
                new("战斗模块", FontAwesomeIcon.PuzzlePiece, [nameof(BossModuleConfig.MinMaturity), nameof(BossModuleConfig.AllowAutomaticActions), nameof(BossModuleConfig.ShowDutyFinderIndicator), nameof(BossModuleConfig.MaxLoadDistance)]),
                new("雷达窗口", FontAwesomeIcon.WindowMaximize,
                [
                    nameof(BossModuleConfig.EnableRadar), nameof(BossModuleConfig.ShowDemo), nameof(BossModuleConfig.Lock), nameof(BossModuleConfig.TrishaMode),
                    nameof(BossModuleConfig.OpaqueArenaBackground), nameof(BossModuleConfig.ArenaScale), nameof(BossModuleConfig.RotateArena), nameof(BossModuleConfig.FlipArena),
                    nameof(BossModuleConfig.SlackForRotations),
                ]),
                new("雷达内容", FontAwesomeIcon.MapMarkedAlt,
                [
                    nameof(BossModuleConfig.ShowBorder), nameof(BossModuleConfig.ShowBorderRisk), nameof(BossModuleConfig.ShowOutlinesAndShadows), nameof(BossModuleConfig.ThicknessScale),
                    nameof(BossModuleConfig.ActorScale), nameof(BossModuleConfig.ShowCardinals), nameof(BossModuleConfig.CardinalsFontSize), nameof(BossModuleConfig.ShowWaymarks),
                    nameof(BossModuleConfig.WaymarkFontSize), nameof(BossModuleConfig.ShowSigns), nameof(BossModuleConfig.ShowIrrelevantPlayers), nameof(BossModuleConfig.ColorPlayersBasedOnRole),
                    nameof(BossModuleConfig.ShowFocusTargetPlayer), nameof(BossModuleConfig.ShowMeleeRangeIndicator),
                ]),
                new("提示", FontAwesomeIcon.CommentDots,
                [
                    nameof(BossModuleConfig.ShowPrePullHints), nameof(BossModuleConfig.HintsInSeparateWindow), nameof(BossModuleConfig.HintsInSeparateWindowTransparent),
                    nameof(BossModuleConfig.ShowMechanicTimers), nameof(BossModuleConfig.ShowGlobalHints), nameof(BossModuleConfig.ShowPlayerHints),
                ]),
                new("3D 世界投影", FontAwesomeIcon.Cube,
                [
                    nameof(BossModuleConfig.ProjectRadarInto3DWorld), nameof(BossModuleConfig.ShowActorTrianglesIn3DWorld), nameof(BossModuleConfig.EnableArenaOutlineIn3DWorld),
                    nameof(BossModuleConfig.EnableTextIconBillboards), nameof(BossModuleConfig.BillboardHeightOffset), nameof(BossModuleConfig.TextBillboardFontSize),
                    nameof(BossModuleConfig.IconBillboardFontSize),
                ]),
                new("屏幕与世界", FontAwesomeIcon.Desktop, [nameof(BossModuleConfig.ShowScreenRiskBorder), nameof(BossModuleConfig.ScreenRiskBorderIntensity), nameof(BossModuleConfig.ShowWorldArrows)]),
            ],
            Fields = new()
            {
                [nameof(BossModuleConfig.MinMaturity)] = new(Options: _maturityOptions),
                [nameof(BossModuleConfig.MaxLoadDistance)] = new(Format: "%.0f y"),
                [nameof(BossModuleConfig.ArenaScale)] = new(Format: "%.2fx"),
                [nameof(BossModuleConfig.SlackForRotations)] = new(Format: "%.2fx"),
                [nameof(BossModuleConfig.ThicknessScale)] = new(Format: "%.2fx"),
                [nameof(BossModuleConfig.ActorScale)] = new(Format: "%.2fx"),
                [nameof(BossModuleConfig.ShowBorderRisk)] = new(Depends: nameof(BossModuleConfig.ShowBorder)),
                [nameof(BossModuleConfig.HintsInSeparateWindowTransparent)] = new(Depends: nameof(BossModuleConfig.HintsInSeparateWindow)),
                [nameof(BossModuleConfig.BillboardHeightOffset)] = new(Depends: nameof(BossModuleConfig.EnableTextIconBillboards), Format: "%.1f y"),
                [nameof(BossModuleConfig.TextBillboardFontSize)] = new(Depends: nameof(BossModuleConfig.EnableTextIconBillboards)),
                [nameof(BossModuleConfig.IconBillboardFontSize)] = new(Depends: nameof(BossModuleConfig.EnableTextIconBillboards)),
            },

            Custom = NoCustom,
            SectionExtras = static (_, section, _, _) =>
            {
                if (section == "雷达窗口" && M3Widgets.RowButton("雷达窗口位置###recenter", "移回屏幕中央",
                    "将雷达窗口移回屏幕中央。也可使用命令 /bmr radar reset。", icon: FontAwesomeIcon.Crosshairs))
                {
                    Service.BossModWindow?.RecenterWindow();
                }
            },
        },

        [typeof(ModuleConfig)] = new()
        {
            Name = "战斗设置",
            Icon = FontAwesomeIcon.SlidersH,
            Description = "配置各场战斗的策略、职责分配和提示",
            Tags = ["encounter-specific settings", "strategy"],
            Notice = (M3Severity.Info, FontAwesomeIcon.InfoCircle, "配置各场战斗的策略和提示。也可在“支持的战斗”列表中点击战斗旁的齿轮按钮打开相同设置。"),
        },

        [typeof(PartyRolesConfig)] = new()
        {
            Name = "队伍职责",
            Icon = FontAwesomeIcon.Users,
            Description = "为需要职责分配的战斗策略指定各队员的职责",
            Tags = ["party roles assignment", "mt", "ot", "h1", "h2"],
            Custom = static (node, _, ws) => PartyRolesPanel.Draw((PartyRolesConfig)node, ws),
        },

        [typeof(ColorConfig)] = new()
        {
            Name = "颜色",
            Icon = FontAwesomeIcon.Palette,
            Description = "设置雷达、技能冷却规划和调试视图中使用的颜色；每种颜色均可恢复默认值",
            Tags = ["color scheme", "colour"],
            Sections =
            [
                new("场地", FontAwesomeIcon.Bullseye,
                [
                    nameof(ColorConfig.ArenaBackground), nameof(ColorConfig.ArenaBorder), nameof(ColorConfig.ArenaAOE), nameof(ColorConfig.ArenaSafeFromAOE), nameof(ColorConfig.ArenaDanger),
                    nameof(ColorConfig.ArenaSafe), nameof(ColorConfig.ArenaEnemy), nameof(ColorConfig.ArenaObject), nameof(ColorConfig.ArenaTrap), nameof(ColorConfig.ArenaLight),
                    nameof(ColorConfig.ArenaVulnerable), nameof(ColorConfig.ArenaFutureVulnerable), nameof(ColorConfig.ArenaMeleeRangeIndicator), nameof(ColorConfig.ArenaOther),
                    nameof(ColorConfig.Shadows),
                ], "Arena: "),
                new("玩家", FontAwesomeIcon.Users,
                [
                    nameof(ColorConfig.ArenaPC), nameof(ColorConfig.ArenaPlayerInteresting), nameof(ColorConfig.ArenaPlayerGeneric), nameof(ColorConfig.ArenaPlayerReallyGeneric),
                    nameof(ColorConfig.ArenaPlayerGenericTank), nameof(ColorConfig.ArenaPlayerGenericHealer), nameof(ColorConfig.ArenaPlayerGenericMelee),
                    nameof(ColorConfig.ArenaPlayerGenericCaster), nameof(ColorConfig.ArenaPlayerGenericPhysRanged), nameof(ColorConfig.ArenaPlayerGenericFocus),
                ], "Arena: "),
                new("标记", FontAwesomeIcon.MapMarkerAlt,
                [
                    nameof(ColorConfig.WaymarkA), nameof(ColorConfig.WaymarkB), nameof(ColorConfig.WaymarkC), nameof(ColorConfig.WaymarkD), nameof(ColorConfig.Waymark1),
                    nameof(ColorConfig.Waymark2), nameof(ColorConfig.Waymark3), nameof(ColorConfig.Waymark4), nameof(ColorConfig.CardinalN), nameof(ColorConfig.CardinalE),
                    nameof(ColorConfig.CardinalS), nameof(ColorConfig.CardinalW), nameof(ColorConfig.PositionalColors),
                ]),
                new("技能冷却规划", FontAwesomeIcon.CalendarAlt,
                [
                    nameof(ColorConfig.PlannerBackground), nameof(ColorConfig.PlannerBackgroundHighlight), nameof(ColorConfig.PlannerCooldown), nameof(ColorConfig.PlannerFallback),
                    nameof(ColorConfig.PlannerEffect), nameof(ColorConfig.PlannerWindow),
                ], "Planner: "),
                new("界面与调试", FontAwesomeIcon.Bug,
                [
                    nameof(ColorConfig.ButtonPushColor), nameof(ColorConfig.TextColors), nameof(ColorConfig.CollisionColors), nameof(ColorConfig.PathfindingColors),
                    nameof(ColorConfig.PathfindingColors5), nameof(ColorConfig.PathfindingColors6), nameof(ColorConfig.PathfindingColors7), nameof(ColorConfig.PathfindingColors8),
                ]),
            ],
        },

        [typeof(ActionTweaksConfig)] = new()
        {
            Icon = FontAwesomeIcon.Wrench,
            Description = "调整手动操作和自动循环中的技能排队、选取目标及执行方式",
            Sections =
            [
                new("技能", FontAwesomeIcon.Bolt,
                [
                    nameof(ActionTweaksConfig.RemoveAnimationLockDelay), nameof(ActionTweaksConfig.AnimationLockDelayMax), nameof(ActionTweaksConfig.RemoveCooldownDelay),
                    nameof(ActionTweaksConfig.UseManualQueue), nameof(ActionTweaksConfig.AutoAutos), nameof(ActionTweaksConfig.AutoDismount), nameof(ActionTweaksConfig.CancelCastOnDeadTarget),
                ]),
                new("移动", FontAwesomeIcon.Running,
                [
                    nameof(ActionTweaksConfig.PreventMovingWhileCasting), nameof(ActionTweaksConfig.MoveEscapeHatch), nameof(ActionTweaksConfig.PyreticThreshold),
                    nameof(ActionTweaksConfig.MisdirectionThreshold), nameof(ActionTweaksConfig.RestoreRotation), nameof(ActionTweaksConfig.DashSafety), nameof(ActionTweaksConfig.DashSafetyExtra),
                ]),
                new("目标选择", FontAwesomeIcon.Crosshairs, [nameof(ActionTweaksConfig.PreferMouseover), nameof(ActionTweaksConfig.PreventForbiddenTargets), nameof(ActionTweaksConfig.GTMode)]),
            ],
            Fields = new()
            {
                [nameof(ActionTweaksConfig.AnimationLockDelayMax)] = new(Depends: nameof(ActionTweaksConfig.RemoveAnimationLockDelay)),
                [nameof(ActionTweaksConfig.MoveEscapeHatch)] = new(Tooltip: "按住此键时，即使正在施法且通常无法移动，也允许移动。", Depends: nameof(ActionTweaksConfig.PreventMovingWhileCasting)),
                [nameof(ActionTweaksConfig.PyreticThreshold)] = new("火炎机制安全阈值", "火炎类机制即将发生时，阻止移动和技能执行。设为 0 可关闭；否则请根据网络延迟调高阈值。", Format: "%.2f 秒"),
                [nameof(ActionTweaksConfig.MisdirectionThreshold)] = new("混乱移动自动保护阈值", "处于混乱移动效果时，若正常移动方向与反向移动方向的夹角大于此阈值，则阻止移动。设为 180 可关闭。", Format: "%.0f°"),
                [nameof(ActionTweaksConfig.DashSafetyExtra)] = new(Depends: nameof(ActionTweaksConfig.DashSafety)),
            },

            Custom = NoCustom,
            SectionExtras = static (node, section, _, _) =>
            {
                if (section == "目标选择")
                {
                    SmartTargetingRow((ActionTweaksConfig)node);
                }
            },
        },

        [typeof(SmartRotationConfig)] = new() { Icon = FontAwesomeIcon.Compass },
        [typeof(OutOfCombatActionsConfig)] = new()
        {
            Icon = FontAwesomeIcon.Walking,
            Fields = new() { [nameof(OutOfCombatActionsConfig.AutoPeloton)] = new(Depends: nameof(OutOfCombatActionsConfig.Enabled)) },
        },
        [typeof(SharedActionsConfig)] = new() { Icon = FontAwesomeIcon.ExchangeAlt },

        [typeof(AIConfig)] = new()
        {
            Icon = FontAwesomeIcon.Robot,
            Description = "跟随指定队员，并移动至战斗模块识别出的安全位置",
            Tags = ["ai", "follow"],
            Sections =
            [
                new("界面", FontAwesomeIcon.Desktop, [nameof(AIConfig.DrawUI), nameof(AIConfig.ShowDTR), nameof(AIConfig.EchoToChat)]),
                new("跟随", FontAwesomeIcon.Users,
                [
                    nameof(AIConfig.FollowSlot), nameof(AIConfig.FocusTargetMaster), nameof(AIConfig.FollowDuringCombat), nameof(AIConfig.FollowDuringActiveBossModule),
                    nameof(AIConfig.FollowOutOfCombat), nameof(AIConfig.MaxDistanceToSlot), nameof(AIConfig.FollowTarget), nameof(AIConfig.DesiredPositional),
                    nameof(AIConfig.FollowRSRDesiredPositional), nameof(AIConfig.MaxDistanceToTarget), nameof(AIConfig.MinDistance), nameof(AIConfig.PreferredDistance),
                ]),
                new("限制", FontAwesomeIcon.Ban, [nameof(AIConfig.ForbidActions), nameof(AIConfig.ForbidMovement), nameof(AIConfig.ManualTarget), nameof(AIConfig.ForbidAIMovementMounted)]),
                new("行为", FontAwesomeIcon.SlidersH, [nameof(AIConfig.AutoAFK), nameof(AIConfig.AFKModeTimer), nameof(AIConfig.MoveDelay), nameof(AIConfig.DisableObstacleMaps), nameof(AIConfig.BroadcastToSlaves)]),
            ],
            Fields = new()
            {
                [nameof(AIConfig.FollowSlot)] = new(Tooltip: "要跟随的队伍成员位置，范围为 0（通常是自己）至 7。", Min: 0, Max: 7),
                [nameof(AIConfig.MaxDistanceToSlot)] = new(Tooltip: "与跟随队员保持的距离。"),
                [nameof(AIConfig.FollowTarget)] = new(Tooltip: "战斗中改为跟随目标，而非跟随队员；跟随距离由下方选项设置。"),
                [nameof(AIConfig.DesiredPositional)] = new(Depends: nameof(AIConfig.FollowTarget)),
                [nameof(AIConfig.FollowRSRDesiredPositional)] = new(Depends: nameof(AIConfig.FollowTarget)),
                [nameof(AIConfig.MaxDistanceToTarget)] = new(Depends: nameof(AIConfig.FollowTarget)),
                [nameof(AIConfig.ManualTarget)] = new(Tooltip: "启用 AI 自动循环时仍允许手动选择目标。"),
                [nameof(AIConfig.AFKModeTimer)] = new(Depends: nameof(AIConfig.AutoAFK)),
            },
        },

        [typeof(AutorotationConfig)] = new()
        {
            Name = "自动循环",
            Icon = FontAwesomeIcon.Bolt,
            Description = "设置 BossMod 自带自动循环的启停行为；Combat Reborn 不支持此功能",
            Tags = ["unsupported", "rotation"],
            Notice = (M3Severity.Warning, FontAwesomeIcon.ExclamationTriangle, "Combat Reborn 不支持 BossMod 自带的自动循环。请勿在 Combat Reborn Discord 中询问此功能的相关问题。"),
            Sections =
            [
                new("界面", FontAwesomeIcon.Desktop,
                [
                    nameof(AutorotationConfig.ShowUI), nameof(AutorotationConfig.ShowDTR), nameof(AutorotationConfig.HideDefaultPresets), nameof(AutorotationConfig.ShowPositionals),
                    nameof(AutorotationConfig.FollowRSRDesiredPositional),
                ]),
                new("自动停止", FontAwesomeIcon.StopCircle,
                [
                    nameof(AutorotationConfig.ClearPresetOnDeath), nameof(AutorotationConfig.ClearPresetOnCombatEnd), nameof(AutorotationConfig.ClearPresetOnLuring),
                    nameof(AutorotationConfig.ClearForceDisableOnCombatEnd),
                ]),
                new("开怪安全", FontAwesomeIcon.ShieldAlt, [nameof(AutorotationConfig.EarlyPullThreshold), nameof(AutorotationConfig.PlannedPullSafety)]),
            ],
            Fields = new() { [nameof(AutorotationConfig.EarlyPullThreshold)] = new(Format: "%.1f s") },
        },

        [typeof(ZoneModuleConfig)] = new()
        {
            Name = "任务自动战斗",
            Icon = FontAwesomeIcon.Route,
            Description = "通过区域模块自动完成任务战斗和单人任务",
            Tags = ["full duty automation", "zone", "quest"],
            Sections =
            [
                new("常规", FontAwesomeIcon.None, [nameof(ZoneModuleConfig.MinMaturity), nameof(ZoneModuleConfig.EnableQuestBattles), nameof(ZoneModuleConfig.UseDash)]),
                new("显示", FontAwesomeIcon.WindowMaximize, [nameof(ZoneModuleConfig.ShowWaypoints), nameof(ZoneModuleConfig.Lock), nameof(ZoneModuleConfig.TransparentMode)]),
            ],
            Fields = new() { [nameof(ZoneModuleConfig.MinMaturity)] = new(Options: _maturityOptions) },
        },

        [typeof(ReplayManagementConfig)] = new()
        {
            Icon = FontAwesomeIcon.Film,
            Description = "录制战斗，用于提交错误报告、开发战斗模块和制定技能冷却计划",
            Tags = ["record", "log"],
            Sections =
            [
                new("录制", FontAwesomeIcon.Video,
                [
                    nameof(ReplayManagementConfig.ShowUI), nameof(ReplayManagementConfig.ImportantDutyAlert), nameof(ReplayManagementConfig.AutoRecord), nameof(ReplayManagementConfig.AutoARR),
                    nameof(ReplayManagementConfig.Anonymize), nameof(ReplayManagementConfig.MaxReplays), nameof(ReplayManagementConfig.WorldLogFormat),
                ]),
                new("数据包", FontAwesomeIcon.NetworkWired,
                [
                    nameof(ReplayManagementConfig.RecordServerPackets), nameof(ReplayManagementConfig.DumpServerPackets), nameof(ReplayManagementConfig.DumpServerPacketsPlayerOnly),
                    nameof(ReplayManagementConfig.DumpClientPackets),
                ]),
            ],
            Fields = new()
            {
                [nameof(ReplayManagementConfig.MaxReplays)] = new(Tooltip: "设为 0 可保留所有回放。"),
                [nameof(ReplayManagementConfig.DumpServerPacketsPlayerOnly)] = new(Depends: nameof(ReplayManagementConfig.DumpServerPackets)),
            },
        },

        [typeof(DeveloperConfig)] = new()
        {
            Name = "开发者",
            Icon = FontAwesomeIcon.Code,
            Description = "BossMod Reborn 开发者使用的设置",
            Tags = ["developer settings", "obstacle"],
            Fields = new() { [nameof(DeveloperConfig.MapSourcePath)] = new(Depends: nameof(DeveloperConfig.MapLoadFromSource)) },
        },
    };

    private static void SmartTargetingRow(ActionTweaksConfig config)
    {
        var rsrEnabled = ActionTweaksConfig.IsRSREnabled();
        const string supporting = "如果常用目标（鼠标指向目标或当前主目标）不符合技能目标要求，则自动选择下一个合适目标（例如 Shirk 的副坦克）。";
        var row = M3SettingRow.Begin("智能技能目标", rsrEnabled ? $"{supporting}\n当前已加载 Rotation Solver Reborn，请勿同时启用此功能。" : $"{supporting}\n请勿与 Rotation Solver Reborn 同时使用。",
            M3Widgets.SwitchSize(), rsrEnabled ? M3.Scheme.Error : null);
        ImGui.SetCursorScreenPos(row.ControlPosition);
        if (M3Widgets.Switch("##SmartTargeting", ref config.SmartTargeting))
        {
            config.Modified.Fire();
        }
        M3SettingRow.End(row);
    }
}
