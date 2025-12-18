namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class RPRConfig : ConfigNode
{
    [PropertyDisplay("禁止在开怪前过早使用收割")]
    public bool ForbidEarlyHarpe = true;

    [PropertyDisplay("使地狱之门/地狱之路与镜头方向对齐")]
    public bool AlignDashToCamera = false;
}
