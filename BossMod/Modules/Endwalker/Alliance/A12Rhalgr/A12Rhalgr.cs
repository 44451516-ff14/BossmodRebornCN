namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class DestructiveBolt(BossModule module) : Components.BaitAwayCast(module, (uint)AID.DestructiveBoltAOE, 3f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
class StrikingMeteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StrikingMeteor, 6f);
class BronzeLightning(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BronzeLightning, new AOEShapeCone(50f, 22.5f.Degrees()), 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11273, SortOrder = 3, PlanLevel = 90)]
public class A12Rhalgr(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-29.125f, 235.5f), new(-31.371f, 237.971f), new(-34.382f, 241.552f), new(-36.774f, 244.204f), new(-39.093f, 246.871f),
    new(-41.312f, 249.545f), new(-43.397f, 252.207f), new(-45.088f, 254.989f), new(-46.635f, 257.715f), new(-47.844f, 260.435f),
    new(-49.105f, 263.13f), new(-50.266f, 265.694f), new(-51.346f, 268.28f), new(-52.387f, 270.504f), new(-53.368f, 272.296f), new(-53.588f, 273.264f),
    new(-53.49f, 273.623f),new(-53.285f, 273.95f), new(-52.64f, 274.2f), new(-51.759f, 273.843f), new(-49.168f, 274.097f),
    new(-47.83f, 274.666f), new(-46.29f, 276.04f), new(-45.676f, 275.29f), new(-45.676f, 275.29f), new(-44.942f, 274.517f),
    new(-43.739f, 272.79f), new(-42.3f, 271.298f), new(-41.363f, 270.882f), new(-40.425f, 270.466f), new(-39.896f, 270.657f),
    new(-39.101f, 270.943f), new(-38.426f, 271.834f), new(-38.353f, 272.41f), new(-38.281f, 272.986f), new(-38.19f, 273.697f),
    new(-38.194f, 274.646f), new(-38.289f, 275.662f), new(-38.475f, 276.74f), new(-38.884f, 277.892f), new(-39.317f, 279.053f),
    new(-39.826f, 280.478f), new(-40.546f, 282.257f), new(-41.621f, 284.198f), new(-42.967f, 286.181f), new(-44.362f, 288.169f),
    new(-45.526f, 290.215f), new(-46.469f, 292.028f), new(-47.312f, 293.499f), new(-48.077f, 294.8f), new(-48.832f, 296.152f),
    new(-49.39f, 296.73f), new(-40.96f, 300.2f), new(-39.523f, 297.835f), new(-38.114f, 295.081f), new(-37.071f, 293.169f),
    new(-36.038f, 291.811f), new(-35.144f, 290.715f), new(-34.375f, 290.474f), new(-33.606f, 290.234f),
    new(-32.93f, 290.14f), new(-32.27f, 290.119f), new(-31.439f, 290.269f), new(-30.393f, 291.399f), new(-30.324f, 292.351f),
    new(-30.256f, 293.302f), new(-30.437f, 295.104f), new(-30.639f, 297.222f), new(-30.8f, 299.232f), new(-30.91f, 300.924f),
    new(-31.001f, 302.312f), new(-31.128f, 303.684f), new(-31.3f, 304.94f), new(-22.35f, 306.16f),
    new(-22.438f, 305.682f), new(-22.206f, 304.537f), new(-22.03f, 303.366f), new(-21.831f, 302.302f), new(-21.539f, 301.155f),
    new(-21.189f, 299.72f), new(-20.87f, 297.251f), new(-20.486f, 294.652f), new(-20.175f, 292.81f), new(-20.119f, 291.293f),
    new(-19.906f, 290.194f), new(-19.32f, 289.086f), new(-18.25f, 288.535f), new(-17.259f, 288.489f), new(-16.21f, 289.062f),
    new(-15.08f, 289.642f), new(-14.139f, 290.287f), new(-13.639f, 290.967f), new(-13.306f, 291.935f), new(-13.34f, 293.547f),
    new(-13.578f, 295.56f), new(-13.77f, 297.547f), new(-13.747f, 300.133f), new(-13.751f, 302.166f), new(-13.9f, 303.98f),
    new(-6.23f, 304.72f), new(-6.136f, 304.301f), new(-6.065f, 303.345f), new(-5.978f, 301.849f), new(-5.886f, 299.967f),
    new(-5.734f, 297.595f), new(-5.378f, 294.621f), new(-4.985f, 291.704f), new(-4.586f, 288.637f), new(-4.305f, 287.21f),
    new(-3.465f, 286.755f), new(-2.47f, 287.028f), new(-1.673f, 287.454f), new(-0.847f, 287.98f), new(0.102f, 289.346f),
    new(1.097f, 291.413f), new(2.189f, 294.027f), new(2.917f, 295.808f), new(3.31f, 297.2f), new(9.09f, 293.91f), new(8.214f, 291.819f),
    new(7.376f, 289.885f), new(6.691f, 288.32f), new(6.213f, 287.016f), new(5.972f, 285.837f), new(5.898f, 284.628f),
    new(6.015f, 283.377f), new(6.226f, 282.071f), new(6.563f, 280.707f), new(6.892f, 279.282f), new(7.125f, 277.429f),
    new(7.238f, 274.778f), new(7.331f, 271.638f), new(7.405f, 268.327f), new(7.395f, 264.881f), new(7.238f, 261.338f),
    new(6.999f, 257.713f), new(6.745f, 254.024f), new(6.497f, 250.338f), new(5.606f, 246.607f), new(4.541f, 242.959f),
    new(3.557f, 239.602f), new(2.737f, 236.809f), new(2.375f, 235.5f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
}
