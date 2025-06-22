namespace BossMod.RealmReborn.Raid.T04Gauntlet;

public enum OID : uint
{
    ClockworkBugP1 = 0x8EA, // R1.200, x6
    ClockworkBug = 0x7E0, // R1.200, spawn during fight
    ClockworkSoldier = 0x7E1, // R1.400, spawn during fight
    ClockworkKnight = 0x7E2, // R1.400, spawn during fight
    SpinnerRook = 0x7E3, // R0.500, spawn during fight
    ClockworkDreadnaught = 0x7E4, // R3.000, spawn during fight
    DriveCylinder = 0x1B2, // R0.500, x1
    TerminalEnd = 0x1E86FA,
    TerminalStart = 0x1E86F9
}

public enum AID : uint
{
    AutoAttack = 872, // ClockworkBugP1/ClockworkBug/ClockworkSoldier/ClockworkKnight/SpinnerRook/ClockworkDreadnaught->player, no cast, single-target

    Leech = 1230, // ClockworkBugP1/ClockworkBug->player, no cast, single-target
    HeadspinSoldier = 1231, // ClockworkSoldier->self, 0.5s cast, range 4+R circle cleave
    HeadspinKnight = 1233, // ClockworkKnight->self, 0.5s cast, range 4+R circle cleave
    Electromagnetism = 673, // ClockworkKnight->player, no cast, single-target attract
    BotRetrieval = 1239, // ClockworkDreadnaught->self, no cast, single-target, kills bug and buffs dreadnaught
    Rotoswipe = 1238, // ClockworkDreadnaught->self, no cast, range 8+R ?-degree cone cleave
    GravityThrust = 1236, // SpinnerRook->player, 1.5s cast, single-target damage (avoidable by moving to the back)
    Pox = 1237, // SpinnerRook->player, 3.0s cast, single-target debuff (avoidable by moving to the back)
    EmergencyOverride = 1258 // DriveCylinder->self, no cast, soft enrage raidwide
}

class Rotoswipe(BossModule module) : Components.Cleave(module, (uint)AID.Rotoswipe, new AOEShapeCone(11, 60.Degrees()), [(uint)OID.ClockworkDreadnaught]); // TODO: verify angle

class GravityThrustPox(BossModule module) : Components.GenericAOEs(module, default, "Move behind rook!")
{
    private static readonly AOEShape _shape = new AOEShapeRect(50, 50);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var rooks = ((T04Gauntlet)Module).Rooks;
        var count = 0;
        var countR = rooks.Count;

        if (countR == 0)
            return [];

        for (var i = 0; i < countR; ++i)
        {
            if (rooks[i].CastInfo?.TargetID == actor.InstanceID)
                ++count;
        }

        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;

        for (var i = 0; i < countR; ++i)
        {
            var c = rooks[i];
            if (c.CastInfo?.TargetID == actor.InstanceID)
                aoes[index++] = new(_shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
        }
        return aoes;
    }
}

class EmergencyOverride(BossModule module) : Components.CastCounter(module, (uint)AID.EmergencyOverride);

class T04GauntletStates : StateMachineBuilder
{
    private readonly T04Gauntlet _module;

    public T04GauntletStates(T04Gauntlet module) : base(module)
    {
        _module = module;
        SimplePhase(0, SinglePhase, "Gauntlet")
            .Raw.Update = () =>
            {
                var tStart = module.Enemies((uint)OID.TerminalStart);
                var tEnd = module.Enemies((uint)OID.TerminalEnd);
                var countS = tStart.Count;
                var countE = tEnd.Count;
                if (countS != 0)
                {
                    var t = tStart[0];
                    if (t.IsTargetable || t.IsDestroyed)
                        return true;
                }
                if (countE != 0)
                {
                    var t = tEnd[0];
                    if (t.IsTargetable || t.IsDestroyed)
                        return true;
                }
                return false;
            };
    }

    private void SinglePhase(uint id)
    {
        Condition(id + 0x00000, 61.1f, () =>
        {
            var countS = _module.Soldiers.Count;
            for (var i = 0; i < countS; ++i)
            {
                if (_module.Soldiers[i].IsTargetable)
                    return true;
            }
            var countK = _module.Knights.Count;
            for (var i = 0; i < countK; ++i)
            {
                if (_module.Knights[i].IsTargetable)
                    return true;
            }
            return false;
        }, "2x Soldiers + 2x Knights");

        Condition(id + 0x10000, 60.0f, () =>
        {
            var count = _module.Dreadnaughts.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_module.Dreadnaughts[i].IsTargetable)
                    return true;
            }
            return false;
        }, "Dreadnaught + 4x Bugs");

        Condition(id + 0x20000, 60.0f, () =>
        {
            var count = _module.Rooks.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_module.Rooks[i].IsTargetable)
                    return true;
            }
            return false;
        }, "2x Rooks + 4x Bugs")
            .ActivateOnEnter<Rotoswipe>();

        Condition(id + 0x30000, 60.0f, () =>
        {
            bool hasDreadnaught = false, hasSoldier = false, hasKnight = false;
            var countD = _module.Dreadnaughts.Count;
            var countS = _module.Soldiers.Count;
            var countK = _module.Knights.Count;
            for (var i = 0; i < countD; ++i)
            {
                var d = _module.Dreadnaughts[i];
                if (d.IsTargetable && !d.IsDead)
                    hasDreadnaught = true;
            }
            if (!hasDreadnaught)
                return false;
            for (var i = 0; i < countS; ++i)
            {
                var s = _module.Soldiers[i];
                if (s.IsTargetable && !s.IsDead)
                    hasSoldier = true;
            }
            if (!hasSoldier)
                return false;
            for (var i = 0; i < countK; ++i)
            {
                var k = _module.Knights[i];
                if (k.IsTargetable && !k.IsDead)
                    hasKnight = true;
            }
            return hasDreadnaught && hasSoldier && hasKnight;
        }, "Dreadnaught + Soldier + Knight")
            .ActivateOnEnter<GravityThrustPox>();

        Condition(id + 0x40000, 60.0f, () =>
        {
            bool hasDreadnaught = false, hasSoldier = false, hasKnight = false, hasBug = false;
            var countD = _module.Dreadnaughts.Count;
            var countS = _module.Soldiers.Count;
            var countK = _module.Knights.Count;
            var countB = _module.Bugs.Count;
            for (var i = 0; i < countD; ++i)
            {
                var d = _module.Dreadnaughts[i];
                if (d.IsTargetable && !d.IsDead)
                    hasDreadnaught = true;
            }
            if (!hasDreadnaught)
                return false;
            for (var i = 0; i < countS; ++i)
            {
                var s = _module.Soldiers[i];
                if (s.IsTargetable && !s.IsDead)
                    hasSoldier = true;
            }
            if (!hasSoldier)
                return false;
            for (var i = 0; i < countK; ++i)
            {
                var k = _module.Knights[i];
                if (k.IsTargetable && !k.IsDead)
                    hasKnight = true;
            }
            if (!hasKnight)
                return false;
            for (var i = 0; i < countB; ++i)
            {
                var b = _module.Bugs[i];
                if (b.IsTargetable && !b.IsDead)
                    hasBug = true;
            }
            return hasDreadnaught && hasSoldier && hasKnight && hasBug;
        }, "Dreadnaught + Soldier + Knight + Rook + 2x Bugs");

        ComponentCondition<EmergencyOverride>(id + 0x50000, 118.9f, comp => comp.NumCasts > 0, "Soft enrage")
            .ActivateOnEnter<EmergencyOverride>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.TerminalStart, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 96)]
public class T04Gauntlet : BossModule
{
    public readonly List<Actor> P1Bugs;
    public readonly List<Actor> Bugs;
    public readonly List<Actor> Soldiers;
    public readonly List<Actor> Knights;
    public readonly List<Actor> Rooks;
    public readonly List<Actor> Dreadnaughts;

    public T04Gauntlet(WorldState ws, Actor primary) : base(ws, primary, default, new ArenaBoundsCircle(25f))
    {
        P1Bugs = Enemies((uint)OID.ClockworkBugP1);
        Bugs = Enemies((uint)OID.ClockworkBug);
        Soldiers = Enemies((uint)OID.ClockworkSoldier);
        Knights = Enemies((uint)OID.ClockworkKnight);
        Rooks = Enemies((uint)OID.SpinnerRook);
        Dreadnaughts = Enemies((uint)OID.ClockworkDreadnaught);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // note: we don't bother checking for physical/magical defense on knights/soldiers and just have everyone aoe them down; magic reflect is very small
        // note: we try to kill dreadnaught first, since it's the only dangerous thing here
        // note: we try to offtank all bugs and not have dreadnaught eat them
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID == (uint)OID.ClockworkDreadnaught ? 2 : 1;
            e.AttackStrength = e.Actor.OID == (uint)OID.ClockworkDreadnaught ? 0.2f : 0.05f;
            e.ShouldBeTanked = assignment switch
            {
                PartyRolesConfig.Assignment.MT => e.Actor.OID != (uint)OID.ClockworkBug,
                PartyRolesConfig.Assignment.OT => e.Actor.OID == (uint)OID.ClockworkBug,
                _ => false
            };
        }
    }

    protected override bool CheckPull()
    {
        var tStart = Enemies((uint)OID.TerminalStart);
        var count = tStart.Count;
        if (count != 0)
        {
            var t = tStart[0];
            if (!t.IsTargetable)
                return true;
        }
        return false;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(P1Bugs);
        Arena.Actors(Bugs);
        Arena.Actors(Soldiers);
        Arena.Actors(Knights);
        Arena.Actors(Rooks);
        Arena.Actors(Dreadnaughts);
    }
}
