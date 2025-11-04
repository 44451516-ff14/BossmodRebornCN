namespace BossMod.Endwalker.Savage.P6SHegemone;

class PteraIxou(BossModule module) : Components.CastCounter(module, (uint)AID.PteraIxouAOESnake) // doesn't matter which spell to track
{
    private BitMask _vulnSnake;
    private BitMask _vulnWing;

    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ForbiddenCenters(slot).Any(dir => _shape.Check(actor.Position, Arena.Center, dir)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var dir in ForbiddenCenters(pcSlot))
            _shape.Draw(Arena, Arena.Center, dir);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.GlossalResistanceDown:
                _vulnSnake.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.ChelicResistanceDown:
                _vulnWing.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.GlossalResistanceDown:
                _vulnSnake.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.ChelicResistanceDown:
                _vulnWing.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    private IEnumerable<Angle> ForbiddenCenters(int slot)
    {
        if (_vulnSnake[slot])
            yield return 90.Degrees();
        if (_vulnWing[slot])
            yield return -90.Degrees();
    }
}

class PteraIxouSpreadStack(BossModule module) : Components.CastStackSpread(module, (uint)AID.PteraIxouUnholyDarkness, (uint)AID.PteraIxouDarkSphere, 6, 10, 3);
