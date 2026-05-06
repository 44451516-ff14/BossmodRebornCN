namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

sealed class Meteorain(BossModule module) : Components.RaidwideCast(module, (uint)AID.Meteorain);

sealed class VacuumAOE(BossModule module) : Components.Voidzone(module, 7f, FindVacuums)
{
    private static Actor[] FindVacuums(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.VoidVacuum);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }
        var vacs = new Actor[count];
        var index = 0;
        for (var i = 0; i < enemies.Count; i++)
        {
            var z = enemies[i];
            // These appear at the start of the mechanic at Arena.Center and zoom out to their final locations, we only care about showing them while they're standing still and not in the Center. They die after exploding.
            if (z.Renderflags == 0 && !z.Position.AlmostEqual(module.Arena.Center, 2f) && z.LastFrameMovement == new WDir(0f, 0f))
            {
                vacs[index++] = z;
            }
        }
        return vacs[..index];
    }
}

sealed class VacuumArc1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilentTorrentArc1, new AOEShapeDonutSector(17f, 19f, 20f.Degrees()));

sealed class VacuumArc2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilentTorrentArc2, new AOEShapeDonutSector(17f, 19f, 30f.Degrees()));

sealed class VacuumArc3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilentTorrentArc3, new AOEShapeDonutSector(17f, 19f, 10f.Degrees()));
sealed class VacuumTelegraph(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SilentTorrentDash, (uint)AID.SilentTorrentDash2, (uint)AID.SilentTorrentDash3], new AOEShapeCircle(7f));

sealed class DeepFreezeFlares(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DeepFreeze, 10f);

sealed class DeepFreeze(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeepFreeze)
        {
            for (var i = 0; i < PlayerStates.Length; i++)
            {
                PlayerStates[i] = new(Requirement.Move, WorldState.FutureTime(spell.RemainingTime));
            }
        }
    }
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.FreezingUp)
        {
            PlayerStates[Raid.FindSlot(actor.InstanceID)] = new(Requirement.Move, WorldState.CurrentTime, finish: status.ExpireAt);
        }
    }
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.FreezingUp)
        {
            PlayerStates[Raid.FindSlot(actor.InstanceID)] = default;
        }
    }
}

sealed class LightlessWorld(BossModule module) : Components.RaidwideCast(module, (uint)AID.LightlessWorldCastbar);

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LoomingEmptinessKillzone)
        {
            Arena.Bounds = new ArenaBoundsCircle(40f); // encompasses the towers, haven't tested for a real border.
        }
    }
    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.BeaconInTheDark)
        {
            Arena.Bounds = new ArenaBoundsCircle(20f); // back to normal when the last add dies
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(Ex8EnuoStates),
ConfigType = null, // replace null with typeof(EnuoConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
IconIDType = typeof(IconID), // replace null with typeof(IconID) if applicable
PrimaryActorOID = (uint)OID.Enuo,
Contributors = "HerStolenLight",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Extreme,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1116u,
NameID = 14749u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Ex8Enuo(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    private Actor? _castingadd;
    private Actor? _breachadd;
    public Actor? CastingAdd() => _castingadd;
    public Actor? BeaconAdd() => _breachadd;

    protected override void UpdateModule()
    {
        _castingadd ??= GetActor((uint)OID.LoomingShadow);
        _breachadd ??= GetActor((uint)OID.BeaconInTheDark);
    }
}

