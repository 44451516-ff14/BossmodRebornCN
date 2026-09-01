namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME1TwoHeadedAevis;

[SkipLocalsInit]
sealed class StormsBreath(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.StormsBreathCast, 14f)
{
    // TODO: verify with which mechanics can happen at same time in EX, can happen with at least Freezing Fugue
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Add(new(Arena.Center, Distance, Module.CastFinishAt(spell), Shape, spell.Rotation, KnockbackKind, 0, [], caster.InstanceID, IgnoreImmunes));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kbs = ActiveKnockbacks(slot, actor);
        if (kbs.Length != 0)
        {
            var kb = kbs[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var freezing = Module.FindComponent<FreezingFugue>();
                if (freezing != null && freezing.ActiveAOEs(slot, actor) is var aoes && aoes.Length != 0)
                {
                    var count = aoes.Length;
                    var pos = new WPos[count];
                    for (var i = 0; i < count; i++)
                    {
                        pos[i] = aoes[i].Origin;
                    }
                    hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOriginPlusAOECircles(Arena.Center, kb.Origin, 17f, 20f, pos, 21f, count));
                }
                else
                {
                    // slightly larger to avoid sus knockback
                    hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, kb.Origin, 17f, 20f), act);
                }
            }
        }
    }
}
