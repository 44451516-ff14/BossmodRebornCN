namespace BossMod.Components;

// generic unavoidable raidwide, started and finished by a single cast
public class RaidwideCast(BossModule module, uint aid, string hint = "Raidwide") : CastHint(module, aid, hint)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        for (var i = 0; i < count; ++i)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Module.CastFinishAt(Casters[i].CastInfo));
    }
}

public class RaidwideCasts(BossModule module, uint[] aids, string hint = "Raidwide") : RaidwideCast(module, default, hint)
{
    private readonly uint[] AIDs = aids;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                Casters.Add(caster);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                Casters.Remove(caster);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                ++NumCasts;
            }
        }
    }
}

// generic unavoidable raidwide, initiated by a custom condition and applied by an instant cast after a delay
public class RaidwideInstant(BossModule module, uint aid, float delay = default, string hint = "Raidwide") : CastCounter(module, aid)
{
    public readonly float Delay = delay;
    public readonly string Hint = hint;
    public DateTime Activation; // default if inactive, otherwise expected cast time

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Activation != default && Hint.Length > 0)
            hints.Add(Hint);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != default)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            Activation = default;
        }
    }
}

// generic unavoidable instant raidwide initiated by a cast (usually visual-only)
public class RaidwideCastDelay(BossModule module, uint actionVisual, uint actionAOE, float delay, string hint = "Raidwide") : RaidwideInstant(module, actionAOE, delay, hint)
{
    public uint ActionVisual = actionVisual;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == ActionVisual)
            Activation = Module.CastFinishAt(spell, Delay);
    }
}

// generic unavoidable instant raidwide cast initiated by NPC yell
public class RaidwideAfterNPCYell(BossModule module, uint aid, uint npcYellID, float delay, string hint = "Raidwide") : RaidwideInstant(module, aid, delay, hint)
{
    public uint NPCYellID = npcYellID;

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (id == NPCYellID)
            Activation = WorldState.FutureTime(Delay);
    }
}

// generic unavoidable single-target damage, started and finished by a single cast (typically tankbuster, but not necessary)
public class SingleTargetCast(BossModule module, uint aid, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : CastHint(module, aid, hint)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Casters[i];
            if (c.CastInfo != null)
            {
                var target = c.CastInfo.TargetID != c.InstanceID ? c.CastInfo.TargetID : c.TargetID; // assume self-targeted casts actually hit main target
                hints.AddPredictedDamage(new BitMask().WithBit(Raid.FindSlot(target)), Module.CastFinishAt(c.CastInfo), damageType);
            }
        }
    }
}

public class SingleTargetCasts(BossModule module, uint[] aids, string hint = "Tankbuster") : SingleTargetCast(module, default, hint)
{
    private readonly uint[] AIDs = aids;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                Casters.Add(caster);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                Casters.Remove(caster);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                ++NumCasts;
            }
        }
    }
}

// generic unavoidable single-target damage, initiated by a custom condition and applied by an instant cast after a delay
public class SingleTargetInstant(BossModule module, uint aid, float delay = 0f, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : CastCounter(module, aid)
{
    public readonly float Delay = delay; // delay from visual cast end to cast event
    public readonly string Hint = hint;
    public readonly List<(int slot, DateTime activation, ulong instanceID)> Targets = [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Targets.Count != 0 && Hint.Length != 0)
            hints.Add(Hint);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in Targets)
            hints.AddPredictedDamage(new BitMask().WithBit(t.slot), t.activation, damageType);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            Targets.RemoveAll(t => t.instanceID == spell.MainTargetID);
        }
    }
}

// generic unavoidable instant single-target damage initiated by a cast (usually visual-only)
public class SingleTargetCastDelay(BossModule module, uint actionVisual, uint actionAOE, float delay, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : SingleTargetInstant(module, actionAOE, delay, hint, damageType)
{
    public uint ActionVisual = actionVisual;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == ActionVisual)
        {
            var target = spell.TargetID != caster.InstanceID ? spell.TargetID : caster.TargetID; // assume self-targeted casts actually hit main target
            Targets.Add((Raid.FindSlot(target), Module.CastFinishAt(spell, Delay), target));
        }
    }
}

// generic unavoidable instant single-target damage initiated by a cast (usually visual-only)
public class SingleTargetEventDelay(BossModule module, uint actionVisual, uint actionAOE, float delay, string hint = "Tankbuster") : SingleTargetInstant(module, actionAOE, delay, hint)
{
    public uint ActionVisual = actionVisual;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == ActionVisual)
        {
            var target = spell.MainTargetID != caster.InstanceID ? spell.MainTargetID : caster.TargetID; // assume self-targeted casts actually hit main target
            Targets.Add((Raid.FindSlot(target), WorldState.FutureTime(Delay), target));
        }
    }
}

// generic unavoidable single-target damage, started and finished by a single cast, that can be delayed by moving out of range (typically tankbuster, but not necessary)
public class SingleTargetDelayableCast(BossModule module, uint aid, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster) : SingleTargetCastDelay(module, aid, aid, default, hint, damageType);
