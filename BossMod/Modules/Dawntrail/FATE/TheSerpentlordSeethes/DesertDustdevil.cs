namespace BossMod.Dawntrail.FATE.Ttokrrone;

sealed class DesertDustdevil(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone cone = new(60f, 45f.Degrees());
    private readonly DustcloakDustdevil _aoe = module.FindComponent<DustcloakDustdevil>()!;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        switch (id)
        {
            case (uint)AID.FangwardDustdevilVisualCW:
            case (uint)AID.FangwardDustdevilVisualCCW:
            case (uint)AID.TailwardDustdevilVisualCW:
            case (uint)AID.TailwardDustdevilVisualCCW:
                var rotation = spell.Rotation;
                var direction = 90f.Degrees();
                var pos = spell.LocXZ;
                if (id is (uint)AID.FangwardDustdevilVisualCW or (uint)AID.TailwardDustdevilVisualCW)
                {
                    direction = -direction;
                }
                if (id is (uint)AID.TailwardDustdevilVisualCW or (uint)AID.TailwardDustdevilVisualCCW)
                {
                    rotation += 180f.Degrees();
                }
                var maxCasts = Sequences.Count == 0 ? 7 : 4;
                Sequences.Clear();
                Sequences.Add(new(cone, pos, rotation, direction, Module.CastFinishAt(spell, 0.9d), 2.6f, maxCasts));
                _aoe.SetAOE(pos, Module.CastFinishAt(spell, 0.9d));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.TailwardDustdevilFirst:
                case (uint)AID.FangwardDustdevilFirst:
                case (uint)AID.RightwardSandspoutDDRest:
                case (uint)AID.LeftwardSandspoutDDRest:
                    AdvanceSequence(0, WorldState.CurrentTime);
                    if (Sequences.Count != 0)
                    {
                        _aoe.SetAOE(spell.LocXZ, WorldState.FutureTime(2.6d));
                    }
                    else
                    {
                        _aoe.ClearAOE();
                    }
                    break;
            }
        }
    }
}

sealed class DustcloakDustdevil(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(13f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public void SetAOE(WPos position, DateTime activation) => _aoe = [new(circle, position, default, activation)];
    public void ClearAOE() => _aoe = [];
}
