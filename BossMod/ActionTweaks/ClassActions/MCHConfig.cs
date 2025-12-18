namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class MCHConfig : ConfigNode
{
    [PropertyDisplay("引导火焰喷射器时暂停自动循环")]
    public bool PauseForFlamethrower = false;
}
