namespace BossMod;

// game updates its timings every frame (in the beginning of the tick)
// it's all based on QueryPerformanceCounter; we assume that its frequency can never change (indeed, game never tries to update it anyway, and OS guarantees that too)
// unfortunately, game does slightly weird thing - it samples QPC, calculates raw dt, then samples it again and stores second value as 'previous' QPC
// this means that raw dt is slightly smaller than d(QPC)/QPF
// then there is a frame duration override logic, which can modify dt used for further calculations (not sure when is it used)
// finally, there is additional multiplier for cooldown/status/etc. calculations (used by duty recorder replays)
public struct FrameState(DateTime timestamp, ulong qpc, uint index, float durationRaw, float duration, float tickSpeedMultiplier)
{
    public DateTime Timestamp = timestamp;
    public ulong QPC = qpc;
    public uint Index = index;
    public float Duration = duration;
    public float DurationRaw = durationRaw;
    public float TickSpeedMultiplier = tickSpeedMultiplier;
}
