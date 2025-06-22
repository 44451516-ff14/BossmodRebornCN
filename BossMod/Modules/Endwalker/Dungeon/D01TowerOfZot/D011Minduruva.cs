namespace BossMod.Endwalker.Dungeon.D01TowerOfZot.D011Minduruva;

public enum OID : uint
{
    Boss = 0x33EE, // R=2.04
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    ManusyaBio = 25248, // Boss->player, 4.0s cast, single-target
    Teleport = 25241, // Boss->location, no cast, single-target
    ManusyaBlizzardIII1 = 25234, // Boss->self, 4.0s cast, single-target
    ManusyaBlizzardIII2 = 25238, // Helper->self, 4.0s cast, range 40+R 20-degree cone
    ManusyaFireIII1 = 25233, // Boss->self, 4.0s cast, single-target
    ManusyaFireIII2 = 25237, // Helper->self, 4.0s cast, range 5-40 donut
    ManusyaThunderIII1 = 25235, // Boss->self, 4.0s cast, single-target
    ManusyaThunderIII2 = 25239, // Helper->self, 4.0s cast, range 3 circle
    ManusyaBioIII1 = 25236, // Boss->self, 4.0s cast, single-target
    ManusyaBioIII2 = 25240, // Helper->self, 4.0s cast, range 40+R 180-degree cone
    TransmuteFireIII = 25242, // Boss->self, 2.7s cast, single-target
    Unknown = 25243, // Helper->Boss, 3.6s cast, single-target
    ManusyaFire2 = 25699, // Boss->player, 2.0s cast, single-target
    Dhrupad = 25244, // Boss->self, 4.0s cast, single-target, after this each of the non-tank players get hit once by a single-target spell (ManusyaBlizzard, ManusyaFire1, ManusyaThunder)
    ManusyaFire1 = 25245, // Boss->player, no cast, single-target
    ManusyaBlizzard = 25246, // Boss->player, no cast, single-target
    ManusyaThunder = 25247, // Boss->player, no cast, single-target
    TransmuteBlizzardIII = 25371, // Boss->self, 2.7s cast, single-target
    TransmuteThunderIII = 25372 // Boss->self, 2.7s cast, single-target
}

public enum SID : uint
{
    Poison = 18 // Boss->player, extra=0x0
}

class ManusyaBio(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ManusyaBio, "Tankbuster + cleansable poison");

class Poison(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Poison, "Poison", "poisoned");

class Dhrupad(BossModule module) : BossComponent(module)
{
    private int NumCasts;
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Dhrupad)
            active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ManusyaFire1 or (uint)AID.ManusyaBlizzard or (uint)AID.ManusyaThunder)
        {
            ++NumCasts;
            if (NumCasts == 3)
            {
                NumCasts = 0;
                active = false;
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (active)
            hints.Add("3 single target hits + DoTs");
    }
}

class ManusyaThunderIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManusyaThunderIII2, 3f);
class ManusyaBioIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManusyaBioIII2, new AOEShapeCone(40.5f, 90f.Degrees()));
class ManusyaBlizzardIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManusyaBlizzardIII2, new AOEShapeCone(40.5f, 10f.Degrees()));
class ManusyaFireIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManusyaFireIII2, new AOEShapeDonut(5f, 60f));

class D011MinduruvaStates : StateMachineBuilder
{
    public D011MinduruvaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Dhrupad>()
            .ActivateOnEnter<ManusyaBio>()
            .ActivateOnEnter<Poison>()
            .ActivateOnEnter<ManusyaThunderIII>()
            .ActivateOnEnter<ManusyaFireIII>()
            .ActivateOnEnter<ManusyaBioIII>()
            .ActivateOnEnter<ManusyaBlizzardIII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 783, NameID = 10256)]
public class D011Minduruva(WorldState ws, Actor primary) : BossModule(ws, primary, new(68f, -124f), new ArenaBoundsCircle(19.5f));
