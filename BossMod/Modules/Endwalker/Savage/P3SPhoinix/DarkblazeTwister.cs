namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to darkblaze twister mechanics
class TwisterVoidzone(BossModule module) : Components.Voidzone(module, 5f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.TwisterVoidzone);
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
class BurningTwister(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BurningTwister), new AOEShapeDonut(7f, 20f));

class DarkTwister(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.DarkTwister), _knockbackRange, true)
{
    private const float _knockbackRange = 17;
    private const float _aoeInnerRadius = 5;
    private const float _aoeMiddleRadius = 7;
    private const float safeOffset = _knockbackRange + (_aoeInnerRadius + _aoeMiddleRadius) / 2;
    private const float safeRadius = (_aoeMiddleRadius - _aoeInnerRadius) / 2;

    public List<Actor> BurningTwisters()
    {
        List<Actor> burningTwisters = [];
        var twisters = Module.Enemies((uint)OID.DarkblazeTwister);
        var count = twisters.Count;
        for (var i = 0; i < count; ++i)
        {
            var twister = twisters[i];
            if (twister.CastInfo != null && twister.CastInfo.IsSpell(AID.BurningTwister))
                burningTwisters.Add(twister);
        }
        return burningTwisters;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (Casters.Count == 0)
            return;
        var darkTwister = Casters[0];
        foreach (var burningTwister in BurningTwisters())
        {
            var dir = burningTwister.Position - darkTwister.Position;
            var len = dir.Length();
            dir /= len;
            Arena.AddCircle(darkTwister.Position + dir * (len - safeOffset), safeRadius, Colors.Safe);
        }
    }
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var comp1 = Module.FindComponent<TwisterVoidzone>();
        if (comp1 != null)
        {
            var aoes = comp1.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].Check(pos))
                    return true;
            }
        }
        var comp2 = Module.FindComponent<BurningTwister>();
        if (comp2 != null)
        {
            var aoes = comp2.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].Check(pos))
                    return true;
            }
        }
        return !Module.InBounds(pos);
    }
}
