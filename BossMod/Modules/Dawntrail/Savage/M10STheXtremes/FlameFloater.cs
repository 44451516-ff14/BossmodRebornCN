namespace BossMod.Dawntrail.Savage.M10STheXtremes;

sealed class FlameFloater(BossModule module) : Components.GenericAOEs(module)
{
    // could also try using a tether component?
    // tethers roughly 13.5f before changing to safe
    private readonly int[] _baitTargets = [-1, -1, -1, -1];
    private readonly int[] _baitSlot = new int[8];
    private readonly AOEShapeRect _rect = new(60f, 4f);
    public bool Active = false;
    private int _voidSpawned = 0;
    private const float _tetherDist = 13.5f;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_voidSpawned >= 4)
            return [];

        if (slot == _baitTargets[_voidSpawned])
            return [];

        var baiter = GetNextTarget();
        if (baiter == null)
            return [];

        var dir = baiter.Position - Module.PrimaryActor.Position;
        AOEInstance[] aoe = [new AOEInstance(new AOEShapeRect(60f, 4f), Module.PrimaryActor.Position, dir.ToAngle())];
        return aoe;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is not (uint)SID.FirstInLine and not (uint)SID.SecondInLine and not (uint)SID.ThirdInLine and not (uint)SID.FourthInLine)
            return;

        var order = status.ID switch
        {
            (uint)SID.FirstInLine => 0,
            (uint)SID.SecondInLine => 1,
            (uint)SID.ThirdInLine => 2,
            (uint)SID.FourthInLine => 3,
            _ => -1
        };

        if (order == -1)
            return;

        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot == -1)
            return;

        _baitTargets[order] = slot;
        _baitSlot[slot] = order + 1;
        Active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not (uint)AID.FlameFloater1 and not (uint)AID.FlameFloater2 and not (uint)AID.FlameFloater3 and not (uint)AID.FlameFloater4)
            return;

        var order = spell.Action.ID switch
        {
            (uint)AID.FlameFloater1 => 0,
            (uint)AID.FlameFloater2 => 1,
            (uint)AID.FlameFloater3 => 2,
            (uint)AID.FlameFloater4 => 3,
            _ => -1
        };

        if (order == -1)
            return;

        NumCasts++;

        if (NumCasts == 4)
        {
            Active = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (!Active)
            return;

        if (!IsBaitTarget(slot))
            return;

        var order = _baitSlot[slot];
        if (order == 0)
            return;

        hints.Add($"Bait order: {order}", false);

        order = BaitOrder(slot);
        if (order == -1)
            return;

        var bait = Raid[_baitTargets[order == 0 ? order : order - 1]];
        if (bait == null)
            return;

        var startPos = order == _voidSpawned ? Module.PrimaryActor.Position : bait.Position;

        var dist = actor.DistanceToPoint(startPos);
        if (dist < _tetherDist)
        {
            hints.Add("Stretch tether further!");
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.PuddleFlameFloater)
        {
            _voidSpawned++;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        if (!IsBaitTarget(pcSlot))
            return;

        var order = BaitOrder(pcSlot);
        if (order == -1)
            return;

        var bait = Raid[_baitTargets[order == 0 ? order : order - 1]];
        if (bait == null)
            return;

        var startPos = order == _voidSpawned ? Module.PrimaryActor.Position : bait.Position;
        var pcPos = pc.Position;
        var dir = pcPos - startPos;
        Arena.AddRect(startPos, dir.Normalized(), 60f, 0f, 4f, Colors.Danger, 1f);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;

        for (var i = 0; i < len; i++)
        {
            var slot = party[i].Item1;

            if (!IsBaitTarget(slot))
                continue;

            var order = BaitOrder(slot);
            if (order == -1)
                continue;

            // only show AOE for current bait for non-baiters
            if (order != _voidSpawned)
                continue;

            // don't show if next bait is player
            if (slot == pcSlot)
                continue;

            var bait = Raid[_baitTargets[order]];
            if (bait == null)
                continue;

            var startPos = Module.PrimaryActor.Position;
            var baitPos = bait.Position;
            var dir = baitPos - startPos;
            _rect.Draw(Arena, startPos, dir.ToAngle(), Colors.AOE);
        }
    }

    private bool IsBaitTarget(int slot)
    {
        for (var i = NumCasts; i < _baitTargets.Length; i++)
        {
            if (_baitTargets[i] == slot)
                return true;
        }

        return false;
    }

    private int BaitOrder(int slot)
    {
        for (var i = NumCasts; i < _baitTargets.Length; i++)
        {
            if (_baitTargets[i] == slot)
                return i;
        }

        return -1;
    }

    private Actor? GetNextTarget()
    {
        if (NumCasts >= _baitTargets.Length)
            return null;

        var target = Raid[_baitTargets[_voidSpawned]];
        return target;
    }
}
