namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class WARConfig : ConfigNode
{
    [PropertyDisplay("偏好自身目标的死斗；需要启用智能目标选择（并允许通过鼠标悬停覆盖目标）")]
    public bool HolmgangSelf = true;

    [PropertyDisplay("禁止在开怪前过早使用战斧")]
    public bool ForbidEarlyTomahawk = true;
}
