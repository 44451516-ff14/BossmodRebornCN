namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

abstract class Ekpyrosis(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 19f); // TODO: verify falloff
class EkpyrosisProximityV(BossModule module) : Ekpyrosis(module, (uint)AID.EkpyrosisProximityV);
class EkpyrosisProximityH(BossModule module) : Ekpyrosis(module, (uint)AID.EkpyrosisProximityH);

class EkpyrosisExaflare(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EkpyrosisExaflareFirst)
        {
            Lines.Add(new(caster.Position, 8f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 2.1d, 5, 2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.EkpyrosisExaflareFirst or (uint)AID.EkpyrosisExaflareRest)
        {
            ++NumCasts;
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}

class EkpyrosisSpread : Components.UniformStackSpread
{
    public EkpyrosisSpread(BossModule module) : base(module, default, 6f)
    {
        foreach (var p in Raid.WithoutSlot(true, true, true))
            AddSpread(p, module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EkpyrosisSpread)
            Spreads.Clear();
    }
}
