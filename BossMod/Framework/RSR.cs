using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
namespace BossMod;

public sealed class RotationSolverRebornModule(IDalamudPluginInterface pluginInterface)
{
    private readonly ICallGateSubscriber<SpecialCommandType, object> _changeOperationMode = pluginInterface.GetIpcSubscriber<SpecialCommandType, object>("RotationSolverReborn.TriggerSpecialState");
    private const string rsr = "Rotation Solver Reborn";

    public bool IsInstalled
    {
        get
        {
            var installedPlugins = pluginInterface.InstalledPlugins;
            foreach (var x in installedPlugins)
            {
                if (x.IsLoaded && x.Name.Equals(rsr, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public void PauseRSR()
    {
        _changeOperationMode.InvokeAction(SpecialCommandType.NoCasting);
    }

    public void UnPauseRSR()
    {
        _changeOperationMode.InvokeAction(SpecialCommandType.EndSpecial);
    }

    public enum SpecialCommandType : byte
    {
        EndSpecial,
        HealArea,
        HealSingle,
        DefenseArea,
        DefenseSingle,
        DispelStancePositional,
        RaiseShirk,
        MoveForward,
        MoveBack,
        AntiKnockback,
        Burst,
        Speed,
        LimitBreak,
        NoCasting,
    }
}
