namespace BossMod.Shadowbringers.Dungeon.D10AnamnesisAnyder.D102Kyklops;

public enum OID : uint
{
    Boss = 0x2CFE, // R4.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 19283, // Boss->player, no cast, single-target

    TheFinalVerse = 19288, // Boss->self, 4.0s cast, range 40 circle
    EyeOfTheCyclone = 19287, // Boss->self, 4.0s cast, range 8-25 donut

    TerribleHammerVisual = 19289, // Boss->self, 3.0s cast, single-target, spawns blade icons
    TerribleBladeVisual = 19290, // Boss->self, 3.0s cast, single-target, spawns hammer icons
    TerribleHammer = 19293, // Helper->self, no cast, range 10 width 10 rect
    TerribleBlade = 19294, // Helper->self, no cast, range 10 width 10 rect

    RagingGlower = 19286, // Boss->self, 3.0s cast, range 45 width 6 rect
    MinaSwipe2000 = 19284, // Boss->self, 4.0s cast, range 12 120-degree cone
    MinaSwing2000 = 19285, // Boss->self, 4.0s cast, range 12 circle

    OpenHearthVisual = 19292, // Boss->self, no cast, single-target, stack
    OpenHearth = 19296, // Helper->player, 5.0s cast, range 6 circle

    WanderersPyreVisual = 19291, // Boss->self, no cast, single-target, spread
    WanderersPyre = 19295 // Helper->player, 5.0s cast, range 5 circle
}

class TheFinalVerse(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TheFinalVerse));
class WanderersPyre(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.WanderersPyre), 5);
class OpenHearth(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.OpenHearth), 6);
class EyeOfTheCyclone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EyeOfTheCyclone), new AOEShapeDonut(8, 25));
class RagingGlower(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RagingGlower), new AOEShapeRect(45, 3));
class MinaSwipe2000(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MinaSwipe2000), new AOEShapeCone(12, 60.Degrees()));
class MinaSwing2000(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MinaSwing2000), new AOEShapeCircle(12));

class TerribleBladeHammer(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly WPos[] xPattern = [new(10, -70), new(30, -70), new(20, -80), new(10, -90), new(30, -90)];
    private static readonly WPos[] crossPattern = [new(20, -70), new(10, -80), new(30, -80), new(20, -90)];
    private static readonly AOEShapeRect rect = new(5, 5, 5);
    private const int XPatternActivationTime = 16;
    private const float CrossPatternActivationTime = 18.2f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;

        for (var i = 0; i < _aoes.Count; ++i)
        {
            if ((_aoes[i].Activation - _aoes[0].Activation).TotalSeconds <= 1)
                yield return _aoes[i];
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        // 0D - 08000400 - hammer X pattern, 20001000 - hammer cross pattern
        // 0E - 00800040 - blade cross pattern, 02000100 - blade X pattern
        // any - 00080004 - pattern disappears
        if (_aoes.Count > 0 || index is not 0xD and not 0xE)
            return;

        if (state is 0x08000400 or 0x02000100)
            SetupPattern(xPattern, crossPattern, XPatternActivationTime, CrossPatternActivationTime);
        else if (state is 0x20001000 or 0x00800040)
            SetupPattern(crossPattern, xPattern, XPatternActivationTime, CrossPatternActivationTime);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.TerribleHammer or AID.TerribleBlade)
            _aoes.RemoveAt(0);
    }

    private void SetupPattern(WPos[] primaryPattern, WPos[] secondaryPattern, float primaryTime, float secondaryTime)
    {
        AddAOEs(primaryPattern, primaryTime);
        AddAOEs(secondaryPattern, secondaryTime);
    }

    private void AddAOEs(WPos[] pattern, float activationTime)
    {
        for (var i = 0; i < pattern.Length; ++i)
            _aoes.Add(new(rect, pattern[i], Angle.AnglesCardinals[1], WorldState.FutureTime(activationTime)));
    }
}

class D102KyklopsStates : StateMachineBuilder
{
    public D102KyklopsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TerribleBladeHammer>()
            .ActivateOnEnter<TheFinalVerse>()
            .ActivateOnEnter<WanderersPyre>()
            .ActivateOnEnter<OpenHearth>()
            .ActivateOnEnter<EyeOfTheCyclone>()
            .ActivateOnEnter<RagingGlower>()
            .ActivateOnEnter<MinaSwing2000>()
            .ActivateOnEnter<MinaSwipe2000>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 714, NameID = 9263)]
public class D102Kyklops(WorldState ws, Actor primary) : BossModule(ws, primary, new(20, -80.1f), new ArenaBoundsRect(14.5f, 14.4f));
