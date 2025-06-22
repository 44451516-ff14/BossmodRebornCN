namespace BossMod.Endwalker.VariantCriterion.C02AMR.C020Trash2;

public enum OID : uint
{
    NKotengu = 0x3F98, // R1.500
    NOnmitsugashira = 0x3F99, // R1.360, x1
    NYamabiko = 0x3FA1, // R0.800, x6

    SKotengu = 0x3F9E, // R1.500
    SOnmitsugashira = 0x3F9F, // R1.360, x1
    SYamabiko = 0x3FA3, // R0.800, x6
}

public enum AID : uint
{
    AutoAttack = 31318, // *Kotengu/*Onmitsugashira->player, no cast, single-target
    // kotengu
    NBackwardBlows = 34396, // NKotengu->self, 4.0s cast, single-target, visual (front-back cleave)
    NLeftwardBlows = 34397, // NKotengu->self, 4.0s cast, single-target, visual (front-left cleave)
    NRightwardBlows = 34398, // NKotengu->self, 4.0s cast, single-target, visual (front-right cleave)
    NBladeOfTheTengu = 34399, // NKotengu->self, no cast, range 50 ?-degree cone
    NWrathOfTheTengu = 34400, // NKotengu->self, 4.0s cast, range 40 circle, raidwide with bleed
    NGazeOfTheTengu = 34401, // NKotengu->self, 4.0s cast, range 60 circle, gaze
    SBackwardBlows = 34414, // SKotengu->self, 4.0s cast, single-target, visual (front-back cleave)
    SLeftwardBlows = 34415, // SKotengu->self, 4.0s cast, single-target, visual (front-left cleave)
    SRightwardBlows = 34416, // SKotengu->self, 4.0s cast, single-target, visual (front-right cleave)
    SBladeOfTheTengu = 34417, // SKotengu->self, no cast, range 50 ?-degree cone
    SWrathOfTheTengu = 34418, // SKotengu->self, 4.0s cast, range 40 circle, raidwide with bleed
    SGazeOfTheTengu = 34419, // SKotengu->self, 4.0s cast, range 60 circle, gaze
    // onmitsugashira
    NIssen = 34402, // NOnmitsugashira->player, 4.0s cast, single-target, tankbuster
    NHuton = 34403, // NOnmitsugashira->self, 4.0s cast, single-target, speed up buff
    NJujiShuriken = 34404, // NOnmitsugashira->self, 3.0s cast, range 40 width 3 rect
    NJujiShurikenFast = 34429, // NOnmitsugashira->self, 1.5s cast, range 40 width 3 rect
    SIssen = 34420, // SOnmitsugashira->player, 4.0s cast, single-target, tankbuster
    SHuton = 34421, // SOnmitsugashira->self, 4.0s cast, single-target, speed up buff
    SJujiShuriken = 34422, // SOnmitsugashira->self, 3.0s cast, range 40 width 3 rect
    SJujiShurikenFast = 34430, // SOnmitsugashira->self, 1.5s cast, range 40 width 3 rect
    // yamabiko
    NMountainBreeze = 34439, // NYamabiko->self, 6.0s cast, range 40 width 8 rect
    SMountainBreeze = 34442, // SYamabiko->self, 6.0s cast, range 40 width 8 rect
}

class BladeOfTheTengu(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);

    private static readonly AOEShapeCone _shape = new(50f, 45f.Degrees()); // TODO: verify angle

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var secondAngle = spell.Action.ID switch
        {
            (uint)AID.NBackwardBlows or (uint)AID.SBackwardBlows => 180f.Degrees(),
            (uint)AID.NLeftwardBlows or (uint)AID.SLeftwardBlows => 90f.Degrees(),
            (uint)AID.NRightwardBlows or (uint)AID.SRightwardBlows => -90f.Degrees(),
            _ => default
        };
        if (secondAngle != default)
        {
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 0.1f)));
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation + secondAngle, Module.CastFinishAt(spell, 1.9f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.NBladeOfTheTengu or (uint)AID.SBladeOfTheTengu)
            _aoes.RemoveAt(0);
    }
}

abstract class WrathOfTheTengu(BossModule module, uint aid) : Components.RaidwideCast(module, aid, "Raidwide with bleed");
class NWrathOfTheTengu(BossModule module) : WrathOfTheTengu(module, (uint)AID.NWrathOfTheTengu);
class SWrathOfTheTengu(BossModule module) : WrathOfTheTengu(module, (uint)AID.SWrathOfTheTengu);

abstract class GazeOfTheTengu(BossModule module, uint aid) : Components.CastGaze(module, aid);
class NGazeOfTheTengu(BossModule module) : GazeOfTheTengu(module, (uint)AID.NGazeOfTheTengu);
class SGazeOfTheTengu(BossModule module) : GazeOfTheTengu(module, (uint)AID.SGazeOfTheTengu);

abstract class MountainBreeze(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(40f, 4f));
class NMountainBreeze(BossModule module) : MountainBreeze(module, (uint)AID.NMountainBreeze);
class SMountainBreeze(BossModule module) : MountainBreeze(module, (uint)AID.SMountainBreeze);

abstract class Issen(BossModule module, uint aid) : Components.SingleTargetCast(module, aid);
class NIssen(BossModule module) : Issen(module, (uint)AID.NIssen);
class SIssen(BossModule module) : Issen(module, (uint)AID.SIssen);

abstract class Huton(BossModule module, uint aid) : Components.SingleTargetCast(module, aid, "Cast speed buff");
class NHuton(BossModule module) : Huton(module, (uint)AID.NHuton);
class SHuton(BossModule module) : Huton(module, (uint)AID.SHuton);

abstract class JujiShuriken(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(40, 1.5f));
class NJujiShuriken(BossModule module) : JujiShuriken(module, (uint)AID.NJujiShuriken);
class SJujiShuriken(BossModule module) : JujiShuriken(module, (uint)AID.SJujiShuriken);
class NJujiShurikenFast(BossModule module) : JujiShuriken(module, (uint)AID.NJujiShurikenFast);
class SJujiShurikenFast(BossModule module) : JujiShuriken(module, (uint)AID.SJujiShurikenFast);

class C020Trash2States : StateMachineBuilder
{
    public C020Trash2States(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BladeOfTheTengu>()
            .ActivateOnEnter<NWrathOfTheTengu>(!savage)
            .ActivateOnEnter<NGazeOfTheTengu>(!savage)
            .ActivateOnEnter<SWrathOfTheTengu>(savage)
            .ActivateOnEnter<SGazeOfTheTengu>(savage)
            .ActivateOnEnter<NIssen>(!savage)
            .ActivateOnEnter<NHuton>(!savage)
            .ActivateOnEnter<NJujiShuriken>(!savage)
            .ActivateOnEnter<NJujiShurikenFast>(!savage)
            .ActivateOnEnter<SIssen>(savage)
            .ActivateOnEnter<SHuton>(savage)
            .ActivateOnEnter<SJujiShuriken>(savage)
            .ActivateOnEnter<SJujiShurikenFast>(savage)
            .ActivateOnEnter<NMountainBreeze>(!savage) // for yamabiko
            .ActivateOnEnter<SMountainBreeze>(savage)
            .Raw.Update = () =>
            {
                var allDeadOrDestroyed = true;
                var enemies = module.Enemies(savage ? C020Trash2.TrashSavage : C020Trash2.TrashNormal);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    if (!enemy.IsDeadOrDestroyed)
                    {
                        allDeadOrDestroyed = false;
                        break;
                    }
                }
                return allDeadOrDestroyed;
            };
    }
}

class C020NTrash2States(BossModule module) : C020Trash2States(module, false);
class C020STrash2States(BossModule module) : C020Trash2States(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NOnmitsugashira, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12424, SortOrder = 3)]
public class C020NTrash2(WorldState ws, Actor primary) : C020Trash2(ws, primary, false);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SOnmitsugashira, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12424, SortOrder = 3)]
public class C020STrash2(WorldState ws, Actor primary) : C020Trash2(ws, primary, true);

public abstract class C020Trash2(WorldState ws, Actor primary, bool savage) : BossModule(ws, primary, new(300f, 0f), new ArenaBoundsRect(19.5f, 39.5f))
{
    public static readonly uint[] TrashNormal = [(uint)OID.NKotengu, (uint)OID.NOnmitsugashira, (uint)OID.NYamabiko];
    public static readonly uint[] TrashSavage = [(uint)OID.SKotengu, (uint)OID.SOnmitsugashira, (uint)OID.SYamabiko];

    protected override bool CheckPull()
    {
        var enemies = Enemies(savage ? TrashSavage : TrashNormal);
        var count = enemies.Count;
        for (var i = 0; i < count; ++i)
        {
            var enemy = enemies[i];
            if (enemy.InCombat)
                return true;
        }
        return false;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(savage ? TrashSavage : TrashNormal));
    }
}
