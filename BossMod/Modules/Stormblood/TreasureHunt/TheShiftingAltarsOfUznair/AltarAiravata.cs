namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarAiravata;

public enum OID : uint
{
    Boss = 0x2543, //R4.75
    AltarMatanga = 0x2545, // R3.42
    GoldWhisker = 0x2544, // R0.54
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // GoldWhisker->player, no cast, single-target
    AutoAttack2 = 872, // Boss/AltarMatanga->player, no cast, single-target

    Huff = 13371, // Boss->player, 3.0s cast, single-target
    HurlBoss = 13372, // Boss->location, 3.0s cast, range 6 circle
    Buffet = 13374, // Boss->player, 3.0s cast, single-target, knockback 20 forward
    SpinBoss = 13373, // Boss->self, 4.0s cast, range 30 120-degree cone
    BarbarousScream = 13375, // Boss->self, 3.5s cast, range 14 circle

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

public enum IconID : uint
{
    BuffetTarget = 23 // player
}

sealed class HurlBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HurlBoss, 6f);
sealed class SpinBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinBoss, new AOEShapeCone(30f, 60f.Degrees()));
sealed class BarbarousScream(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BarbarousScream, 14f);
sealed class Huff(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Huff);

sealed class Buffet(BossModule module) : Components.GenericKnockback(module, (uint)AID.Buffet, stopAtWall: true)
{
    private Actor? target;
    private DateTime activation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BuffetTarget)
        {
            target = actor;
            activation = WorldState.FutureTime(3.1d);
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (target != actor)
        {
            return [];
        }
        var pos = target.Position;
        return new Knockback[] { new(pos, 20f, activation, kind: Kind.DirForward, direction: (pos - Module.PrimaryActor.Position).ToAngle()) };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.Buffet)
        {
            target = null;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (target == actor)
        {
            hints.Add("Bait away!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (target == actor)
        {
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 17.5f));
        }
    }
}

sealed class Buffet2(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Buffet, new AOEShapeCone(30f, 60f.Degrees()), true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count != 0 && !IsBaitTarget(actor))
        {
            ref var b = ref CurrentBaits.Ref(0);
            var t = b.Target;
            var tPos = t.Position;
            var prim = Module.PrimaryActor;
            var mPos = prim.Position;
            var len = (tPos - mPos).Length();
            var max = t.HitboxRadius + prim.HitboxRadius;
            var min = len > max ? len : Math.Clamp(len, 0f, max);
            hints.AddForbiddenZone(b.Shape, tPos - min * prim.DirectionTo(t), b.Rotation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (IsBaitTarget(pc))
        {
            ref var b = ref CurrentBaits.Ref(0);
            var t = b.Target;
            var tPos = t.Position;
            var prim = Module.PrimaryActor;
            var mPos = prim.Position;
            var len = (tPos - mPos).Length();
            var max = t.HitboxRadius + prim.HitboxRadius;
            var min = len > max ? len : Math.Clamp(len, 0f, max);
            b.Shape.Outline(Arena, tPos - min * prim.DirectionTo(t), b.Rotation);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (CurrentBaits.Count != 0 && !IsBaitTarget(pc))
        {
            ref var b = ref CurrentBaits.Ref(0);
            var t = b.Target;
            var tPos = t.Position;
            var prim = Module.PrimaryActor;
            var mPos = prim.Position;
            var len = (tPos - mPos).Length();
            var max = t.HitboxRadius + prim.HitboxRadius;
            var min = len > max ? len : Math.Clamp(len, 0f, max);
            b.Shape.Draw(Arena, tPos - min * prim.DirectionTo(t), b.Rotation);
        }
    }
}

sealed class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
sealed class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);
sealed class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);

sealed class AltarAiravataStates : StateMachineBuilder
{
    public AltarAiravataStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HurlBoss>()
            .ActivateOnEnter<SpinBoss>()
            .ActivateOnEnter<BarbarousScream>()
            .ActivateOnEnter<Huff>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<Buffet2>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => AllDeadOrDestroyed(AltarAiravata.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7601)]
public sealed class AltarAiravata(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GoldWhisker, (uint)OID.AltarMatanga];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GoldWhisker => 2,
                (uint)OID.AltarMatanga => 1,
                _ => 0
            };
        }
    }
}
