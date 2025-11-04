namespace BossMod.Shadowbringers.Hunt.RankS.ForgivenPedantry;

public enum OID : uint
{
    Boss = 0x298A, // R=5.5
}

public enum AID : uint
{
    SanctifiedScathe = 17439, // Boss->player, no cast, single-target

    LeftCheek1 = 17446, // Boss->self, 5.0s cast, range 60 180-degree cone
    LeftCheek2 = 17447, // Boss->self, no cast, range 60 180-degree cone
    RightCheek1 = 17448, // Boss->self, 5.0s cast, range 60 180-degree cone
    RightCheek2 = 17449, // Boss->self, no cast, range 60 180-degree cone
    TerrifyingGlance = 17955, // Boss->self, 3.0s cast, range 50 circle, gaze
    TheStake = 17443, // Boss->self, 4.0s cast, range 18 circle
    SecondCircle = 17441, // Boss->self, 3.0s cast, range 40 width 8 rect
    CleansingFire = 17442, // Boss->self, 4.0s cast, range 40 circle
    FeveredFlagellation = 17440, // Boss->players, 4.0s cast, range 15 120-degree cone, tankbuster
    SanctifiedShock = 17900, // Boss->player, no cast, single-target, stuns target before WitchHunt
    WitchHunt1 = 17444, // Boss->players, 3.0s cast, width 10 rect charge
    WitchHunt2 = 17445 // Boss->players, no cast, width 10 rect charge, targets main tank
}

sealed class LeftRightCheek(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCone cone = new(60f, 90f.Degrees());
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftCheek1 or (uint)AID.RightCheek1)
        {
            var pos = spell.LocXZ;
            var rot = spell.Rotation;
            var act = Module.CastFinishAt(spell);
            AddAOE(pos, rot, act);
            AddAOE(pos, rot + 180f.Degrees(), act.AddSeconds(3.1d), false);
            void AddAOE(WPos position, Angle rotation, DateTime activation, bool first = true)
                => _aoes.Add(new(cone, position, rotation, activation, first ? Colors.Danger : default, first, shapeDistance: cone.Distance(position, rotation)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var count = _aoes.Count;
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.LeftCheek1 or (uint)AID.RightCheek1 or (uint)AID.LeftCheek2 or (uint)AID.RightCheek2)
        {
            _aoes.RemoveAt(0);
            if (count == 2)
            {
                ref var aoe = ref _aoes.Ref(0);
                aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
        }
    }
}

sealed class TerrifyingGlance(BossModule module) : Components.CastGaze(module, (uint)AID.TerrifyingGlance);
sealed class TheStake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheStake, 18f);
sealed class SecondCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SecondCircle, new AOEShapeRect(40f, 4f));
sealed class CleansingFire(BossModule module) : Components.RaidwideCast(module, (uint)AID.CleansingFire);

sealed class FeveredFlagellation(BossModule module) : Components.BaitAwayCast(module, (uint)AID.FeveredFlagellation, new AOEShapeCone(15f, 60f.Degrees()), tankbuster: true, endsOnCastEvent: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class WitchHunt(BossModule module) : Components.GenericBaitAway(module)
{
    private bool witchHunt1done;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.SanctifiedShock && WorldState.Actors.Find(spell.MainTargetID) is Actor target)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeRect((target.Position - caster.Position).Length(), 5f)));
        }
        else if (id == (uint)AID.WitchHunt2)
        {
            CurrentBaits.Clear();
            witchHunt1done = false;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WitchHunt1 && WorldState.Actors.Find(spell.TargetID) is Actor target)
        {
            CurrentBaits.Clear();
            CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeRect((target.Position - caster.Position).Length(), 5f)));
        }
    }

    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return;
        }
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            var length = (b.Target.Position - b.Source.Position).Length();
            if (b.Shape is AOEShapeRect rect && rect.LengthFront != length)
            {
                b.Shape = new AOEShapeRect(length, 5f);
            }
        }

        if (witchHunt1done) // updating WitchHunt2 target incase of sudden tank swap
        {
            CurrentBaits.Ref(0).Target = WorldState.Actors.Find(Module.PrimaryActor.TargetID)!;
        }
    }
}

sealed class ForgivenPedantryStates : StateMachineBuilder
{
    public ForgivenPedantryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LeftRightCheek>()
            .ActivateOnEnter<TerrifyingGlance>()
            .ActivateOnEnter<TheStake>()
            .ActivateOnEnter<SecondCircle>()
            .ActivateOnEnter<CleansingFire>()
            .ActivateOnEnter<FeveredFlagellation>()
            .ActivateOnEnter<WitchHunt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8910)]
public sealed class ForgivenPedantry(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
