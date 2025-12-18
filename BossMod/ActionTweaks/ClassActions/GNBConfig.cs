namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class GNBConfig : ConfigNode
{
    [PropertyDisplay("防止在开怪前过早使用'闪雷弹'")]
    public bool ForbidEarlyLightningShot = true;
}
