namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME2SwordDancer;

[SkipLocalsInit]
sealed class Cyclosword(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly Dictionary<ulong, AOEShape> _cycloswords = [];
    public int MaxCasts = 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || MaxCasts == 0)
        {
            return [];
        }

        var max = count > MaxCasts ? MaxCasts : count;

        if (MaxCasts == 2)
        {
            // followup AOE resolves at roughly same time as next AOE
            // skip and take next if same actorID? or take if activation <1s of each other?
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            ref var aoe0 = ref aoes[0];
            var actorId = aoe0.ActorID;
            List<AOEInstance> subset = [aoe0];

            for (var i = 1; i < count; i++)
            {
                if (subset.Count == max)
                    break;

                ref var aoe = ref aoes[i];
                if (aoe.ActorID != actorId)
                {
                    subset.Add(aoe);
                }
            }

            return CollectionsMarshal.AsSpan(subset);
        }

        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.DancingSwordCyclosword && animState1 == 1 && animState2 == 0)
        {
            AOEShape? shape = modelState switch
            {
                0 => new AOEShapeDonut(10f, 60f),
                4 => new AOEShapeDonut(15f, 60f),
                5 => new AOEShapeDonut(20f, 60f),
                6 => new AOEShapeCircle(10f),
                7 => new AOEShapeCircle(15f),
                31 => new AOEShapeCircle(20f),
                _ => null
            };

            if (shape == null)
            {
                return;
            }

            _cycloswords[actor.InstanceID] = shape;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Cyclosword)
        {
            if (_cycloswords.TryGetValue(actor.InstanceID, out var shape))
            {
                // activation time seems different for each cyclosword step
                // 1st -> 12.3d delay, 3.9d afterwards seems fine
                // 2nd -> 190.967 status -> 207.306 resolve -> 211.291 2nd resolve
                // 3rd -> 317.779 | 320.808 | 323.786 status -> 334.174 | 338.086 -> 338.141 | 342.117 -> 342.162 | 346.148 -> 16.4s | 17.3s | 18.35s
                // next cyclosword starts casting before previous ends; previous 2nd resolves just before next 1st
                var delay = MaxCasts switch
                {
                    1 => 12.3d,
                    3 => 16.3d,
                    2 => 16.3d + _aoes.Count * 0.5d,
                    _ => 0d
                };
                var activation = WorldState.CurrentTime.AddSeconds(delay);
                AOEShape second = shape is AOEShapeCircle circle ? new AOEShapeDonut(circle.Radius, 60f) : new AOEShapeCircle(((AOEShapeDonut)shape).InnerRadius);
                _aoes.Add(new(shape, actor.Position, default, activation, actorID: actor.InstanceID));
                _aoes.Add(new(second, actor.Position, default, activation.AddSeconds(3.9d), actorID: actor.InstanceID));
                SortHelpers.SortAOEByActivation(_aoes);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.SpinDonut10:
                case (uint)AID.SpinDonut15:
                case (uint)AID.SpinDonut20:
                case (uint)AID.SpinCircle10:
                case (uint)AID.SpinCircle15:
                case (uint)AID.SpinCircle20:
                    ++NumCasts;
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // TODO: during cycloswords2, move to safe area closest to where 3 AOEs overlap
        base.AddAIHints(slot, actor, assignment, hints);

        var aoes = ActiveAOEs(slot, actor);
        var count = aoes.Length;
        if (count != 0)
        {
            if (count == 2)
            {
                var aoe1 = aoes[1];
                AddGoalZone(aoe1, hints);
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    var aoe = aoes[i];
                    AddGoalZone(aoe, hints);
                }
            }
        }

        void AddGoalZone(AOEInstance aoe, AIHints hints)
        {
            var goalBuffer = 3f;

            if (aoe.Shape is AOEShapeCircle circle)
            {
                var radius = circle.Radius;
                hints.GoalZones.Add(AIHints.GoalDonut(aoe.Origin, radius, radius + goalBuffer));
            }
            else if (aoe.Shape is AOEShapeDonut donut)
            {
                var innerRadius = donut.InnerRadius;
                hints.GoalZones.Add(AIHints.GoalDonut(aoe.Origin, innerRadius - goalBuffer, innerRadius));
            }
        }
    }
}
