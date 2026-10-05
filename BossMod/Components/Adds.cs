namespace BossMod.Components;

// generic component used for drawing adds
public abstract class Adds(BossModule module, uint oid, int priority = 0, bool forbidDots = false, bool allowUntargetable = false) : BossComponent(module)
{
    public readonly List<Actor> Actors = module.Enemies(oid);
    private readonly bool AllowUntargetable = allowUntargetable;

    public List<Actor> ActiveActors
    {
        get
        {
            var count = Actors.Count;
            var activeActors = new List<Actor>(count);
            for (var i = 0; i < count; ++i)
            {
                var actor = Actors[i];
                if ((AllowUntargetable || actor.IsTargetable) && !actor.IsDead)
                {
                    activeActors.Add(actor);
                }
            }
            return activeActors;
        }
    }

    public int ActiveActorsCount
    {
        get
        {
            var count = Actors.Count;
            var active = 0;
            for (var i = 0; i < count; ++i)
            {
                var actor = Actors[i];
                if ((AllowUntargetable || actor.IsTargetable) && !actor.IsDead)
                {
                    ++active;
                }
            }
            return active;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) => hints.PrioritizeTargetsByOIDAndForbidDOTs(oid, priority, forbidDots);

    public override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(Actors);
}

// component for adds that shouldn't be targeted at all, but should still be drawn
public abstract class AddsPointless(BossModule module, uint oid, bool allowUntargetable = false) : Adds(module, oid, allowUntargetable: allowUntargetable)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Actors.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.SetPriority(Actors[i], AIHints.Enemy.PriorityPointless);
        }
    }
}

// generic component used for drawing multiple adds with multiple oids, when it's not useful to distinguish between them
public abstract class AddsMulti(BossModule module, uint[] oids, int priority = 0, bool allowUntargetable = false) : BossComponent(module)
{
    public readonly uint[] OIDs = oids;
    private readonly bool AllowUntargetable = allowUntargetable;

    public List<Actor> ActiveActors
    {
        get
        {
            var len = OIDs.Length;
            var active = 0;
            for (var j = 0; j < len; ++j)
            {
                var actors = Module.Enemies(OIDs[j]);
                var count = actors.Count;
                for (var i = 0; i < count; ++i)
                {
                    var actor = actors[i];
                    if ((AllowUntargetable || actor.IsTargetable) && !actor.IsDead)
                    {
                        ++active;
                    }
                }
            }
            List<Actor> activeActors = [with(active)];
            for (var j = 0; j < len; ++j)
            {
                var actors = Module.Enemies(OIDs[j]);
                var count = actors.Count;
                for (var i = 0; i < count; ++i)
                {
                    var actor = actors[i];
                    if ((AllowUntargetable || actor.IsTargetable) && !actor.IsDead)
                    {
                        activeActors.Add(actor);
                    }
                }
            }
            return activeActors;
        }
    }

    public int ActiveActorsCount
    {
        get
        {
            var len = OIDs.Length;
            var active = 0;
            for (var j = 0; j < len; ++j)
            {
                var actors = Module.Enemies(OIDs[j]);
                var count = actors.Count;
                for (var i = 0; i < count; ++i)
                {
                    var actor = actors[i];
                    if ((AllowUntargetable || actor.IsTargetable) && !actor.IsDead)
                    {
                        ++active;
                    }
                }
            }
            return active;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (priority != 0)
        {
            hints.PrioritizeTargetsByOID(OIDs, priority);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(Module, OIDs, allowDeadAndUntargetable: AllowUntargetable);
}
