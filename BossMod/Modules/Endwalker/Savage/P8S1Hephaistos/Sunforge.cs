namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class SunforgeCenterHint(BossModule module) : Components.CastHint(module, (uint)AID.SunforgeCenter, "Avoid center")
{
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
        {
            Arena.ZoneRect(Arena.Center, new WDir(1, 0), 21, -7, 21, Colors.SafeFromAOE);
            Arena.ZoneRect(Arena.Center, new WDir(-1, 0), 21, -7, 21, Colors.SafeFromAOE);
        }
    }
}

class SunforgeSidesHint(BossModule module) : Components.CastHint(module, (uint)AID.SunforgeSides, "Avoid sides")
{
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
        {
            Arena.ZoneRect(Arena.Center, new WDir(0, 1), 21, 21, 7, Colors.SafeFromAOE);
        }
    }
}

class SunforgeCenter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScorchingFang, new AOEShapeRect(21, 7, 21));
class SunforgeSides(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SunsPinion, new AOEShapeRect(21, 21, -7));
class SunforgeCenterIntermission(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScorchingFangIntermission, new AOEShapeRect(42, 7));
class SunforgeSidesIntermission(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScorchedPinion, new AOEShapeRect(21, 42, -7));
