namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class WHMConfig : ConfigNode
{
    [PropertyDisplay("使以太步与镜头方向对齐")]
    public bool AlignDashToCamera = false;
}
