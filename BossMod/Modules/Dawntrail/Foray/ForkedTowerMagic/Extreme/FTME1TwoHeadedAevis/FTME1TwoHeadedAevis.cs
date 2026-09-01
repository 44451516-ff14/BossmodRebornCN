namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME1TwoHeadedAevis;

[SkipLocalsInit]
sealed class FreezingFugue(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FreezingFugue1, (uint)AID.FreezingFugue2, (uint)AID.FreezingFugue3], 20f);
[SkipLocalsInit]
sealed class PoisonBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PoisonBreath, 18f);
[SkipLocalsInit]
sealed class FulgurousFugue(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FulgurousFugue1, (uint)AID.FulgurousFugue2, (uint)AID.FulgurousFugue3], new AOEShapeDonut(20f, 60f));
[SkipLocalsInit]
sealed class FreezingFulgurousFugue(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> Casters = [];
    private readonly AOEShapeCircle _circle = new(20f);
    private readonly AOEShapeDonut _donut = new(20f, 60f);

    public ReadOnlySpan<AOEInstance> ActiveCasters
    {
        get
        {
            var count = Casters.Count;
            var max = count > 1 ? 1 : count;
            return CollectionsMarshal.AsSpan(Casters)[..max];
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count > 1 ? 1 : count;
        var aoes = CollectionsMarshal.AsSpan(Casters);
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FreezingFugue1:
            case (uint)AID.FreezingFugue2:
            case (uint)AID.FreezingFugue3:
                Casters.Add(new(_circle, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: _circle.Distance(spell.LocXZ, spell.Rotation)));
                break;
            case (uint)AID.FulgurousFugue1:
            case (uint)AID.FulgurousFugue2:
            case (uint)AID.FulgurousFugue3:
                Casters.Add(new(_donut, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: _donut.Distance(spell.LocXZ, spell.Rotation)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FreezingFugue1:
            case (uint)AID.FreezingFugue2:
            case (uint)AID.FreezingFugue3:
            case (uint)AID.FulgurousFugue1:
            case (uint)AID.FulgurousFugue2:
            case (uint)AID.FulgurousFugue3:
                var count = Casters.Count;
                var id = caster.InstanceID;
                var aoes = CollectionsMarshal.AsSpan(Casters);
                for (var i = 0; i < count; ++i)
                {
                    if (aoes[i].ActorID == id)
                    {
                        Casters.RemoveAt(i);
                        return;
                    }
                }
                break;
        }
    }
}
[SkipLocalsInit]
sealed class ThunderfrostTempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThunderfrostTempest);
[SkipLocalsInit]
sealed class Archaeofury(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Tankbuster, default, 6f, 5f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var aid = spell.Action.ID;
        if (aid is (uint)AID.Archaeofury1 or (uint)AID.Archaeofury2)
        {
            var count = Spreads.Count;
            var id = spell.MainTargetID;
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                if (spreads[i].Target.InstanceID == id)
                {
                    Spreads.RemoveAt(i);
                    ++NumFinishedSpreads;
                    return;
                }
            }
            if (count != 0)
            {
                ++NumFinishedSpreads;
                Spreads.RemoveAt(0);
            }
        }
    }
}
[SkipLocalsInit]
sealed class TwoTerrorsWide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwoTerrors1, new AOEShapeRect(40f, 10f));
[SkipLocalsInit]
sealed class TwoTerrorsThin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwoTerrors2, new AOEShapeRect(40f, 5f));
[SkipLocalsInit]
sealed class ArcaneRevelation(BossModule module) : Components.GenericAOEs(module)
{
    // arcane revelation, dangerous squares based on which boss is glowing during cast
    // if during two terror, head casting the thicker attack is the glowing head
    // if during fugue, blue/green in freezing/fulgurous order

    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect _rect = new(30f, 2.5f, 30f);
    private readonly List<(WPos, Angle)> _green = [];
    private readonly List<(WPos, Angle)> _blue = [];

    public ReadOnlySpan<AOEInstance> ActiveCasters => CollectionsMarshal.AsSpan(_aoes);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _green.Count;
        if (count == 0)
        {
            return [];
        }

        var aoecount = _aoes.Count;
        var max = aoecount < count ? aoecount : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }
    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.ArcaneFontGreen:
                _green.Add((actor.Position, actor.Rotation));
                break;
            case (uint)OID.ArcaneFontBlue:
                _blue.Add((actor.Position, actor.Rotation));
                break;
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.ArcaneFontGreen:
                _green.Clear();
                break;
            case (uint)OID.ArcaneFontBlue:
                _blue.Clear();
                break;
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwoTerrors1:
                AddArcaneBeacon(spell.LocXZ.X < Arena.Center.X ? _blue : _green);
                break;
            case (uint)AID.FreezingFugue1:
            case (uint)AID.FreezingFugue2:
            case (uint)AID.FreezingFugue3:
                AddArcaneBeacon(_blue);
                break;
            case (uint)AID.FulgurousFugue1:
            case (uint)AID.FulgurousFugue2:
            case (uint)AID.FulgurousFugue3:
                AddArcaneBeacon(_green);
                break;
        }

        void AddArcaneBeacon(List<(WPos, Angle)> arcanes)
        {
            var actors = CollectionsMarshal.AsSpan(arcanes);
            var count = actors.Length;
            for (var i = 0; i < count; i++)
            {
                var arcane = actors[i];
                _aoes.Add(new(_rect, arcane.Item1, arcane.Item2));
            }
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.ArcaneBeacon1 or (uint)AID.ArcaneBeacon2)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(FTME1TwoHeadedAevisStates),
    ConfigType = null, // replace null with typeof(FTME1TwoHeadedAevisConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.TwoHeadedAevis,
    Contributors = "gynorhino",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.TheForkedTowerMagic,
    GroupID = 1114u,
    NameID = 14490u,
    SortOrder = 1,
    PlanLevel = 100)]
[SkipLocalsInit]
public sealed class FTME1TwoHeadedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-900f, 700f), new ArenaBoundsSquare(20f))
{
    private Actor? _greenHead;
    private Actor? _blueHead;
    private Actor? _green1;
    private Actor? _blue1;

    public Actor? GreenHead()
    {
        return _greenHead;
    }
    public Actor? BlueHead()
    {
        return _blueHead;
    }
    public Actor? Green1()
    {
        return _green1;
    }
    public Actor? Blue1()
    {
        return _blue1;
    }

    protected override void UpdateModule()
    {
        _greenHead ??= GetActor((uint)OID.GreenHead);
        _blueHead ??= GetActor((uint)OID.BlueHead);
        _green1 ??= GetActor((uint)OID.GreenHead1);
        _blue1 ??= GetActor((uint)OID.BlueHead1);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(_greenHead);
        Arena.Actor(_blueHead);
    }

    protected override bool CheckPull()
    {
        return PrimaryActor.InCombat && Raid.Player()!.Position.InSquare(Arena.Center, 20f);
    }
}
