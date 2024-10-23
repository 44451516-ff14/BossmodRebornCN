namespace BossMod.Endwalker.Savage.P9SKokytos;

class GluttonysAugur(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.GluttonysAugurAOE));
class SoulSurge(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.SoulSurge));
class BeastlyFury(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BeastlyFuryAOE));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 937, NameID = 12369, PlanLevel = 90)]
public class P9SKokytos(WorldState ws, Actor primary) : BossModule(ws, primary, center, arena)
{
    public static readonly WPos center = new(100, 100);
    private const int rectWidth = 8;
    private const int rectHeight = 2;

    private static readonly Circle[] union = [new(center, 20)];
    private static readonly Rectangle[] difference0 = [new(new(100, 119.5f), rectWidth, rectHeight), new(new(80.5f, 100), rectWidth, rectHeight, 90.Degrees()),
    new(new(119.5f, 100), rectWidth, rectHeight, 90.Degrees()), new(new(100, 80.5f), rectWidth, rectHeight)];
    private static readonly Rectangle[] difference45 = [RotatedRectangle(new WPos(100, 119.5f), 45.Degrees()),
    RotatedRectangle(new WPos(80.5f, 100), 135.Degrees()), RotatedRectangle(new WPos(119.5f, 100), -45.Degrees()),
    RotatedRectangle(new WPos(100, 80.5f), -135.Degrees())];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union);
    public static readonly ArenaBounds arenaUplift0 = new ArenaBoundsComplex(union, difference0);
    public static readonly ArenaBounds arenaUplift45 = new ArenaBoundsComplex(union, difference45);
    private static Rectangle RotatedRectangle(WPos position, Angle rotation)
    {
        var rotatedPosition = Helpers.RotateAroundOrigin(45, center, position);
        return new(rotatedPosition, rectWidth, rectHeight, rotation);
    }
}