namespace BossMod.Endwalker.Dungeon.D05Aitiascope.D053Amon;

public enum OID : uint
{
    Boss = 0x346E, // R=16.98
    YsaylesSpirit = 0x346F, // R2.0
    Antistrophe = 0x1EB26D, // R0.5
    Ice = 0x3470, // R6.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 24712, // Boss->player, no cast, single-target

    Antistrophe = 25694, // Boss->self, 3.0s cast, single-target
    DarkForte = 25700, // Boss->player, 5.0s cast, single-targe, tankbuster
    Entracte = 25701, // Boss->self, 5.0s cast, range 40 circle, raidwide

    DreamsOfIce = 27756, // Helper->self, 14.7s cast, range 6 circle, knockback 13, dir forward, summons ice to hide behind
    CurtainCall = 25702, // Boss->self, 32.0s cast, range 40 circle, line of sight AOE

    Epode = 25695, // Helper->self, 8.0s cast, range 70 width 12 rect

    EruptionForteVisual = 24709, // Boss->self, 3.0s cast, single-target
    EruptionForte = 25704, // Helper->location, 4.0s cast, range 8 circle, baited AOE

    LeftFiragaForte = 25697, // Boss->self, 7.0s cast, range 40 width 20 rect
    RightFiragaForte = 25696, // Boss->self, 7.0s cast, range 40 width 20 rect

    Strophe = 25693, // Boss->self, 3.0s cast, single-target

    ThundagaForte1 = 25690, // Boss->location, 5.0s cast, range 40 circle, damage fall off AoE
    ThundagaForte2 = 25691, // Helper->self, 5.0s cast, range 20 45-degree cone
    ThundagaForte3 = 25692, // Helper->self, 11.0s cast, range 20 45-degree cone

    Visual = 25703 // YsaylesSpirit->self, no cast, single-target
}

class CurtainCallArenaChange(BossModule module) : BossComponent(module)
{
    private static readonly Shape[] circle = [new Polygon(new(11, -490), 6.4f / MathF.Cos(MathF.PI / 20), 20, 9.Degrees())];
    public static readonly ArenaBoundsComplex CurtaincallArena = new(D053Amon.union, [.. D053Amon.difference, .. circle]);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x05)
        {
            if (state == 0x00020001)
                Arena.Bounds = CurtaincallArena;
            else if (state == 0x00080004)
                Arena.Bounds = D053Amon.arena;
        }
    }
}

class Epode(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Epode), new AOEShapeRect(35, 6, 35));
class EruptionForte(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.EruptionForte), 8);
class LeftFiragaForte(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftFiragaForte), new AOEShapeRect(40, 40, DirectionOffset: 90.Degrees()));
class RightFiragaForte(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightFiragaForte), new AOEShapeRect(40, 40, DirectionOffset: -90.Degrees()));
class ThundagaForte1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ThundagaForte1), 15);
class DarkForte(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.DarkForte));
class Entracte(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Entracte));
class DreamsOfIce(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DreamsOfIce), new AOEShapeCircle(6));

class CurtainCall(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.CurtainCall), 60)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Ice);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Arena.Bounds == CurtainCallArenaChange.CurtaincallArena ? Safezones : [];
    }
}

class ThundagaForte2(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Strophe))
{
    private readonly List<Actor> _castersThundagaForte2 = [];
    private readonly List<Actor> _castersThundagaForte3 = [];

    private static readonly AOEShape _cone = new AOEShapeCone(20, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersThundagaForte2.Count > 0
            ? _castersThundagaForte2.Select(c => new AOEInstance(_cone, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)))
            : _castersThundagaForte3.Select(c => new AOEInstance(_cone, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.ThundagaForte2 => _castersThundagaForte2,
        AID.ThundagaForte3 => _castersThundagaForte3,
        _ => null
    };
}

class D053AmonStates : StateMachineBuilder
{
    public D053AmonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CurtainCall>()
            .ActivateOnEnter<CurtainCallArenaChange>()
            .ActivateOnEnter<DreamsOfIce>()
            .ActivateOnEnter<Epode>()
            .ActivateOnEnter<EruptionForte>()
            .ActivateOnEnter<LeftFiragaForte>()
            .ActivateOnEnter<RightFiragaForte>()
            .ActivateOnEnter<ThundagaForte1>()
            .ActivateOnEnter<ThundagaForte2>()
            .ActivateOnEnter<DarkForte>()
            .ActivateOnEnter<Entracte>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 786, NameID = 10293)]
public class D053Amon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly Polygon[] union = [new Polygon(new(11, -490), 19.5f, 48)];
    public static readonly Shape[] difference = [new Rectangle(new(11, -469), 20, 1.75f)];
    public static readonly ArenaBoundsComplex arena = new(union, difference);
}
