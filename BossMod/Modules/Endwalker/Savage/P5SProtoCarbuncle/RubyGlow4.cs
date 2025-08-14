namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

// this includes venom pools and raging claw/searing ray aoes
class RubyGlow4(BossModule module) : RubyGlowRecolor(module, 5)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var poison = ActivePoisonAOEs();
        var len = poison.Length;
        var aoes = new List<AOEInstance>(len + 2);
        if (CurRecolorState != RecolorState.BeforeStones && MagicStones.Count != 0)
            aoes.Add(new(ShapeHalf, Arena.Center.Quantized(), Angle.FromDirection(QuadrantDir(AOEQuadrant))));
        aoes.AddRange(poison);

        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.RagingClaw) ?? false)
            aoes.Add(new(ShapeHalf, Module.PrimaryActor.Position, Module.PrimaryActor.CastInfo.Rotation, Module.CastFinishAt(Module.PrimaryActor.CastInfo)));
        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.SearingRay) ?? false)
            aoes.Add(new(ShapeHalf, Arena.Center.Quantized(), Module.PrimaryActor.CastInfo.Rotation + 180f.Degrees(), Module.CastFinishAt(Module.PrimaryActor.CastInfo)));
        return CollectionsMarshal.AsSpan(aoes);
    }
}
