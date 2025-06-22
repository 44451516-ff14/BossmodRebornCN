namespace BossMod.Stormblood.Alliance.A11Mateus;

class BlizzardSphere(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.KiteTether, 10, needToKite: true)
{
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.BlizzardSphere) // same tether ID is used by 2 mechanics, so we need to filter
            base.OnTethered(source, tether);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.BlizzardSphere)
            base.OnUntethered(source, tether);
    }
}

class FinRays(BossModule module) : Components.Cleave(module, (uint)AID.FinRays, new AOEShapeCone(11.8f, 60f.Degrees()), [(uint)OID.AzureGuard]);
