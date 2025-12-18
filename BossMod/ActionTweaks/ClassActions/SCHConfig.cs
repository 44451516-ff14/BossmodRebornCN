namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class SCHConfig : ConfigNode
{
    [PropertyDisplay("防止在开怪前过早使用'灼热'")]
    public bool ForbidEarlyBroil = true;
}
