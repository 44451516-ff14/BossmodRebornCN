namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD100NybethObdilord;

public enum OID : uint
{
    Boss = 0x1808, // R2.4
    BicephalicCorse = 0x180A, // R1.9
    GiantCorse = 0x1809, // R1.9
    IronCorse = 0x180B // R1.9
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss/BicephalicCorse/GiantCorse/IronCorse->player, no cast, single-target

    Abyss = 6872, // Boss->player, 2.0s cast, range 6 circle // kinda like a tankbuster? It's a circle on the player
    ButterflyFloat = 6879, // IronCorse->player, 3.0s cast, single-target
    Catapult = 6878, // BicephalicCorse->location, 3.0s cast, range 6 circle
    Doom = 6875, // Boss->self, 5.0s cast, range 45+R 120-degree cone, feels like this is wrong,
    GlassPunch = 6877, // GiantCorse/BicephalicCorse->self, no cast, range 6+R ?-degree cone
    Shackle = 6874, // Boss->self, 3.0s cast, range 50+R width 8 rect
    SummonDarkness = 6876, // Boss->self, 3.0s cast, ???, Summons Corses,
    WordOfPain = 6873 // Boss->self, no cast, range 40+R circle
}

class Abyss(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Abyss, 6f);
class Catapult(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Catapult, 6f);
class CorseAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.BicephalicCorse, (uint)OID.GiantCorse, (uint)OID.IronCorse]);
class Doom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Doom, new AOEShapeCone(47.4f, 60f.Degrees()));
class Shackle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shackle, new AOEShapeRect(52.4f, 4f));
class SummonDarkness(BossModule module) : Components.CastHint(module, (uint)AID.SummonDarkness, "Summoning the corse, incoming Adds! \nRemember to use a resolution to make them permanently disappear");

class EncounterHints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"There is 3 sets of adds that spawn at HP %'s -> (90%, 65%, 40%) \nA resolution can make the adds permanently disappear once they are at 0% HP/the corpse are just laying on the floor.\nResolution is also does high damage to the adds + 0.3% to the Boss\nSolo tip: Either pop a resolution on all add packs, or pop lust -> resolution on 2nd ad pack. Make sure to keep regen up!");
    }
}

class DD100NybethObdilordStates : StateMachineBuilder
{
    public DD100NybethObdilordStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Abyss>()
            .ActivateOnEnter<Catapult>()
            .ActivateOnEnter<CorseAdds>()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<Shackle>()
            .ActivateOnEnter<SummonDarkness>()
            .DeactivateOnEnter<EncounterHints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 208, NameID = 5356)]
public class DD100NybethObdilord : BossModule
{
    public DD100NybethObdilord(WorldState ws, Actor primary) : base(ws, primary, arena.Center, arena)
    {
        ActivateComponent<EncounterHints>();
    }

    private static readonly WPos[] vertices = [new(302.11f, 276.34f), new(307.39f, 276.36f), new(308.04f, 276.58f), new(309.51f, 278.03f), new(310.04f, 278.17f),
    new(311.94f, 278.24f), new(312.52f, 278.61f), new(321.65f, 287.79f), new(321.81f, 288.44f), new(321.9f, 290.34f),
    new(324.01f, 292.58f), new(324.28f, 293.17f), new(324.29f, 306.71f), new(324.08f, 307.35f), new(321.86f, 309.64f),
    new(321.77f, 312.15f), new(312.3f, 321.62f), new(311.60f, 321.80f), new(309.56f, 321.92f), new(307.32f, 324.12f),
    new(306.66f, 324.29f), new(300.1f, 324.22f), new(293.11f, 324.29f), new(292.52f, 323.96f), new(290.57f, 322.02f),
    new(290.01f, 321.88f), new(288.08f, 321.81f), new(287.52f, 321.42f), new(278.22f, 312.13f), new(278.14f, 310.01f),
    new(278f, 309.45f), new(275.71f, 307.13f), new(275.73f, 293.36f), new(275.86f, 292.68f), new(277.75f, 290.77f),
    new(278.12f, 290.27f), new(278.20f, 288.37f), new(278.33f, 287.73f), new(287.51f, 278.59f), new(288.05f, 278.21f),
    new(290.12f, 278.13f), new(290.61f, 277.91f), new(291.97f, 276.56f), new(302.11f, 276.34f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);
}
