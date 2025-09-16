namespace BossMod.Stormblood.Raid.O3NHalicarnassus;

public enum OID : uint
{
    Boss = 0x1E14, // R0.5
    SoulReaper = 0x1E15, // R2.0
    GreatDragon = 0x1E16, // R6.0
    Helper = 0x186D
}

public enum AID : uint
{
    AutoAttackDragon = 970, // GreatDragon->player, no cast, single-target
    AutoAttack = 871, // Boss->player, no cast, single-target

    SpellbladeHoly = 8943, // Boss->self, 3.0s cast, single-target
    SpellbladeBlizzardIII = 9313, // Boss->self, 3.0s cast, range 6, point blank AOE
    SpellbladeFireIII = 9312, // Boss->self, 3.0s cast, range ?-15 AOE Donut
    SpellbladeThunderIII = 9314, // Boss->self, 2.5s cast, range 60, width 6, frontal line AOE
    HolyBlur = 9316, //Helper->player, no cast, range 6
    Ribbit = 9318, //Boss->none, 4.0s cast, range 17, 95-degree cone, frontal AOE cone

    TheQueensWaltz1 = 9327, // Boss->self, 3.0s cast
    SwordDance = 9328, // Helper->self, 3.0s cast, range 34, 20-degree cone
    TheQueensWaltz2 = 9329, // Boss->self, 5.0s cast
    EarthlyDance = 9330, // Helper->self, no cast, range 80
    Uplift = 9331, // Helper->self, no cast, range 80

    ThePlayingField = 8960, // Boss->self, 3.0s cast
    PanelSwap = 8964, // Boss->self, 3.0s cast
    TheGame = 9325, // Boss->self, 5.0s cast, range 80

    PlaceToken = 8955, // Boss->self, 3.0s cast, spawns GreatDragon
    PlaceDarkToken = 8956, // Boss->self 3.0s cast, spawns SoulReapers
    CrossReaper = 9323, // SoulReaper->self, 8.5s cast, range8, point blank AOE
    FrostBreath = 9334, // GreatDragon->self, no cast, range 15, 8.5-degree cone, frontal cone AoE
}

class Ribbit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ribbit, new AOEShapeCone(19.5f, 75f.Degrees()));
class SpellbladeThunderIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpellbladeThunderIII, new AOEShapeRect(60f, 3f));
class SpellbladeBlizzardIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpellbladeBlizzardIII, 9f);
class SpellbladeFireIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpellbladeFireIII, new AOEShapeDonut(4f, 15f));
class CrossReaper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrossReaper, 10f);
class TheQueensWaltz1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheQueensWaltz1, new AOEShapeCone(34f, 10f.Degrees()));
class TheQueensWaltz2(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(5f, 5f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheQueensWaltz2)
        {
            int[] xOffsets = [-15, -5, 5, 15];
            int[] zOffsets = [-15, -5, 5, 15];

            for (var i = 0; i < 4; ++i)
            {
                var z = zOffsets[i];
                for (var j = 0; j < 4; ++j)
                {
                    var x = xOffsets[j];
                    if (x != z)
                    {
                        _aoes.Add(new(rect, new(x, z)));
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TheQueensWaltz2)
        {
            _aoes.Clear();
        }
    }
}

class TheGame(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(5f, 5f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            var safeTiles = new List<int>();

            switch (actor.Role)
            {
                case Role.Tank:
                    safeTiles.Add(6);
                    break;
                case Role.Healer:
                    safeTiles.Add(9);
                    break;
                case Role.Melee:
                case Role.Ranged:
                    safeTiles.Add(5);
                    safeTiles.Add(10);
                    break;
            }
            var aoes = new List<AOEInstance>();
            for (var i = 0; i < 16; ++i)
            {
                if (!safeTiles.Contains(i))
                {
                    aoes.Add(_aoes[i]);
                }
            }
            return CollectionsMarshal.AsSpan(aoes);
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheGame)
        {
            int[] xOffsets = [-15, -5, 5, 15];
            int[] zOffsets = [-15, -5, 5, 15];

            for (var i = 0; i < 4; ++i)
            {
                var z = zOffsets[i];
                for (var j = 0; j < 4; ++j)
                {
                    _aoes.Add(new(rect, new(xOffsets[j], z)));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TheGame)
        {
            _aoes.Clear();
        }
    }
}
class O3NHalicarnassusStates : StateMachineBuilder
{
    public O3NHalicarnassusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Ribbit>()
            .ActivateOnEnter<SpellbladeThunderIII>()
            .ActivateOnEnter<SpellbladeBlizzardIII>()
            .ActivateOnEnter<SpellbladeFireIII>()
            .ActivateOnEnter<CrossReaper>()
            .ActivateOnEnter<TheQueensWaltz1>()
            .ActivateOnEnter<TheQueensWaltz2>()
            .ActivateOnEnter<TheGame>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 254, NameID = 5633)]
public class O3NHalicarnassus(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsSquare(20));
