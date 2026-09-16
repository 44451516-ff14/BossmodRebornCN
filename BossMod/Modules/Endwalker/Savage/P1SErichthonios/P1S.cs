namespace BossMod.Endwalker.Savage.P1SErichthonios;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 809u, NameID = 10576u, PlanLevel = 90)]
public sealed class P1S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
{
    public const float InnerCircleRadius = 12; // this determines in/out flails and cells boundary

    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Bounds is ArenaBoundsCircle)
        {
            // cells mode
            var diag = Bounds.Radius / 1.414214f;
            Arena.ZoneCircleOutline(Center, InnerCircleRadius, Colors.Border);
            Arena.AddLine(Center + new WDir(Bounds.Radius, 0), Center - new WDir(Bounds.Radius, 0), Colors.Border);
            Arena.AddLine(Center + new WDir(0, Bounds.Radius), Center - new WDir(0, Bounds.Radius), Colors.Border);
            Arena.AddLine(Center + new WDir(diag, +diag), Center - new WDir(diag, +diag), Colors.Border);
            Arena.AddLine(Center + new WDir(diag, -diag), Center - new WDir(diag, -diag), Colors.Border);
        }
    }
}
