namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.CatoblepasPiece;

public enum OID : uint
{
    CatoblepasPiece = 0x4C9B,
    DemonicEyeCircle = 0x4C9C, // R1.500, x0 (spawn during fight)
    DemonicEyeDonut = 0x4C9D, // R1.500, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49682, // CatoblepasPiece->player, no cast, single-target
    BestialRoar = 48504, // CatoblepasPiece->self, 3.0s cast, range 60 circle

    Farburst = 48507, // 4C9D->self, no cast, single-target
    Farburst1 = 48508, // Helper->self, 0.5s cast, range 5-50 donut
    Nearburst = 48505, // 4C9C->self, no cast, single-target
    Nearburst1 = 48506, // Helper->self, 0.5s cast, range 25 circle
    ShiftingGaze = 48509, // CatoblepasPiece->self, 3.0s cast, single-target
    ShiftingGaze1 = 48954, // CatoblepasPiece->4C9D/4C9C, no cast, single-target
    FalseDemonEye = 48510, // Helper->self, no cast, range 100 circle, cast by tethered eye
    _Weaponskill_SinisterGleam = 48511, // CatoblepasPiece->self, 6.0s cast, single-target
    SinisterGleam = 48512, // Helper->self, 6.5s cast, range 60 180.000-degree cone

}

public enum SID : uint
{
    _Gen_ = 2056, // CatoblepasPiece->4C9D/4C9C, extra=0xAE
    _Gen_Petrification = 4891, // Helper->player, extra=0x0

}

public enum TetherID : uint
{
    _Gen_Tether_chn_ice_mouth01x = 195, // 4C9D/4C9C->CatoblepasPiece
}
sealed class BestialRoar(BossModule module) : Components.RaidwideCast(module, (uint)AID.BestialRoar);
sealed class SinisterGleam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SinisterGleam, new AOEShapeCone(60f, 90f.Degrees()));
sealed class DemonicEye(BossModule module) : Components.Voidzone(module, 2f, GetEyes, 2f)
{
    private static Actor[] GetEyes(BossModule module)
    {
        var eyes = module.Enemies((uint)OID.DemonicEyeCircle);
        eyes.AddRange(module.Enemies((uint)OID.DemonicEyeDonut));
        var count = eyes.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = eyes[i];
            if (z.Renderflags == 0)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
sealed class DemonicEyeCircle(BossModule module) : Components.Voidzone(module, 2f, module => module.Enemies((uint)OID.DemonicEyeCircle).Where(z => z.Renderflags == 0), 2f);
sealed class DemonicEyeDonut(BossModule module) : Components.Voidzone(module, 2f, module => module.Enemies((uint)OID.DemonicEyeDonut).Where(z => z.Renderflags == 0), 2f);
sealed class NearFarburst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<DemonicEye> _eyes = [];
    private readonly AOEShapeCircle _circle = new(25f);
    private readonly AOEShapeDonut _donut = new(5f, 50f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var count = aoes.Length;
        var max = count > 2 ? 2 : count;
        return aoes[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.DemonicEyeCircle or (uint)OID.DemonicEyeDonut)
        {
            var position = actor.Position;
            _eyes.Add(new(actor, position, actor.OID == (uint)OID.DemonicEyeCircle ? _circle : _donut));
        }
    }

    public override void Update()
    {
        // all eyes are spawned before 1st one starts moving
        // any better way to determine orb moving CW CCW?
        var count = _eyes.Count;
        for (var i = 0; i < count; i++)
        {
            var eye = _eyes[i];
            var start = eye.StartPosition;
            var cur = eye.Actor.Position;

            if (start.AlmostEqual(cur, 0.5f))
            {
                continue;
            }

            var activation = WorldState.CurrentTime.AddSeconds(19d);
            var startrot = (start - Arena.Center).ToAngle();
            var currot = (cur - Arena.Center).ToAngle();
            var ccw = startrot.DistanceToAngle(currot).Deg > 0f;

            var cardIntercard = Angle.AnglesCardinals.Concat(Angle.AnglesIntercardinals).ToArray();
            for (var j = 0; j < 8; j++)
            {
                var angle = cardIntercard[j];
                if (startrot.AlmostEqual(angle, 20f.Degrees().Rad))
                {
                    var finalPos = Arena.Center + (angle + 135f.Degrees() * (ccw ? 1f : -1f)).ToDirection() * 20f;
                    _aoes.Add(new(eye.Shape, finalPos, default, activation, actorID: eye.Actor.InstanceID));
                    _eyes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Nearburst1 or (uint)AID.Farburst1)
        {
            _aoes.RemoveAt(0);
        }
    }

    private class DemonicEye(Actor actor, WPos startPos, AOEShape shape)
    {
        public Actor Actor = actor;
        public WPos StartPosition = startPos;
        public AOEShape Shape = shape;
        public bool HasGaze = false;
    }
}

sealed class FalseDemonEye(BossModule module) : Components.GenericGaze(module, (uint)AID.FalseDemonEye)
{
    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => [];
}

sealed class CatoblepasPieceStates : StateMachineBuilder
{
    public CatoblepasPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BestialRoar>()
            //.ActivateOnEnter<DemonicEye>()
            .ActivateOnEnter<DemonicEyeCircle>()
            .ActivateOnEnter<DemonicEyeDonut>()
            .ActivateOnEnter<NearFarburst>()
            .ActivateOnEnter<FalseDemonEye>()
            .ActivateOnEnter<SinisterGleam>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy, PrimaryActorOID = (uint)OID.CatoblepasPiece, Contributors = "gynorhino", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14577u, SortOrder = 3)]
public sealed class CatoblepasPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f));
