namespace BossMod.Endwalker.Savage.P1SErichthonios;

// state related to [aether]flails mechanics
sealed class Flails : BossComponent
{
    public int NumCasts;
    private AOEShape? _first;
    private AOEShape? _second;
    private bool _detectSecond;
    private bool _showSecond;

    private readonly AOEShape _aoeLeft = new AOEShapeCone(60f, 135f.Degrees(), 90f.Degrees());
    private readonly AOEShape _aoeRight = new AOEShapeCone(60f, 135f.Degrees(), -90f.Degrees());
    private readonly AOEShape _aoeInner = new AOEShapeCircle(P1S.InnerCircleRadius);
    private readonly AOEShape _aoeOuter = new AOEShapeDonut(P1S.InnerCircleRadius, 60f);

    public Flails(BossModule module) : base(module)
    {
        (_first, _second) = (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) switch
        {
            (uint)AID.GaolerFlailRL => (_aoeRight, _aoeLeft),
            (uint)AID.GaolerFlailLR => (_aoeLeft, _aoeRight),
            (uint)AID.GaolerFlailIO1 => (_aoeInner, _aoeOuter),
            (uint)AID.GaolerFlailIO2 => (_aoeInner, _aoeOuter),
            (uint)AID.GaolerFlailOI1 => (_aoeOuter, _aoeInner),
            (uint)AID.GaolerFlailOI2 => (_aoeOuter, _aoeInner),
            (uint)AID.AetherflailRX => (_aoeRight, null),
            (uint)AID.AetherflailLX => (_aoeLeft, null),
            (uint)AID.AetherflailIL => (_aoeInner, _aoeLeft),
            (uint)AID.AetherflailIR => (_aoeInner, _aoeRight),
            (uint)AID.AetherflailOL => (_aoeOuter, _aoeLeft),
            (uint)AID.AetherflailOR => (_aoeOuter, _aoeRight),
            _ => (null, null)
        };

        if (_first == null)
            ReportError("Failed to detect flail zones");

        _detectSecond = _first != null && _second == null;
        _showSecond = (_first is AOEShapeCone) != (_second is AOEShapeCone);
    }

    public override void Update()
    {
        // currently the best way i've found to determine secondary aetherflail attack if first attack is a cone is to watch spawned npcs
        // these can appear few frames later...
        if (!_detectSecond)
            return;

        var weaponsBall = Module.Enemies((uint)OID.FlailI);
        var weaponsChakram = Module.Enemies((uint)OID.FlailO);
        if (weaponsBall.Count + weaponsChakram.Count > 0)
        {
            _detectSecond = false;
            if (weaponsBall.Count > 0 && weaponsChakram.Count > 0)
            {
                ReportError($"Failed to determine second aetherflail: there are {weaponsBall.Count} balls and {weaponsChakram.Count} chakrams");
            }
            else
            {
                _second = weaponsBall.Count > 0 ? _aoeInner : _aoeOuter;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_first?.Check(actor.Position, Module.PrimaryActor) ?? false)
            hints.Add("Hit by first flail!");
        if (_showSecond && _second != null && _second.Check(actor.Position, Module.PrimaryActor))
            hints.Add("Hit by second flail!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _first?.Draw(Arena, Module.PrimaryActor);
        if (_showSecond)
            _second?.Draw(Arena, Module.PrimaryActor);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.GaolerFlailR1:
            case (uint)AID.GaolerFlailL1:
            case (uint)AID.GaolerFlailI1:
            case (uint)AID.GaolerFlailO1:
                ++NumCasts;
                _first = null;
                _showSecond = true;
                break;
            case (uint)AID.GaolerFlailR2:
            case (uint)AID.GaolerFlailL2:
            case (uint)AID.GaolerFlailI2:
            case (uint)AID.GaolerFlailO2:
                ++NumCasts;
                _second = null;
                break;
        }
    }
}
