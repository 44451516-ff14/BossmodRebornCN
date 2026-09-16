namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P3SeventhUmbralEra(BossModule module) : Components.GenericKnockback(module, (uint)AID.SeventhUmbralEra)
{
    private readonly DateTime _activation = module.WorldState.FutureTime(5.3d);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        return new Knockback[1] { new(Arena.Center, 11f, _activation, ignoreImmunes: true) };
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(new SDPrecisePosition(new WPos(0f, 9f), new(0f, 1f), 0.5f, actor.Position, 0.1f), _activation);
    }
}

sealed class P3CalamitousFlame(BossModule module) : Components.CastCounter(module, (uint)AID.CalamitousFlame);
sealed class P3CalamitousBlaze(BossModule module) : Components.CastCounter(module, (uint)AID.CalamitousBlaze);

class P3BahamutMoon(BossModule module) : Components.Voidzone(module, 8f, GetVoidzones)
{
    bool _knockbackHappened;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SeventhUmbralEra)
        {
            _knockbackHappened = true;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_knockbackHappened)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, Sources(Module).Any() ? 10f : 6f));
        }
    }

    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.BahamutMoon);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}

sealed class P3BahamutPositioning(UCOB module) : BossComponent(module)
{
    public WPos? DesiredPosition;
    public Angle? DesiredRotation;
    private readonly Actor _bahamut = module.BahamutPrime()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (hints.FindEnemy(_bahamut) is AIHints.Enemy b)
        {
            b.DesiredRotation ??= DesiredRotation;
            b.DesiredPosition ??= DesiredPosition;
        }
    }
}
