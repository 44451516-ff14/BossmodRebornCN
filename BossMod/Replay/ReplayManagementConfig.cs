namespace BossMod;

public record struct ReplayMemory(string Path, bool IsOpen, DateTime PlaybackPosition);

[ConfigDisplay(Name = "录像设置", Order = 0)]
public sealed class ReplayManagementConfig : ConfigNode
{
    [PropertyDisplay("显示录像管理界面")]
    public bool ShowUI = false;

    [PropertyDisplay("副本开始自动录像")]
    public bool ImportantDutyAlert = true;

    [PropertyDisplay("Auto record replays on duty start/end or outdoor module start/end")]
    public bool AutoRecord = false;

    [PropertyDisplay("职责记录器重放时自动录像", tooltip: "需要启用自动录像功能")]
    public bool AutoARR = false;

    [PropertyDisplay("最大保存录像数量")]
    [PropertySlider(0, 1000)]
    public int MaxReplays = 0;

    [PropertyDisplay("在录像中记录和存储服务器数据包")]
    public bool RecordServerPackets = false;

    [PropertyDisplay("将服务器数据包导出到dalamud.log")]
    public bool DumpServerPackets = false;

    [PropertyDisplay("导出到dalamud.log时忽略其他玩家的数据包")]
    public bool DumpServerPacketsPlayerOnly = false;

    [PropertyDisplay("将客户端数据包导出到dalamud.log")]
    public bool DumpClientPackets = false;

    [PropertyDisplay("录制日志的格式")]
    public ReplayLogFormat WorldLogFormat = ReplayLogFormat.BinaryCompressed;

    [PropertyDisplay("插件重载时打开之前打开的录像")]
    public bool RememberReplays;

    [PropertyDisplay("记住先前打开录像的播放位置")]
    public bool RememberReplayTimes;

    // TODO: 这不应是实际配置的一部分！需确定临时用户偏好的存储位置...
    public List<ReplayMemory> ReplayHistory = [];

    public string ReplayFolder = "";
}
