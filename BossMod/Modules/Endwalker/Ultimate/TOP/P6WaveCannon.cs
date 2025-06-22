namespace BossMod.Endwalker.Ultimate.TOP;

class P6WaveCannonPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.P6WaveCannonPuddle, 6);

class P6WaveCannonExaflare(BossModule module) : Components.Exaflare(module, 8f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.P6WaveCannonExaflareFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 8f * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.1f, ExplosionsLeft = 7, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.P6WaveCannonExaflareFirst or (uint)AID.P6WaveCannonExaflareRest)
        {
            ++NumCasts;
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}

class P6WaveCannonProteans(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect _shape = new(100, 4);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.P6WaveCannonProtean)
            foreach (var p in Raid.WithoutSlot(true, true, true))
                CurrentBaits.Add(new(caster, p, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.P6WaveCannonProteanAOE)
        {
            ++NumCasts;
            if (spell.Targets.Count == 1)
                CurrentBaits.RemoveAll(b => b.Target.InstanceID == spell.Targets[0].ID);
        }
    }
}

class P6WaveCannonWildCharge(BossModule module) : Components.GenericWildCharge(module, 4f, (uint)AID.P6WaveCannonWildCharge, 100)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.P6WaveCannonProtean)
        {
            Source = caster;
            // TODO: find out how it selects target...
            var targetAssigned = false;
            foreach (var (i, p) in Raid.WithSlot(true, true, true))
            {
                PlayerRoles[i] = p.Role == Role.Tank ? PlayerRole.Share : targetAssigned ? PlayerRole.ShareNotFirst : PlayerRole.Target;
                targetAssigned |= PlayerRoles[i] == PlayerRole.Target;
            }
        }
    }
}
