namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

// common AI for all phases
sealed class Ex2GarudaAI(BossModule module) : BossComponent(module)
{
    private readonly AerialBlast? _aerialBlast = module.FindComponent<AerialBlast>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.StayAtLongRange = true;
            switch (e.Actor.OID)
            {
                case (uint)OID.Boss:
                    e.Priority = 1;
                    e.AttackStrength = 0.2f;
                    if (_aerialBlast?.NumCasts > 0)
                    {
                        e.DesiredRotation = 135f.Degrees();
                        e.DesiredPosition = Arena.Center + 18f * e.DesiredRotation.Value.ToDirection();
                    }
                    else
                    {
                        e.DesiredRotation = 180f.Degrees();
                        e.DesiredPosition = Arena.Center + 8f * e.DesiredRotation.Value.ToDirection();
                    }
                    break;
                case (uint)OID.Chirada:
                    e.Priority = 2;
                    e.AttackStrength = 0.15f;
                    break;
                case (uint)OID.Suparna:
                    e.Priority = assignment != PartyRolesConfig.Assignment.MT ? 3 : 0;
                    e.AttackStrength = 0.15f;
                    e.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.OT;
                    e.DesiredRotation = (_aerialBlast?.NumCasts > 0 ? -45f : 0f).Degrees();
                    e.DesiredPosition = Arena.Center + 18f * e.DesiredRotation.Value.ToDirection();
                    break;
                case (uint)OID.RazorPlume:
                    e.Priority = assignment != PartyRolesConfig.Assignment.MT ? 4 : 0;
                    e.AttackStrength = 0f;
                    e.ShouldBeTanked = false;
                    break;
                case (uint)OID.SatinPlume:
                    e.Priority = assignment != PartyRolesConfig.Assignment.MT ? 5 : 0;
                    e.AttackStrength = 0f;
                    e.ShouldBeTanked = false;
                    break;
                case (uint)OID.SpinyPlume:
                    e.Priority = Module.PrimaryActor.IsTargetable ? AIHints.Enemy.PriorityPointless : 6;
                    e.AttackStrength = 0f;
                    e.ShouldBeTanked = false;
                    if (actor.Role == Role.Tank && e.Actor.TargetID != actor.InstanceID && (WorldState.Actors.Find(e.Actor.TargetID)?.FindStatus((uint)SID.ThermalLow)?.Extra ?? 0) >= 2)
                    {
                        e.Priority = 6;
                        e.ShouldBeTanked = e.PreferProvoking = true;
                    }
                    break;
            }
        }

        // don't stand near monoliths to avoid clipping them with friction
        var haveMonoliths = false;
        foreach (var monolith in Module.Enemies((uint)OID.Monolith).Where(a => !a.IsDead))
        {
            hints.AddForbiddenZone(new SDCircle(monolith.Position, 5f));
            haveMonoliths = true;
        }

        if (haveMonoliths && actor.Role is Role.Healer or Role.Ranged)
        {
            // have ranged stay in center to avoid los issues
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 7f), DateTime.MaxValue);
        }
    }
}
