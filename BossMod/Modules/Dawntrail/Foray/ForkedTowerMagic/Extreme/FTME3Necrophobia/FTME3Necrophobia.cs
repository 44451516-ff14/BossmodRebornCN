namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME3Necrophobia;

[SkipLocalsInit]
// does it always have 2 instances of each damage eventcast? or is it only when there are 24+ players in tower?
sealed class HailOfHellflares(BossModule module) : Components.RaidwideCast(module, (uint)AID.HailOfHellflares, "Raidwide x5");
[SkipLocalsInit]
sealed class FireIII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AncientFire1, (uint)AID.AncientFire2, (uint)AID.SeveredFire], 18f);
[SkipLocalsInit]
sealed class BlizzardIII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AncientBlizzard1, (uint)AID.AncientBlizzard2, (uint)AID.SeveredBlizzard], new AOEShapeCross(45f, 7.5f));
[SkipLocalsInit]
sealed class ThunderIII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AncientThunder1, (uint)AID.AncientThunder2, (uint)AID.SeveredThunder], new AOEShapeCone(60f, 22.5f.Degrees()));
[SkipLocalsInit]
sealed class DeathlyRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathlyRay, new AOEShapeRect(30f, 3f));
[SkipLocalsInit]
sealed class VacuumWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VacuumWave, new AOEShapeCone(30f, 90f.Degrees()));
[SkipLocalsInit]
sealed class CorpseMangler(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CorpseMangler);
[SkipLocalsInit]
sealed class FertileGroundRaidwide(BossModule module) : Components.CastCounter(module, (uint)AID.FertileGround);
[SkipLocalsInit]
sealed class SpellProcession(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // preposition towards middle
        hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 5f));
    }
}
[SkipLocalsInit]
sealed class DeathShroud(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle _circle = new(18f);
    private readonly AOEShapeCross _cross = new(45f, 7.5f);
    private readonly AOEShapeCone _cone = new(60f, 22.5f.Degrees());
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AncientFire1:
            case (uint)AID.AncientFire2:
            case (uint)AID.SeveredFire:
                _aoes.Add(new(_circle, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), default, true, caster.InstanceID, _circle.Distance(spell.LocXZ, spell.Rotation)));
                break;
            case (uint)AID.AncientBlizzard1:
            case (uint)AID.AncientBlizzard2:
            case (uint)AID.SeveredBlizzard:
                _aoes.Add(new(_cross, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), default, true, caster.InstanceID, _cross.Distance(spell.LocXZ, spell.Rotation)));
                break;
            case (uint)AID.AncientThunder1:
            case (uint)AID.AncientThunder2:
            case (uint)AID.SeveredThunder:
                _aoes.Add(new(_cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), default, true, caster.InstanceID, _cone.Distance(spell.LocXZ, spell.Rotation)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.AncientFire1:
                case (uint)AID.AncientFire2:
                case (uint)AID.SeveredFire:
                case (uint)AID.AncientBlizzard1:
                case (uint)AID.AncientBlizzard2:
                case (uint)AID.SeveredBlizzard:
                case (uint)AID.AncientThunder1:
                case (uint)AID.AncientThunder2:
                case (uint)AID.SeveredThunder:
                    ++NumCasts;
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }
}
[SkipLocalsInit]
sealed class DarkCurrent(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect _rect = new(60f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count == 5 ? 3 : count > 3 ? 4 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        var isFourAOEs = max == 4;
        var isThreeAOEs = max == 3;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];

            var shouldBeDanger = isFourAOEs && i < 2 || isThreeAOEs && i == 0;
            var shouldBeRisky = shouldBeDanger || max == 2 && i < 2;

            if (shouldBeDanger)
                aoe.Color = Colors.Danger;

            if (shouldBeRisky)
                aoe.Risky = true;
        }

        return aoes;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarkCurrent1)
        {
            //2.1s between casts
            var act = Module.CastFinishAt(spell);
            var position = spell.LocXZ;
            var rotation = spell.Rotation;
            var dir = rotation.ToDirection().OrthoL().Normalized();
            var distance = 10f;
            _aoes.Add(new(_rect, position, rotation, act, risky: true));

            for (var i = 1; i <= 2; i++)
            {
                _aoes.Add(new(_rect, position + i * distance * dir, rotation, act.AddSeconds(2.1d * i), risky: false));
                _aoes.Add(new(_rect, position + i * distance * dir * -1f, rotation, act.AddSeconds(2.1d * i), risky: false));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.DarkCurrent1:
                case (uint)AID.DarkCurrent2:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // stay near initial cast to move in after
        if (_aoes.Count == 5)
        {
            ref var aoe = ref _aoes.Ref(0);
            hints.GoalZones.Add(AIHints.GoalRectangle(aoe.Origin, aoe.Rotation.ToDirection(), 7f, 60f, 100f));
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(FTME3NecrophobiaStates),
ConfigType = null, // replace null with typeof(NecrophobiaConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
IconIDType = typeof(IconID), // replace null with typeof(IconID) if applicable
PrimaryActorOID = (uint)OID.Necrophobia,
Contributors = "gynorhino",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Foray,
GroupType = BossModuleInfo.GroupType.TheForkedTowerMagic,
GroupID = 1114u,
NameID = 14503u,
SortOrder = 3,
PlanLevel = 100)]
[SkipLocalsInit]
public sealed class FTME3Necrophobia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 800f), new ArenaBoundsCircle(24f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 24f);
}
