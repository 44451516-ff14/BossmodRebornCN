namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P1JagdDolls(BossModule module) : BossComponent(module)
{
    public int NumExhausts;
    private readonly List<Actor> _dolls = module.Enemies((uint)OID.JagdDoll);
    private readonly HashSet<ulong> _exhaustsDone = [];

    private const float _exhaustRadius = 8.8f;

    private IEnumerable<Actor> ActiveDolls => _dolls.Where(d => d.IsTargetable && !d.IsDead);
    public bool Active => ActiveDolls.Any();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumExhausts < 2 && ActiveDolls.InRadius(actor.Position, _exhaustRadius).Count() > 1)
        {
            hints.Add("GTFO from exhaust intersection");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in hints.PotentialTargets.Where(t => t.Actor.OID == (uint)OID.JagdDoll))
            t.ForbidDOTs = true;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var doll in ActiveDolls)
        {
            Arena.Actor(doll, doll.HPMP.CurHP < doll.HPMP.MaxHP / 4u ? default : Colors.Vulnerable);

            var tether = WorldState.Actors.Find(doll.Tether.Target);
            if (tether != null)
            {
                Arena.AddLine(doll.Position, tether.Position);
            }

            if (NumExhausts < 2)
            {
                Arena.AddCircle(doll.Position, _exhaustRadius, Colors.Safe);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Exhaust && NumExhausts < 2)
        {
            if (!_exhaustsDone.Contains(caster.InstanceID))
            {
                NumExhausts = 1;
                _exhaustsDone.Add(caster.InstanceID);
            }
            else
            {
                NumExhausts = 2;
            }
        }
    }
}
