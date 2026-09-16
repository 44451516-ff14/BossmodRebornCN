namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

sealed class StiffBreeze(BossModule module) : Components.Voidzone(module, 1f, GetVoidzones)
{
    // note: actual aoe, if triggered, has radius 2, but we care about triggering radius
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.StiffBreeze);
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
