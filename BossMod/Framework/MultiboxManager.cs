using BossMod.Autorotation;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat;
using Dalamud.Game.Text;

namespace BossMod;

internal sealed class MultiboxManager : IDisposable
{
    private readonly RotationModuleManager _rotations;
    private readonly WorldState _ws;

    public MultiboxManager(RotationModuleManager mgr, WorldState ws)
    {
        _rotations = mgr;
        _ws = ws;
        Service.ChatGui.ChatMessage += OnChatMessage;
    }

    public void Dispose() => Service.ChatGui.ChatMessage -= OnChatMessage;

    private void OnChatMessage(IHandleableChatMessage chatMessage)
    {
        if (chatMessage.LogKind == XivChatType.Echo && chatMessage.Message.TextValue == "test")
        {
            var leaderId = _ws.Party.Members[0].ContentId;

            foreach (var p in _rotations.Database.Presets.AllPresets)
            {
#if DEBUG
                Preset.ModuleSettings? md = null;
                foreach (var m in p.Modules) if (m.Type == typeof(Autorotation.MiscAI.Multibox)) { md = m; break; }
                if (md != null)
                    md.TransientSettings.Add(new Preset.ModuleSetting(default, 0, new StrategyValueInt() { Value = (long)leaderId }));
                else
                    Service.Log($"no matching module in {p.Name}");
#endif
            }
        }
    }
}
