namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P3Inception3Sacrament(BossModule module) : Components.GenericAOEs(module, (uint)AID.SacramentInception)
{
    private AOEInstance[] _aoe = [];
    private readonly Actor? boss = ((TEA)module).AlexPrime();
    private readonly Actor? heart = ((TEA)module).TrueHeart();
    private readonly AOEShapeCross cross = new(100f, 8f);
    private WPos initialPos;
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.Length == 0 && initialPos != default && initialPos != boss!.Position) // only relevant if one or more crystals didn't get destroyed
        {
            return new AOEInstance[1] { new(cross, boss.Position, boss.Rotation, activation) };
        }
        return _aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Inception) // only relevant if one or more crystals didn't get destroyed
        {
            initialPos = caster.Position;
            activation = Module.CastFinishAt(spell, 8.3d);
        }
    }

    public override void Update()
    {
        if (heart != null && _aoe.Length == 0)
        {
            var last = heart.LastFrameMovement;
            if (last != default)
            {
                ref var f = ref WorldState.Frame;
                if (last.Length() / f.Duration > 7f && heart.FindStatus((uint)SID.EnigmaCodexHeart) != null)
                {
                    _ = Intersect.RayCircleAnglesDeg(Arena.Center, 20f, heart.Position, last, out _, out var exit);
                    var a = new Angle(MathF.Round(exit / 22.5f) * 22.5f * Angle.DegToRad); // alexander will appear at one of the 8 arena slices
                    var pos = (Arena.Center + 21.5f * a.ToDirection()).Quantized();
                    var angle = a + 180f.Degrees();
                    _aoe = [new(cross, pos, angle, WorldState.FutureTime(6.9d), shapeDistance: cross.Distance(pos, angle))];
                }
            }
        }
    }
}

[SkipLocalsInit]
sealed class P3Inception3Debuffs(BossModule module) : Components.GenericStackSpread(module)
{
    private Actor? _sharedSentence;
    private BitMask _avoid;
    private BitMask _tethered;
    private bool _inited; // we init stuff on first update, since component is activated when statuses are already applied

    public override void Update()
    {
        if (!_inited)
        {
            _inited = true;
            if (_sharedSentence != null)
                Stacks.Add(new(_sharedSentence, 4f, 3, int.MaxValue, WorldState.FutureTime(4d), _avoid));
        }
        base.Update();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_tethered[slot] && FindPartner(slot) is var partner && partner != null && (partner.Position - actor.Position).LengthSq() < 900f)
            hints.Add("Stay farther from partner!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (_tethered[pcSlot] && FindPartner(pcSlot) is var partner && partner != null)
            Arena.AddLine(pc.Position, partner.Position, (partner.Position - pc.Position).LengthSq() < 900f ? default : Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.AggravatedAssault:
                _avoid.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.SharedSentence:
                _sharedSentence = actor;
                break;
            case (uint)SID.RestrainingOrder:
                _tethered.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    private Actor? FindPartner(int slot)
    {
        var remaining = _tethered;
        remaining[slot] = false;
        return remaining.Any() ? Raid[remaining.LowestSetBit()] : null;
    }
}

[SkipLocalsInit]
sealed class P3TrueHeart(BossModule module) : Components.Adds(module, (uint)OID.TrueHeart, AIHints.Enemy.PriorityPointless);
