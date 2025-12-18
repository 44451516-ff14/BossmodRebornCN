namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class PLDConfig : ConfigNode
{
    [PropertyDisplay("防止在开怪前过早使用'神圣'")]
    public bool ForbidEarlyHolySpirit = true;

    [PropertyDisplay("防止在开怪前过早使用'盾牌投掷'（如果神圣未解锁）")]
    public bool ForbidEarlyShieldLob = true;

    public enum WingsBehavior : uint
    {
        [PropertyDisplay("游戏默认（角色相对，向后）")]
        Default = 0,

        [PropertyDisplay("角色相对，向前")]
        CharacterForward = 1,

        [PropertyDisplay("镜头相对，向后")]
        CameraBackward = 2,

        [PropertyDisplay("镜头相对，向前")]
        CameraForward = 3,
    }

    [PropertyDisplay("武装圣域方向")]
    public WingsBehavior Wings = WingsBehavior.Default;
}
