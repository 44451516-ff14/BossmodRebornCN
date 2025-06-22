namespace BossMod.Endwalker.Dungeon.D10FellCourtOfTroia.D101EvilDreamers;

public enum OID : uint
{
    Boss = 0x3988, // R2.4-7.2
    EvilDreamer1 = 0x3966, // R2.4-4.8
    EvilDreamer2 = 0x3967, // R2.4
    EvilDreamer3 = 0x3968, // R3.6
}

public enum AID : uint
{
    AutoAttack = 30246, // EvilDreamer1/EvilDreamer2/EvilDreamer3/Boss->player, no cast, single-target
    Teleport1 = 29629, // EvilDreamer1->location, no cast, single-target
    Teleport2 = 29623, // EvilDreamer1->location, no cast, single-target

    UniteMare1 = 29621, // EvilDreamer1->self, 11.0s cast, range 6 circle
    UniteMare2 = 29622, // EvilDreamer1->self, 11.0s cast, range 18 circle
    UniteMare3 = 29628, // EvilDreamer3->self, 10.0s cast, range 12 circle
    DarkVision1 = 29624, // EvilDreamer1->self, 8.0s cast, range 40 width 5 rect
    DarkVision2 = 29627, // EvilDreamer1->self, 15.0s cast, range 41 width 5 rect
    VoidGravity = 29626, // EvilDreamer1->player, 6.0s cast, range 6 circle, spread
    EndlessNightmare = 29630, // Boss->self, 60.0s cast, range 20 circle, enrage
    EndlessNightmareRepeat = 29728 // Boss->self, no cast, range 20 circle
}

sealed class UniteMare1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UniteMare1, 6f, riskyWithSecondsLeft: 2.5d); // delay to improve melee greeding during enrage
sealed class UniteMare2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UniteMare2, 18f, riskyWithSecondsLeft: 5d);

sealed class UniteMare3(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(12f);
    private readonly List<AOEInstance> _aoes = new(7);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var dreamers = Module.Enemies((uint)OID.EvilDreamer3);
        var count = dreamers.Count;
        if (count == 0)
            return [];
        for (var i = 0; i < count; ++i)
        {
            if (dreamers[i].IsDead)
                return CollectionsMarshal.AsSpan(_aoes);
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UniteMare3)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UniteMare3)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
sealed class DarkVision1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkVision1, new AOEShapeRect(40f, 2.5f));
sealed class DarkVision2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkVision2, new AOEShapeRect(41f, 2.5f));
sealed class VoidGravity(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.VoidGravity, 6f);
sealed class EndlessNightmare(BossModule module) : Components.CastHint(module, (uint)AID.EndlessNightmare, "Enrage!", true);

sealed class D101EvilDreamersStates : StateMachineBuilder
{
    public D101EvilDreamersStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<UniteMare1>()
            .ActivateOnEnter<UniteMare2>()
            .ActivateOnEnter<UniteMare3>()
            .ActivateOnEnter<DarkVision1>()
            .ActivateOnEnter<DarkVision2>()
            .ActivateOnEnter<VoidGravity>()
            .ActivateOnEnter<EndlessNightmare>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11382, SortOrder = 2)]
public sealed class D101EvilDreamers(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(168, 89.999f), 19.5f * CosPI.Pi32th, 32)], [new Rectangle(new(168f, 110.25f), 20f, 1.25f),
    new Rectangle(new(188.34f, 90.007f), 1.25f, 20f)]);
    private static readonly uint[] dreamers = [(uint)OID.Boss, (uint)OID.EvilDreamer1, (uint)OID.EvilDreamer2, (uint)OID.EvilDreamer3];

    protected override bool CheckPull()
    {
        var enemies = Enemies((uint)OID.EvilDreamer1);
        var count = enemies.Count;
        for (var i = 0; i < count; ++i)
        {
            var enemy = enemies[i];
            if (enemy.InCombat)
                return true;
        }
        return false;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(dreamers));
    }
}
