namespace BossMod.Endwalker.VariantCriterion.C03AAI.C030Trash1;

abstract class Water(BossModule module, uint aid) : Components.StackWithCastTargets(module, aid, 8f, 4, 4);
class NWater(BossModule module) : Water(module, (uint)AID.NWater);
class SWater(BossModule module) : Water(module, (uint)AID.SWater);

class BubbleShowerCrabDribble(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private readonly AOEShapeCone _shape1 = new(9f, 45f.Degrees());
    private readonly AOEShapeCone _shape2 = new(6f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NBubbleShower or (uint)AID.SBubbleShower)
        {
            _aoes.Clear();
            var pos = spell.LocXZ;
            _aoes.Add(new(_shape1, pos, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(_shape2, pos, spell.Rotation + 180f.Degrees(), Module.CastFinishAt(spell, 3.6d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.NBubbleShower or (uint)AID.SBubbleShower or (uint)AID.NCrabDribble or (uint)AID.SCrabDribble)
        {
            _aoes.RemoveAt(0);
        }
    }
}

abstract class C030SnipperStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C030SnipperStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<NWater>(!_savage)
            .ActivateOnEnter<SWater>(_savage)
            .ActivateOnEnter<BubbleShowerCrabDribble>()
            .ActivateOnEnter<NTailScrew>(!_savage) // note: first mob is often pulled together with second one
            .ActivateOnEnter<STailScrew>(_savage)
            .ActivateOnEnter<Twister>();
    }

    private void SinglePhase(uint id)
    {
        Water(id, 7.7f);
        BubbleShowerCrabDribble(id + 0x10000u, 2.1f);
        Water(id + 0x20000u, 11.3f);
        BubbleShowerCrabDribble(id + 0x30000u, 2.1f);
        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void Water(uint id, float delay)
    {
        Cast(id, _savage ? AID.SWater : AID.NWater, delay, 5f, "Stack");
    }

    private void BubbleShowerCrabDribble(uint id, float delay)
    {
        Cast(id, _savage ? AID.SBubbleShower : AID.NBubbleShower, delay, 5f, "Cleave front");
        Cast(id + 0x10u, _savage ? AID.SCrabDribble : AID.NCrabDribble, 2.1f, 1.5f, "Cleave back");
    }
}

sealed class C030NSnipperStates(BossModule module) : C030SnipperStates(module, false);
sealed class C030SSnipperStates(BossModule module) : C030SnipperStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NSnipper, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979u, NameID = 12537u, SortOrder = 2)]
public sealed class C030NSnipper(WorldState ws, Actor primary) : C030Trash1(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.NCrab));
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SSnipper, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980u, NameID = 12537u, SortOrder = 2)]
public sealed class C030SSnipper(WorldState ws, Actor primary) : C030Trash1(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SCrab));
    }
}
