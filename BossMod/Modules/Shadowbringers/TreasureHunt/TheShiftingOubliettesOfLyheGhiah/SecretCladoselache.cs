namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretCladoselache;

public enum OID : uint
{
    Boss = 0x3027, //R=2.47
    SecretShark = 0x3028, //R=3.0 
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // SecretShark->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target

    TidalGuillotine = 21704, // Boss->self, 4.0s cast, range 13 circle
    ProtolithicPuncture = 21703, // Boss->player, 4.0s cast, single-target

    PelagicCleaver = 21705, // Boss->self, 3.5s cast, range 40 60-degree cone
    PelagicCleaverFirst = 21706, // Boss->self, 5.0s cast, range 40 60-degree cone
    PelagicCleaverRest = 21707, // Boss->self, no cast, range 40 60-degree cone

    BiteAndRun = 21709, // SecretShark->player, 5.0s cast, width 5 rect charge
    AquaticLance = 21708, // Boss->player, 5.0s cast, range 8 circle

    Telega = 9630, // KeeperOfKeys->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768 // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
}

public enum IconID : uint
{
    RotateCCW = 168, // Boss
    RotateCW = 167 // Boss
}

class PelagicCleaverRotation(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;
    private static readonly AOEShapeCone _shape = new(40f, 30f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        _increment = iconID switch
        {
            (uint)IconID.RotateCW => -60f.Degrees(),
            (uint)IconID.RotateCCW => 60f.Degrees(),
            _ => default
        };
        InitIfReady(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PelagicCleaverFirst)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
            InitIfReady(caster);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PelagicCleaverFirst or (uint)AID.PelagicCleaverRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(_shape, source.Position.Quantized(), _rotation, _increment, _activation, 2.1f, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

class PelagicCleaver(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PelagicCleaver, new AOEShapeCone(40f, 30f.Degrees()));
class TidalGuillotine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TidalGuillotine, 13f);
class ProtolithicPuncture(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ProtolithicPuncture);
class BiteAndRun(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.BiteAndRun, 2.5f);
class AquaticLance(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AquaticLance, 8f);

class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

class SecretCladoselacheStates : StateMachineBuilder
{
    public SecretCladoselacheStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PelagicCleaver>()
            .ActivateOnEnter<PelagicCleaverRotation>()
            .ActivateOnEnter<TidalGuillotine>()
            .ActivateOnEnter<ProtolithicPuncture>()
            .ActivateOnEnter<BiteAndRun>()
            .ActivateOnEnter<AquaticLance>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(SecretCladoselache.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9778)]
public class SecretCladoselache(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.SecretShark, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SecretShark));
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
                (uint)OID.FuathTrickster => 3,
                (uint)OID.KeeperOfKeys => 2,
                (uint)OID.SecretShark => 1,
                _ => 0
            };
        }
    }
}
