namespace BossMod;

// manager that handles automatic party role assignment when entering duties
public sealed class PartyRolesManager : IDisposable
{
    private readonly WorldState _ws;
    private readonly PartyRolesConfig _config;
    private readonly EventSubscriptions _subscriptions;
    private ushort _lastZone;
    private bool _pendingAutoAssign;
    private DateTime _autoAssignDeadline;

    public PartyRolesManager(WorldState ws)
    {
        _ws = ws;
        _config = Service.Config.Get<PartyRolesConfig>();
        _lastZone = ws.CurrentZone;

        _subscriptions = new(
            ws.CurrentZoneChanged.Subscribe(OnZoneChanged)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public void Update()
    {
        if (!_pendingAutoAssign)
            return;

        // check if we've exceeded the deadline
        if (_ws.CurrentTime >= _autoAssignDeadline)
        {
            Service.Log($"[PartyRoles] Auto-assign timeout reached, assigning with current party state");
            _config.AutoAssignRoles(_ws.Party);
            _pendingAutoAssign = false;
            return;
        }

        // check if all party members have valid actors
        var allValid = true;
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            ref var m = ref _ws.Party.Members[i];
            if (m.IsValid() && m.ContentId != 0)
            {
                var actor = _ws.Party[i];
                if (actor == null)
                {
                    allValid = false;
                    break;
                }
            }
        }

        if (allValid)
        {
            Service.Log($"[PartyRoles] All party members valid, auto-assigning roles");
            _config.AutoAssignRoles(_ws.Party);
            _pendingAutoAssign = false;
        }
    }

    private void OnZoneChanged(WorldState.OpZoneChange op)
    {
        if (!_config.AutoAssignOnDutyEnter)
        {
            _lastZone = op.Zone;
            return;
        }

        if (_lastZone != op.Zone && _lastZone != 0)
        {
            Service.Log($"[PartyRoles] Entering duty, scheduling auto-assign roles");
            _pendingAutoAssign = true;
            _autoAssignDeadline = _ws.CurrentTime.AddSeconds(5); // 5 second timeout
        }

        _lastZone = op.Zone;
    }
}
