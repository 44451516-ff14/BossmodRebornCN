namespace BossMod.Shadowbringers.Dungeon.D08AkadaemiaAnyder.D081Cladoselache;

public enum OID : uint
{
    Boss = 0x27AB, // R2.47
    Doliodus = 0x27AC, // R2.47
    Voidzone = 0x1E909F,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Doliodus->player, no cast, single-target

    ProtolithicPuncture = 15876, // Boss/Doliodus->player, 4.0s cast, single-target
    PelagicCleaver1 = 15881, // Doliodus->self, 3.5s cast, range 40 60-degree cone
    PelagicCleaver2 = 15883, // Doliodus->location, 8.0s cast, range 50 60-degree cone
    TidalGuillotine1 = 15880, // Boss->self, 4.0s cast, range 13 circle
    TidalGuillotine2 = 15882, // Boss->location, 8.0s cast, range 13 circle
    AquaticLance = 15877, // Boss/Doliodus->player, 4.0s cast, range 8 circle, spread + voidzone
    MarineMayhem = 15878, // Doliodus/Boss->self, 3.5s cast, range 40 circle
    MarineMayhemRepeat = 16241, // Boss/Doliodus->self, no cast, range 40 circle
    CarcharianVerve = 15879 // Boss/Doliodus->self, 2.0s cast, single-target, damage up after partner dies
}

class MarineMayhem(BossModule module) : Components.RaidwideCast(module, (uint)AID.MarineMayhem);
class ProtolithicPuncture(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.ProtolithicPuncture);
class PelagicCleaver1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PelagicCleaver1, new AOEShapeCone(40f, 30f.Degrees()));
class PelagicCleaver2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PelagicCleaver2, new AOEShapeCone(50f, 30f.Degrees()));
class TidalGuillotine(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TidalGuillotine1, (uint)AID.TidalGuillotine1], 13f);
class AquaticLance(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AquaticLance, 8f);
class AquaticLanceVoidzone(BossModule module) : Components.Voidzone(module, 8f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Voidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class D081CladoselacheStates : StateMachineBuilder
{
    public D081CladoselacheStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MarineMayhem>()
            .ActivateOnEnter<ProtolithicPuncture>()
            .ActivateOnEnter<PelagicCleaver1>()
            .ActivateOnEnter<PelagicCleaver2>()
            .ActivateOnEnter<TidalGuillotine>()
            .ActivateOnEnter<AquaticLance>()
            .ActivateOnEnter<AquaticLanceVoidzone>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(D081Cladoselache.Bosses);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 661, NameID = 8235)]
public class D081Cladoselache(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-305f, 211.5f), 19.5f * CosPI.Pi60th, 60)]);
    public static readonly uint[] Bosses = [(uint)OID.Boss, (uint)OID.Doliodus];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(Bosses));
    }
}
