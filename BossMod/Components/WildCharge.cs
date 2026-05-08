namespace BossMod.Components;

// generic 'wild charge': various mechanics that consist of charge aoe on some target that other players have to stay in; optionally some players can be marked as 'having to be closest to source' (usually tanks)
[SkipLocalsInit]
public class GenericWildCharge(BossModule module, float halfWidth, uint aid = default, float fixedLength = default)
    : CastCounter(module, aid)
{
    public enum PlayerRole
    {
        Ignore, // player completely ignores the mechanic; no hints for such players are displayed
        Target, // player is charge target
        TargetNotFirst, // player is charge target, and has to hide behind other raid member
        Share, // player has to stay inside aoe
        ShareNotFirst, // player has to stay inside aoe, but not as a closest raid member
        Avoid, // player has to avoid aoe
    }

    public readonly float HalfWidth = halfWidth;
    public readonly float FixedLength = fixedLength; // if == 0, length is up to target
    public Actor? Source; // if null, mechanic is not active
    public DateTime Activation;
    public PlayerRole[] PlayerRoles = new PlayerRole[PartyState.MaxAllies];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Source == null)
        {
            return;
        }

        switch (PlayerRoles[slot])
        {
            case PlayerRole.Ignore:
            case PlayerRole.Target: // TODO: consider hints for target?..
                break; // nothing to advise
            case PlayerRole.TargetNotFirst:
                var inOtherCharge = false;
                foreach (var aoe in EnumerateAOEs(slot))
                {
                    if (InAOE(aoe, actor))
                    {
                        inOtherCharge = true;
                        break;
                    }
                }

                if (inOtherCharge)
                {
                    hints.Add("离开其他冲锋范围！");
                }
                else if (!AnyRoleCloser(GetAOEForTarget(Source.Position, actor.Position), PlayerRole.Share,
                             PlayerRole.Share, (actor.Position - Source.Position).LengthSq()))
                    hints.Add("躲到坦克身后！");

                break;
            case PlayerRole.Share:
            case PlayerRole.ShareNotFirst:
                var badShare = false;
                var numShares = 0;
                foreach (var aoe in EnumerateAOEs())
                {
                    if (!InAOE(aoe, actor))
                    {
                        continue;
                    }

                    if (++numShares > 1)
                    {
                        break;
                    }

                    badShare = PlayerRoles[slot] == PlayerRole.Share
                        ? AnyRoleCloser(aoe, PlayerRole.ShareNotFirst, PlayerRole.TargetNotFirst,
                            (actor.Position - Source.Position).LengthSq())
                        : !AnyRoleCloser(aoe, PlayerRole.Share, PlayerRole.Target,
                            (actor.Position - Source.Position).LengthSq());
                }

                if (numShares == 0)
                {
                    hints.Add("进入冲锋范围！");
                }
                else if (numShares > 1)
                {
                    hints.Add("只站在一个冲锋范围内！");
                }
                else if (badShare)
                {
                    hints.Add(PlayerRoles[slot] == PlayerRole.Share ? "靠近冲锋源！" : "躲到坦克身后！");
                }

                break;
            case PlayerRole.Avoid:
                var inCharge = false;
                foreach (var aoe in EnumerateAOEs())
                {
                    if (InAOE(aoe, actor))
                    {
                        inCharge = true;
                        break;
                    }
                }

                if (inCharge)
                {
                    hints.Add("离开冲锋范围！");
                }

                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source == null)
        {
            return;
        }

        var forbiddenInverted = new List<ShapeDistance>();
        var forbidden = new List<ShapeDistance>();
        switch (PlayerRoles[slot])
        {
            case PlayerRole.Ignore:
                break;
            case PlayerRole.Target:
            case PlayerRole.TargetNotFirst: // TODO: consider some hint to hide behind others?..
                // TODO: improve this - for now, just stack with closest player...
                if (Source != null)
                {
                    var closest = Raid.WithSlot()
                        .WhereSlot(i => PlayerRoles[i] is PlayerRole.Share or PlayerRole.ShareNotFirst).Actors()
                        .Closest(actor.Position);
                    if (closest != null)
                    {
                        var stack = GetAOEForTarget(Source.Position, closest.Position);
                        forbiddenInverted.Add(new SDInvertedRect(stack.origin, stack.dir, stack.length, 0,
                            HalfWidth * 0.5f));
                    }
                }

                break;
            case PlayerRole.Share: // TODO: some hint to be first in line...
            case PlayerRole.ShareNotFirst:
                foreach (var aoe in EnumerateAOEs())
                {
                    forbiddenInverted.Add(new SDInvertedRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth));
                }

                break;
            case PlayerRole.Avoid:
                foreach (var aoe in EnumerateAOEs())
                {
                    forbiddenInverted.Add(new SDRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth));
                }

                break;
        }

        foreach (var aoe in EnumerateAOEs())
        {
            // TODO add separate "tankbuster" hint for PlayerRole.Share if there are any ShareNotFirsts in the party
            var mask = new BitMask();
            foreach (var (pi, pa) in Raid.WithSlot())
            {
                if (InAOE(aoe, pa))
                {
                    mask.Set(pi);
                }
            }

            hints.AddPredictedDamage(mask, Activation);
        }

        if (forbiddenInverted.Count != 0)
        {
            hints.AddForbiddenZone(new SDOutsideOfUnion([.. forbiddenInverted]), Activation);
        }

        if (forbidden.Count != 0)
        {
            hints.AddForbiddenZone(new SDUnion([.. forbidden]), Activation);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Source == null || PlayerRoles[pcSlot] == PlayerRole.Ignore)
        {
            return;
        }

        foreach (var aoe in EnumerateAOEs())
        {
            var dangerous = PlayerRoles[pcSlot] == PlayerRole.Avoid; // TODO: reconsider this condition
            Arena.ZoneRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth, dangerous ? Colors.AOE : Colors.SafeFromAOE);
        }
    }

    private (WPos origin, WDir dir, float length) GetAOEForTarget(WPos sourcePos, WPos targetPos)
    {
        var toTarget = targetPos - sourcePos;
        var length = FixedLength > 0 ? FixedLength : toTarget.Length();
        var dir = toTarget.Normalized();
        return (sourcePos, dir, length);
    }

    protected bool InAOE((WPos origin, WDir dir, float length) aoe, Actor actor) =>
        actor.Position.InRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth);

    protected IEnumerable<(WPos origin, WDir dir, float length)> EnumerateAOEs(int targetSlotToSkip = -1)
    {
        if (Source == null)
        {
            yield break;
        }

        foreach (var (i, p) in Module.Raid.WithSlot().WhereSlot(i =>
                     i != targetSlotToSkip && PlayerRoles[i] is PlayerRole.Target or PlayerRole.TargetNotFirst))
        {
            yield return GetAOEForTarget(Source.Position, p.Position);
        }
    }

    private bool AnyRoleCloser((WPos origin, WDir dir, float length) aoe, PlayerRole role1, PlayerRole role2,
        float thresholdSq)
    {
        foreach (var ia in Raid.WithSlot())
        {
            if ((PlayerRoles[ia.Item1] == role1 || PlayerRoles[ia.Item1] == role2) && InAOE(aoe, ia.Item2) &&
                (ia.Item2.Position - aoe.origin).LengthSq() < thresholdSq)
            {
                return true;
            }
        }

        return false;
    }
}

//Variation on Generic Wild Charge, but where the origin is 'behind' the target, and the charge 'toward' the Source.
public class InverseWildCharge(
    BossModule module,
    float halfWidth,
    float distancebehind,
    uint aid = default,
    float fixedLength = default) : CastCounter(module, aid)
{
    public enum PlayerRole
    {
        Ignore, // player completely ignores the mechanic; no hints for such players are displayed
        Target, // player is charge target
        TargetNotFirst, // player is charge target, and has to hide behind other raid member
        Share, // player has to stay inside aoe
        ShareNotFirst, // player has to stay inside aoe, but not as a closest raid member
        Avoid, // player has to avoid aoe
    }

    public readonly float HalfWidth = halfWidth;
    public readonly float FixedLength = fixedLength; // if == 0, length is up to target
    public Actor? Source; // if null, mechanic is not active
    public DateTime Activation;
    public PlayerRole[] PlayerRoles = new PlayerRole[PartyState.MaxAllies];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Source == null)
        {
            return;
        }

        switch (PlayerRoles[slot])
        {
            case PlayerRole.Ignore:
            case PlayerRole.Target: // TODO: consider hints for target?..
                break; // nothing to advise
            case PlayerRole.TargetNotFirst:
                var inOtherChargeInv = false;
                foreach (var aoe in EnumerateAOEs(slot))
                {
                    if (InAOE(aoe, actor))
                    {
                        inOtherChargeInv = true;
                        break;
                    }
                }

                if (inOtherChargeInv)
                {
                    hints.Add("离开其他冲锋范围！");
                }
                else if (!AnyRoleCloser(GetAOEForTarget(Source.Position, actor.Position, distancebehind),
                             PlayerRole.Share, PlayerRole.Share, (actor.Position - Source.Position).LengthSq()))
                {
                    hints.Add("躲到坦克身后！");
                }

                break;
            case PlayerRole.Share:
            case PlayerRole.ShareNotFirst:
                var badShareInv = false;
                var numSharesInv = 0;
                foreach (var aoe in EnumerateAOEs())
                {
                    if (!InAOE(aoe, actor))
                    {
                        continue;
                    }

                    if (++numSharesInv > 1)
                    {
                        break;
                    }

                    badShareInv = PlayerRoles[slot] == PlayerRole.Share
                        ? AnyRoleCloser(aoe, PlayerRole.ShareNotFirst, PlayerRole.TargetNotFirst,
                            (actor.Position - Source.Position).LengthSq())
                        : !AnyRoleCloser(aoe, PlayerRole.Share, PlayerRole.Target,
                            (actor.Position - Source.Position).LengthSq());
                }

                if (numSharesInv == 0)
                {
                    hints.Add("进入冲锋范围！");
                }
                else if (numSharesInv > 1)
                {
                    hints.Add("只站在一个冲锋范围内！");
                }
                else if (badShareInv)
                {
                    hints.Add(PlayerRoles[slot] == PlayerRole.Share
                        ? "靠近冲锋源！"
                        : "躲到坦克身后！");
                }

                break;
            case PlayerRole.Avoid:
                var inChargeInv = false;
                foreach (var aoe in EnumerateAOEs())
                {
                    if (InAOE(aoe, actor))
                    {
                        inChargeInv = true;
                        break;
                    }
                }

                if (inChargeInv)
                {
                    hints.Add("离开冲锋范围！");
                }

                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source == null)
        {
            return;
        }

        var forbiddenInverted = new List<ShapeDistance>();
        var forbidden = new List<ShapeDistance>();
        switch (PlayerRoles[slot])
        {
            case PlayerRole.Ignore:
                break;
            case PlayerRole.Target:
            case PlayerRole.TargetNotFirst: // TODO: consider some hint to hide behind others?..
                // TODO: improve this - for now, just stack with closest player...
                if (Source != null)
                {
                    var closest = Raid.WithSlot()
                        .WhereSlot(i => PlayerRoles[i] is PlayerRole.Share or PlayerRole.ShareNotFirst).Actors()
                        .Closest(actor.Position);
                    if (closest != null)
                    {
                        var stack = GetAOEForTarget(Source.Position, closest.Position, distancebehind);
                        forbiddenInverted.Add(new SDInvertedRect(stack.origin, stack.dir, stack.length, 0,
                            HalfWidth * 0.5f));
                    }
                }

                break;
            case PlayerRole.Share: // TODO: some hint to be first in line...
            case PlayerRole.ShareNotFirst:
                foreach (var aoe in EnumerateAOEs())
                {
                    forbiddenInverted.Add(new SDInvertedRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth));
                }

                break;
            case PlayerRole.Avoid:
                foreach (var aoe in EnumerateAOEs())
                {
                    forbiddenInverted.Add(new SDRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth));
                }

                break;
        }

        foreach (var aoe in EnumerateAOEs())
        {
            // TODO add separate "tankbuster" hint for PlayerRole.Share if there are any ShareNotFirsts in the party
            var maskInv = new BitMask();
            foreach (var (pi, pa) in Raid.WithSlot())
            {
                if (InAOE(aoe, pa))
                {
                    maskInv.Set(pi);
                }
            }

            hints.AddPredictedDamage(maskInv, Activation);
        }

        if (forbiddenInverted.Count != 0)
        {
            hints.AddForbiddenZone(new SDOutsideOfUnion([.. forbiddenInverted]), Activation);
        }

        if (forbidden.Count != 0)
        {
            hints.AddForbiddenZone(new SDUnion([.. forbidden]), Activation);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Source == null || PlayerRoles[pcSlot] == PlayerRole.Ignore)
        {
            return;
        }

        foreach (var aoe in EnumerateAOEs())
        {
            var dangerous = PlayerRoles[pcSlot] == PlayerRole.Avoid; // TODO: reconsider this condition
            Arena.ZoneRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth, dangerous ? Colors.AOE : Colors.SafeFromAOE);
        }
    }

    //To invert, we catch it at the AoE generation -- invert the direction, extend it behind the player.
    private (WPos origin, WDir dir, float length) GetAOEForTarget(WPos sourcePos, WPos targetPos, float distbeh)
    {
        var toTarget = targetPos - sourcePos;
        var dir = toTarget.Normalized();
        var invertedOrigin = targetPos + dir * distbeh;
        var toInvertedOrigin = invertedOrigin - sourcePos;
        var length = FixedLength > 0 ? FixedLength : toInvertedOrigin.Length();
        dir = toInvertedOrigin.Normalized().Rotate(180f.Degrees());
        return (invertedOrigin, dir, length);
    }

    protected bool InAOE((WPos origin, WDir dir, float length) aoe, Actor actor) =>
        actor.Position.InRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth);

    protected IEnumerable<(WPos origin, WDir dir, float length)> EnumerateAOEs(int targetSlotToSkip = -1)
    {
        if (Source == null)
        {
            yield break;
        }

        foreach (var (i, p) in Module.Raid.WithSlot().WhereSlot(i =>
                     i != targetSlotToSkip && PlayerRoles[i] is PlayerRole.Target or PlayerRole.TargetNotFirst))
        {
            yield return GetAOEForTarget(Source.Position, p.Position, distancebehind);
        }
    }

    // Invert this too so that tanks don't get bad directions.  Just swap the '<' for a '>'
    private bool AnyRoleCloser((WPos origin, WDir dir, float length) aoe, PlayerRole role1, PlayerRole role2,
        float thresholdSq)
    {
        foreach (var ia in Raid.WithSlot())
        {
            if ((PlayerRoles[ia.Item1] == role1 || PlayerRoles[ia.Item1] == role2) && InAOE(aoe, ia.Item2) &&
                (ia.Item2.Position - aoe.origin).LengthSq() > thresholdSq)
            {
                return true;
            }
        }

        return false;
    }
}
