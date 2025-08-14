namespace BossMod.Endwalker.Savage.P12S1Athena;

// TODO: generalize (line stack/spread)
sealed class EngravementOfSouls2Lines(BossModule module) : BossComponent(module)
{
    public int NumCasts;
    private Actor? _lightRay;
    private Actor? _darkRay;
    private BitMask _lightCamp;
    private BitMask _darkCamp;

    private static readonly AOEShapeRect _shape = new(100f, 3f);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (InAOE(_lightRay, actor) != _darkCamp[slot])
        {
            hints.Add(_darkCamp[slot] ? "Go to dark camp" : "GTFO from dark camp");
        }
        if (InAOE(_darkRay, actor) != _lightCamp[slot])
        {
            hints.Add(_lightCamp[slot] ? "Go to light camp" : "GTFO from light camp");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return player == _lightRay || player == _darkRay ? PlayerPriority.Interesting : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        DrawOutline(_lightRay, _darkCamp[pcSlot]);
        DrawOutline(_darkRay, _lightCamp[pcSlot]);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.UmbralTilt:
            case (uint)SID.UmbralbrightSoul:
                _lightCamp[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.AstralTilt:
            case (uint)SID.AstralbrightSoul:
                _darkCamp[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.UmbralstrongSoul:
                _lightRay = actor;
                _darkCamp[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.AstralstrongSoul:
                _darkRay = actor;
                _lightCamp[Raid.FindSlot(actor.InstanceID)] = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.UmbralImpact or (uint)AID.AstralImpact)
            ++NumCasts;
    }

    private bool InAOE(Actor? target, Actor actor) => target != null && _shape.Check(actor.Position, Module.PrimaryActor.Position, Angle.FromDirection(target.Position - Module.PrimaryActor.Position));

    private void DrawOutline(Actor? target, bool safe)
    {
        if (target != null)
        {
            _shape.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(target.Position - Module.PrimaryActor.Position), safe ? Colors.Safe : Colors.Danger);
        }
    }
}

sealed class EngrameventOfSouls2Spread(BossModule module) : Components.GenericStackSpread(module, true, false)
{
    public int NumCasts;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var radius = status.ID switch
        {
            (uint)SID.UmbralbrightSoul or (uint)SID.AstralbrightSoul => 3f,
            (uint)SID.HeavensflameSoul => 6f,
            _ => default
        };
        if (radius != default)
        {
            Spreads.Add(new(actor, radius)); // TODO: activation
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var radius = spell.Action.ID switch
        {
            (uint)AID.UmbralGlow or (uint)AID.AstralGlow => 3f,
            (uint)AID.TheosHoly => 6f,
            _ => default
        };
        if (radius != default)
        {
            Spreads.RemoveAll(s => s.Radius == radius);
            ++NumCasts;
        }
    }
}
