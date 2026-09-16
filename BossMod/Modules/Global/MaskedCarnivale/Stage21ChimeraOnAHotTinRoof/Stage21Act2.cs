namespace BossMod.Global.MaskedCarnivale.Stage21.Act2;

public enum OID : uint
{
    Boss = 0x2730, //R=3.7
    ArenaImp = 0x2731, //R=0.45
    Voidzone = 0x1E972A
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    Blizzard = 14267, // Imp->player, 1.0s cast, single-target
    VoidBlizzard = 15063, // Imp->player, 6.0s cast, single-target
    Icefall = 15064, // Imp->location, 2.5s cast, range 5 circle
    TheRamsVoice = 15079, // Boss->self, 4.0s cast, range 9 circle
    TheDragonsVoice = 15080, // Boss->self, 4.0s cast, range 8-30 donut
    TheRamsKeeper = 15081 // Boss->location, 6.0s cast, range 9 circle
}

sealed class TheRamsKeeper(BossModule module) : Components.VoidzoneAtCastTarget(module, 9f, (uint)AID.TheRamsKeeper, GetVoidzones, 0.9d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Voidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }
}
sealed class TheRamsKeeperHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.TheRamsKeeper);
sealed class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, 9f);
sealed class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, new AOEShapeDonut(8f, 30f));
sealed class Icefall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Icefall, 5f);
sealed class VoidBlizzard(BossModule module) : Components.CastInterruptHint(module, (uint)AID.VoidBlizzard);

sealed class Stage21Act2States : StateMachineBuilder
{
    public Stage21Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidBlizzard>()
            .ActivateOnEnter<Icefall>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheRamsKeeper>()
            .ActivateOnEnter<TheRamsKeeperHint>()
            .ActivateOnEnter<TheRamsVoice>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 631u, NameID = 8121u, SortOrder = 2)]
public sealed class Stage21Act2(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ArenaImp), Colors.Object);
    }

    private readonly string[] _prePullHints =
    [
        "Interrupt The Rams Keeper with Flying Sardine.",
        "You can start the Final Sting combination at about 50% health left. (Off-guard->Bristle->Moonflute->Final Sting).",
        "The boss will sometimes spawn an imp during the fight.",
        "The imps are weak to fire spells and strong against ice. Interrupt Void Blizzard with Flying Sardine."
    ];

    public override string[] PrePullHints => _prePullHints;
}
