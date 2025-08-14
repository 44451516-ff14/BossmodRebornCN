namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD190TheGodfather;

public enum OID : uint
{
    Boss = 0x1820, // R3.750, x1
    GiddyBomb = 0x18F3, // R0.6, small bombs that are untargetable, multiple spawn up and explode at once
    LavaBomb = 0x18F2, // R1.2, (also known as greybomb) cast a pbaoe that stuns needs to be on top of the boss's hitbox to stun
    RemedyBomb = 0x18F1 // R1.2, cast a roomwide aoe that will hit for 80% of max hp + inflicts a dot
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    Flashthoom = 7170, // LavaBomb->self, 5.0s cast, range 6+R circle, needs to be on boss's hitbox and finish the cast to stun the boss
    HypothermalCombustionMinionCast = 7172, // GiddyBomb->self, 1.5s cast, range 6+R circle
    HypothermalCombustionRemedyBomb = 7171, // RemedyBomb->self, 15.0s cast, range 50+R circle, raidwide, will take 80% of your hp and give a dot (15 potency for 5 seconds) if not killed
    MassiveBurst = 7103, // Boss->self, 25.0s cast, range 50 circle, raidwide, needs to be stunned by Flashthoom (Lavabomb/GreyBomb) cast
    Sap = 7169, // Boss->location, 3.5s cast, range 8 circle
    ScaldingScolding = 7168 // Boss->self, no cast, range 8+R 120-degree cone
}

//TODO: Make the boss's hitbox show up potentially/maybe add 2 circle indicators to show how close the stun bomb needs to be, add indicators to where the stun bomb will spawn next, to allow pre-positioning
//spawn locations for stun bomb are as follows: 1:(-288.626, -300.256) 2:(-297.465, -297.525) 3:(-288.837, -305.537) 4:(-309.132, -303.739) 5:(-298.355, -293.630) 6:(-301.954, -314.289) 7:(-299.119, -297.563)
sealed class BossAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.LavaBomb, (uint)OID.RemedyBomb]);
sealed class Flashthoom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flashthoom, 7.2f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var bomb = Module.Enemies((uint)OID.LavaBomb);
        if (bomb.Count != 0 && bomb[0] is Actor g && Module.PrimaryActor.Position.InCircle(g.Position, 7.2f))
            hints.SetPriority(g, AIHints.Enemy.PriorityForbidden);
    }
}
sealed class Sap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sap, 8f);
sealed class ScaldingScoldingCleave(BossModule module) : Components.Cleave(module, (uint)AID.ScaldingScolding, new AOEShapeCone(11.75f, 60f.Degrees()), activeWhileCasting: false);
sealed class RemedyBombEnrage(BossModule module) : Components.CastHint(module, (uint)AID.HypothermalCombustionRemedyBomb, "Remedy bomb is enraging!", true);
sealed class MassiveBurstEnrage(BossModule module) : Components.CastHint(module, (uint)AID.MassiveBurst, "Enrage! Stun boss with the Lavabomb!", true);

sealed class HypothermalMinion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(6.6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.GiddyBomb)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(10d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HypothermalCombustionMinionCast)
        {
            _aoes.Clear();
        }
    }
}

sealed class DD190TheGodfatherStates : StateMachineBuilder
{
    public DD190TheGodfatherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BossAdds>()
            .ActivateOnEnter<Flashthoom>()
            .ActivateOnEnter<Sap>()
            .ActivateOnEnter<ScaldingScoldingCleave>()
            .ActivateOnEnter<RemedyBombEnrage>()
            .ActivateOnEnter<MassiveBurstEnrage>()
            .ActivateOnEnter<HypothermalMinion>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 217, NameID = 5471)]
public sealed class DD190TheGodfather(WorldState ws, Actor primary) : BossModule(ws, primary, SharedBounds.ArenaBounds160170180190.Center, SharedBounds.ArenaBounds160170180190);
