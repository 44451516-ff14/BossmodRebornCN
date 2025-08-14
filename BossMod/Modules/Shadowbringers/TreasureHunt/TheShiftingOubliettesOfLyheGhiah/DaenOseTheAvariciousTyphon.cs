namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.DaenOseTheAvariciousTyphon;

public enum OID : uint
{
    Boss = 0x301A, // R0.75-4.5
    WrigglingMenace = 0x3024, // R1.8
    LingeringSnort = 0x301B, // R0.8
    DaenOseTheAvaricious1 = 0x3082, // R1.0, TODO: rotation ccw?
    DaenOseTheAvaricious2 = 0x3051, // R1.0, TODO: rotation helper?
    DaenOseTheAvaricious3 = 0x3050, // R1.0, TODO: rotation cw?
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss/WrigglingMenace->player, no cast, single-target
    AutoAttack2 = 872, // Mandragoras->player, no cast, single-target

    AChoo = 21689, // Boss->self, 3.0s cast, range 12 90-degree cone

    UnpleasantBreezeVisual = 21696, // Boss->self, 3.0s cast, single-target
    UnpleasantBreeze = 21697, // Helper->location, 3.0s cast, range 6 circle
    StoutSnort = 21687, // Boss->self, 4.0s cast, range 40 circle, raidwide

    VisualLingeringSnort = 21694, // Boss->self, 3.0s cast, single-target
    LingeringSnort = 21695, // Helper->self, 6.5s cast, range 50 circle, damage fall off aoe
    SnortsaultKB = 21690, // Boss->self, 6.5s cast, range 40 circle, knockback 20, away from source
    SnortsaultCircle = 21782, // Boss->self, no cast, range 5 circle
    SnortsaultCone = 21781, // Helper->self, no cast, range 20 45-degree cone
    SnortAssaultEnd = 21691, // Boss->self, no cast, range 40 circle

    FellSwipe = 21771, // WrigglingMenace->self, 3.0s cast, range 8 120-degree cone
    WindShot = 21772, // LingeringSnort->self, 3.0s cast, range 40 width 6 rect
    Fireball = 21688, // Boss->players, 5.0s cast, range 6 circle, stack

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

class AChoo(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AChoo, new AOEShapeCone(12f, 45f.Degrees()));
class FellSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FellSwipe, new AOEShapeCone(8f, 60f.Degrees()));
class WindShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindShot, new AOEShapeRect(40f, 3f));
class LingeringSnort(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LingeringSnort, 20f);
class UnpleasantBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnpleasantBreeze, 6f);
class Fireball(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Fireball, 6f, 8, 8);

class SnortsaultKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SnortsaultKB, 20f, stopAtWall: true);
class SnortsaultCircle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly LingeringSnort _aoes = module.FindComponent<LingeringSnort>()!;
    private static readonly AOEShapeCircle circle = new(5f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.ActiveCasters.Length == 0 && _aoe != null)
            return new AOEInstance[1] { _aoe.Value };
        return [];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DaenOseTheAvaricious2)
            _aoe = new(circle, Arena.Center.Quantized(), default, WorldState.FutureTime(14.3d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SnortAssaultEnd)
            _aoe = null;
    }
}

class Snortsault(BossModule module) : Components.GenericRotatingAOE(module)
{
    private readonly LingeringSnort _aoe = module.FindComponent<LingeringSnort>()!;
    private static readonly Angle increment = 6f.Degrees();
    private static readonly AOEShapeCone cone = new(20f, 22.5f.Degrees());

    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.DaenOseTheAvaricious3:
                AddSequences(false);
                break;
            case (uint)OID.DaenOseTheAvaricious1:
                AddSequences(true);
                break;
        }
        void AddSequences(bool isClockwise)
        {
            var rotationIncrement = isClockwise ? increment : -increment;
            AddSequence(default);
            AddSequence(180f.Degrees());
            void AddSequence(Angle offset) => Sequences.Add(new(cone, Arena.Center.Quantized(), actor.Rotation + offset, rotationIncrement, WorldState.FutureTime(14.5d), 1.1f, 31, 9));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SnortsaultCone)
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.ActiveCasters.Length == 0)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.ActiveCasters.Length == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_aoe.ActiveCasters.Length == 0)
            base.DrawArenaBackground(pcSlot, pc);
    }
}

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class DaenOseTheAvariciousTyphonStates : StateMachineBuilder
{
    public DaenOseTheAvariciousTyphonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AChoo>()
            .ActivateOnEnter<FellSwipe>()
            .ActivateOnEnter<WindShot>()
            .ActivateOnEnter<UnpleasantBreeze>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<LingeringSnort>()
            .ActivateOnEnter<SnortsaultCircle>()
            .ActivateOnEnter<Snortsault>()
            .ActivateOnEnter<SnortsaultKB>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(DaenOseTheAvariciousTyphon.All);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9808, SortOrder = 1)]
public class DaenOseTheAvariciousTyphon(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.WrigglingMenace, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.WrigglingMenace));
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.SecretOnion => 6,
                (uint)OID.SecretEgg => 5,
                (uint)OID.SecretGarlic => 4,
                (uint)OID.SecretTomato => 3,
                (uint)OID.SecretQueen => 2,
                (uint)OID.WrigglingMenace => 1,
                _ => 0
            };
        }
    }
}
