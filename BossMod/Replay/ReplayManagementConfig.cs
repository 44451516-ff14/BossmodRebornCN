namespace BossMod;

public record struct ReplayMemory(string Path, bool IsOpen, DateTime PlaybackPosition);


[ConfigDisplay(Name = "录像设置", Order = 0)]

public sealed class ReplayManagementConfig : ConfigNode

{
     [PropertyDisplay("显示录像管理界面")]
    public bool ShowUI = false;

    [PropertyDisplay("副本开始自动录像")]
    public bool AutoRecord = false;
    

    [PropertyDisplay("Auto record in Duty Recorder replays", tooltip: "Requires auto-record to be turned on")]
    public bool AutoARR = false;

    
    [PropertyDisplay("最大录像数量")]
    [PropertySlider(0, 1000)]
    public int MaxReplays = 0;

    [PropertyDisplay("在录像中记录和存储服务器数据包")]
    public bool RecordServerPackets = false;

    [PropertyDisplay("Dump server packets into dalamud.log")]
    public bool DumpServerPackets = false;

    [PropertyDisplay("Ignore packets for other players when dumping to dalamud.log")]
    public bool DumpServerPacketsPlayerOnly = false;

    [PropertyDisplay("Dump client packets into dalamud.log")]
    public bool DumpClientPackets = false;

    [PropertyDisplay("Format for recorded logs")]
    public ReplayLogFormat WorldLogFormat = ReplayLogFormat.BinaryCompressed;

    [PropertyDisplay("Open previously open replays on plugin reload")]
    public bool RememberReplays;

    [PropertyDisplay("Remember playback position for previously opened replays")]
    public bool RememberReplayTimes;

    // TODO: this should not be part of the actual config! figure out where to store transient user preferences...
    public List<ReplayMemory> ReplayHistory = [];

    public string ReplayFolder = "";
}
