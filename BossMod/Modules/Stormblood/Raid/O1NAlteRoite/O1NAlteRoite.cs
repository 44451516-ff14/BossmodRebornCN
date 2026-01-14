using BossMod.Components;
using BossMod;

namespace BossMod.Stormblood.Raid.O1NAlteRoite;

////////////////////////
// Raidwide Mechanics//
///////////////////////
sealed class Roar(BossModule module) : Components.RaidwideCast(module, (uint)AID.Roar);
sealed class ThinIce(BossModule module) : Components.ThinIce(module, 6f, true, (uint)SID.ThinIce, true);
sealed class Charybdis(BossModule module) : Components.RaidwideCast(module, (uint)AID.Charybdis);

////////////////////////
// AOE stuff         //
///////////////////////
sealed class ClampAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Clamp, new AOEShapeRect(9f + module.PrimaryActor.HitboxRadius, 5f));

////////////////////////
// Twinbolt stuff    //
///////////////////////
sealed class TwinBoltTetheredBuster(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TwinBolt, hint: "Tankbuster! - Watch for Tethered Player!", AIHints.PredictedDamageType.Tankbuster);
sealed class TwinBoltAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwinBolt1, new AOEShapeCircle(5f));

////////////////////////
// Knockbacks        //
///////////////////////
sealed class ClampKB(BossModule module)
    : Components.SimpleKnockbacks(
        module,
        (uint)AID.Clamp,
        distance: 20f,
        kind: Components.GenericKnockback.Kind.DirForward,
        shape: new AOEShapeRect(9f + module.PrimaryActor.HitboxRadius, 5f)); // width 10 => halfwidth 5

sealed class BreathwingKB(BossModule module)
    : Components.SimpleKnockbacks(module, (uint)AID.BreathWing, distance: 20f, kind: Components.GenericKnockback.Kind.DirForward);

sealed class DownburstKB(BossModule module)
    : Components.SimpleKnockbacks(module, (uint)AID.Downburst, distance: 20f, kind: Components.GenericKnockback.Kind.AwayFromOrigin);

////////////////////////
// Tornado  Mechanics//
///////////////////////
sealed class DownburstTornado(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // same filter you used, but draw in danger color
        var aoes = new List<AOEInstance>();
        foreach (var a in Module.Enemies((uint)OID.Gen_AlteRoite)
                     .Where(a => a.Position.InCircle(Module.Arena.Center, 2f) && Module.PrimaryActor.CastInfo?.Action.ID == (uint)AID.Downburst))
        {
            aoes.Add(new(_shape, a.Position.Quantized(), a.Rotation, default, Colors.Danger));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }
}
////////////////////////
// Stack + Spread    //
///////////////////////
sealed class BlazeLevinStackSpread(BossModule module)
    : Components.IconStackSpread(
        module,
        stackIcon: 62,
        spreadIcon: 108,
        stackAID: (uint)AID.Blaze,
        spreadAID: (uint)AID.Levinbolt,
        stackRadius: 6f,
        spreadRadius: 6f,
        activationDelay: 5.0);

////////////////////////
// Module Stuff       //
///////////////////////
[ModuleInfo(BossModuleInfo.Maturity.WIP,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
IconIDType = typeof(IconID), // replace null with typeof(IconID) if applicable
Contributors = "JoeSparkx",
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 252,
NameID = 5629)]
public class O1NAlteRoite(WorldState ws, Actor primary) : BossModule(ws, primary, new(00, 00), new ArenaBoundsCircle(20));
