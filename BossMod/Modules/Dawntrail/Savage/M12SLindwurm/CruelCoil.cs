namespace BossMod.Dawntrail.Savage.M12SLindwurm;

sealed class CruelCoil(BossModule module) : BossComponent(module)
{
    // has mapeffect for 0x02 to 0x0A, with state 0x00010001 and 0x00040004
    // 0x0A 0x00010001 is a "reset", maybe boss still spinning around
    // 0x02-0x09 0x00010001 is the exit, with 0x03 = SE, 0x05 = SW, 0x07 = NW, 0x09 = NE?
    // savage uses cardinal exits
    private bool _active;
    private Angle _openingDir;
    private readonly AOEShapeDonut _skinsplitterDonut = new(8.75f, 14.25f); // donut AOE when coil rotates
    private static readonly AOEShapeCircle _centerTower = new(0.5f);
    private static readonly AOEShapeCircle _oppositeTower = new(1.75f);
    private static readonly float _oppositeTowerOffset = 7.25f;

    private readonly DateTime[] _betaExpiry = new DateTime[8];
    private readonly bool[] _hasUnbreakableFlesh = new bool[8];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0 || slot >= 8)
            return;

        if (status.ID == (uint)SID.BondsOfFleshBeta)
            _betaExpiry[slot] = status.ExpireAt;
        else if (status.ID == (uint)SID.UnbreakableFlesh1)
            _hasUnbreakableFlesh[slot] = true;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0 || slot >= 8)
            return;

        if (status.ID == (uint)SID.BondsOfFleshBeta)
            _betaExpiry[slot] = default;
        else if (status.ID == (uint)SID.UnbreakableFlesh1)
            _hasUnbreakableFlesh[slot] = false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Skinsplitter0)
        {
            _active = false;
            UpdateArena();
        }
        // constrictor ends the mechanic
        else if (spell.Action.ID == (uint)AID.Constrictor3)
        {
            _active = false;
            Arena.Bounds = M12SLindwurm.DefaultBounds;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // index 0x0A controls overall active state
        if (index == 0x0A)
        {
            if (state == 0x00010001u)
                _active = false;
            else if (state == 0x00040004u)
                _active = true;
        }

        // index 0x02-0x09 indicates opening direction (see the comment in the normal variant)
        if (index is >= 0x02 and <= 0x09 && state == 0x00010001u)
        {
            _active = true;
            UpdateArena(index);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_active)
            _skinsplitterDonut.Draw(Arena, Arena.Center);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // beta player tower indicators
        var betaExpiry = _betaExpiry[pcSlot];
        var hasUnbreakable = _hasUnbreakableFlesh[pcSlot];

        // when beta timer < 2.5s, draw small tower at center
        if (betaExpiry != default && (betaExpiry - WorldState.CurrentTime).TotalSeconds < 2.5f)
        {
            var isInside = _centerTower.Check(pc.Position, Arena.Center);
            var color = isInside ? Colors.Danger : Colors.Safe;
            _centerTower.Outline(Arena, Arena.Center, default, color);
        }
        // when player has Unbreakable Flesh and coil is active, draw tower at opposite end
        else if (hasUnbreakable && _active)
        {
            var towerPos = Arena.Center + _oppositeTowerOffset * _openingDir.ToDirection();
            var isInside = _oppositeTower.Check(pc.Position, towerPos);
            var color = isInside ? Colors.Danger : Colors.Safe;
            _oppositeTower.Outline(Arena, towerPos, default, color);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_active && _skinsplitterDonut.Check(actor.Position, Arena.Center))
            hints.Add("Get out of the snake coil!");

        var betaExpiry = _betaExpiry[slot];
        var hasUnbreakable = _hasUnbreakableFlesh[slot];

        if (betaExpiry != default && (betaExpiry - WorldState.CurrentTime).TotalSeconds < 2.5f)
        {
            if (!_centerTower.Check(actor.Position, Arena.Center))
                hints.Add("Go to center!");
        }
        else if (hasUnbreakable && _active)
        {
            var towerPos = Arena.Center + _oppositeTowerOffset * _openingDir.ToDirection();
            if (!_oppositeTower.Check(actor.Position, towerPos))
                hints.Add("Go to opposite tower!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_active)
            hints.AddForbiddenZone(_skinsplitterDonut, Arena.Center);
    }

    private void UpdateArena(int openingIndex = 0)
    {
        _openingDir = (openingIndex == 0 ? 0f : (openingIndex - 0x02) * -45f).Degrees();
        var openingHalfAngle = (openingIndex == 0 ? 180f : 160f).Degrees();
        Arena.Bounds = new ArenaBoundsCustom(
            [new Rectangle(Arena.Center, 20f, 15f)],
            [new DonutSegmentV(Arena.Center, 9.5f, 13.5f, _openingDir, openingHalfAngle, 128)]);
    }
}
