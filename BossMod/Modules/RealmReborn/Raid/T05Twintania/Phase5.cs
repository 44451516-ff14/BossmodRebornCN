namespace BossMod.RealmReborn.Raid.T05Twintania;

// what happens here is marker appears -> 5 liquid hells drop at (0.6 + 1.7*N)s; each liquid hell cast does small damage and spawns voidzone 1.2s later
class P5LiquidHell(BossModule module) : Components.VoidzoneAtCastTarget(module, 6, (uint)AID.LiquidHellBoss, m => m.Enemies(OID.LiquidHell).Where(z => z.EventState != 7), 1.5f)
{
    public Actor? Target;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        switch (spell.Action.ID)
        {
            case (uint)AID.LiquidHellMarker:
                Target = WorldState.Actors.Find(spell.MainTargetID);
                break;
            case (uint)AID.LiquidHellBoss:
                if (NumCasts % 5 == 0)
                    Target = null;
                break;
        }
    }
}

class P5Hatch(BossModule module) : BossComponent(module)
{
    public Actor? Target;
    public readonly List<Actor> Orbs = module.Enemies((uint)OID.Oviform);
    public readonly List<Actor> Neurolinks = module.Enemies((uint)OID.Neurolink);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == Target)
            hints.Add("Go to neurolink!", !Neurolinks.InRadius(actor.Position, T05Twintania.NeurolinkRadius).Any());
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return player == Target ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Target != null)
            foreach (var orb in Orbs)
                Arena.AddLine(orb.Position, Target.Position, Colors.Danger);
        foreach (var neurolink in Neurolinks)
            Arena.AddCircle(neurolink.Position, T05Twintania.NeurolinkRadius, Target == pc ? Colors.Safe : Colors.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HatchMarker:
                Target = WorldState.Actors.Find(spell.MainTargetID);
                break;
            case (uint)AID.Hatch:
                Target = null;
                break;
        }
    }
}

class P5AI(BossModule module) : BossComponent(module)
{
    private readonly DeathSentence? _deathSentence = module.FindComponent<DeathSentence>();
    private readonly P5LiquidHell? _liquidHell = module.FindComponent<P5LiquidHell>();
    private readonly P5Hatch? _hatch = module.FindComponent<P5Hatch>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidNeurolinks = true; // stay out of neurolinks by default
        if (_hatch?.Target != null)
        {
            // see if there is anyone intercepting orb in a neurolink
            var neurolinkUnderBoss = _hatch.Neurolinks.FirstOrDefault(n => n.Position.InCircle(Module.PrimaryActor.Position, 1));
            // note: i've used to have extra logic if orb is being intercepted: in such case neither target would move anywhere nor others would give space
            // however, it's a bit finicky - instead, it's safer to just let everyone move, and if orb ends up being intercepted - oh well...
            //var orbIntercepted = neurolinkUnderBoss != null && Raid.WithoutSlot(false, true, true).InRadius(neurolinkUnderBoss.Position, T05Twintania.NeurolinkRadius).Any();
            if (actor == _hatch.Target)
            {
                // hatch target should run to safe neurolink (except for neurolink under boss, this is unsafe) if orb is not being intercepted
                //if (!orbIntercepted)
                {
                    forbidNeurolinks = false;
                    var forbidden = new List<Func<WPos, float>>();
                    for (var i = 0; i < _hatch.Neurolinks.Count; ++i)
                    {
                        var neurolink = _hatch.Neurolinks[i];
                        if (neurolink != neurolinkUnderBoss)
                            forbidden.Add(ShapeDistance.Circle(neurolink.Position, T05Twintania.NeurolinkRadius));
                    }
                    hints.AddForbiddenZone(ShapeDistance.InvertedUnion(forbidden));
                }
            }
            else if (assignment == ((_deathSentence?.TankedByOT ?? false) ? PartyRolesConfig.Assignment.MT : PartyRolesConfig.Assignment.OT) && neurolinkUnderBoss != null && actor != _liquidHell?.Target)
            {
                // current offtank should try to intercept orb by standing in a neurolink under boss, unless it is covered by liquid hells or tank is baiting liquid hells away
                var neurolinkUnsafe = _liquidHell != null && _liquidHell.Sources(Module).Any(z => neurolinkUnderBoss.Position.InCircle(z.Position, _liquidHell.Shape.Radius));
                if (!neurolinkUnsafe)
                {
                    forbidNeurolinks = false;
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(neurolinkUnderBoss.Position, T05Twintania.NeurolinkRadius));
                }
            }
            else //if (!orbIntercepted)
            {
                // everyone else should gtfo from orb path
                foreach (var orb in _hatch.Orbs)
                    hints.AddForbiddenZone(ShapeDistance.Rect(orb.Position, _hatch.Target.Position, 2));
                // also avoid predicted movement path
                var closestNeurolink = _hatch.Neurolinks.Exclude(neurolinkUnderBoss).Closest(_hatch.Target.Position);
                if (closestNeurolink != null)
                    hints.AddForbiddenZone(ShapeDistance.Rect(_hatch.Target.Position, closestNeurolink.Position, 2));
            }
        }

        if (forbidNeurolinks && _hatch != null)
            foreach (var neurolink in _hatch.Neurolinks)
                hints.AddForbiddenZone(ShapeDistance.Circle(neurolink.Position, 5));

        if (actor == _liquidHell?.Target)
        {
            // liquid hell target should gtfo from raid
            foreach (var p in Raid.WithoutSlot(false, true, true).Exclude(actor))
                hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, _liquidHell.Shape.Radius));
        }
    }
}
