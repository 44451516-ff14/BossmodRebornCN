namespace BossMod.Endwalker.Dungeon.D01TowerOfZot.D011Minduruva;

public enum OID : uint
{
    Minduruva = 0x33EE, // R=2.04
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Minduruva->player, no cast, single-target

    ManusyaBio = 25248, // Minduruva->player, 4.0s cast, single-target
    Teleport = 25241, // Minduruva->location, no cast, single-target
    ManusyaBlizzardIII1 = 25234, // Minduruva->self, 4.0s cast, single-target
    ManusyaBlizzardIII2 = 25238, // Helper->self, 4.0s cast, range 40+R 20-degree cone
    ManusyaFireIII1 = 25233, // Minduruva->self, 4.0s cast, single-target
    ManusyaFireIII2 = 25237, // Helper->self, 4.0s cast, range 5-40 donut
    ManusyaThunderIII1 = 25235, // Minduruva->self, 4.0s cast, single-target
    ManusyaThunderIII2 = 25239, // Helper->self, 4.0s cast, range 3 circle
    ManusyaBioIII1 = 25236, // Minduruva->self, 4.0s cast, single-target
    ManusyaBioIII2 = 25240, // Helper->self, 4.0s cast, range 40+R 180-degree cone
    ManusyaFire2 = 25699, // Minduruva->player, 2.0s cast, single-target
    Dhrupad = 25244, // Minduruva->self, 4.0s cast, single-target, after this each of the non-tank players get hit once by a single-target spell (ManusyaBlizzard, ManusyaFire1, ManusyaThunder)
    ManusyaFire1 = 25245, // Minduruva->player, no cast, single-target
    ManusyaBlizzard = 25246, // Minduruva->player, no cast, single-target
    ManusyaThunder = 25247, // Minduruva->player, no cast, single-target

    TansmuteVisual = 25243, // Helper->Boss, 3.6s cast, single-target
    TransmuteFireIII = 25242, // Minduruva->self, 2.7s cast, single-target
    TransmuteBlizzardIII = 25371, // Minduruva->self, 2.7s cast, single-target
    TransmuteThunderIII = 25372, // Minduruva->self, 2.7s cast, single-target
    TransmuteBioIII = 25373 // Minduruva->self, 2.7s cast, single-target
}

public enum SID : uint
{
    Poison = 18 // Minduruva->player, extra=0x0
}

sealed class ManusyaBio(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ManusyaBio, "Tankbuster + cleansable poison");

sealed class Poison(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Poison, "Poison", "poisoned");

sealed class Dhrupad(BossModule module) : BossComponent(module)
{
    private int NumCasts;
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Dhrupad)
        {
            active = true;
        }
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
        {
            hints.Add("3 single target hits + DoTs");
        }
    }
}

sealed class ManusyaThunderIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManusyaThunderIII2, 3f);
sealed class ManusyaBioIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManusyaBioIII2, new AOEShapeCone(40.5f, 90f.Degrees()));
sealed class ManusyaBlizzardIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManusyaBlizzardIII2, new AOEShapeCone(40.5f, 10f.Degrees()));
sealed class ManusyaFireIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManusyaFireIII2, new AOEShapeDonut(5f, 60f));

sealed class D011MinduruvaStates : StateMachineBuilder
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", PrimaryActorOID = (uint)OID.Minduruva, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 783u, NameID = 10256u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.Endwalker, SortOrder = 1)]
public sealed class D011Minduruva(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(68f, -124f), 19.5f, 48)], [new Rectangle(new(68f, -104f), 8f, 1.25f),
    new Rectangle(new(68f, -143.5f), 8f, 1.25f)]);
}
