namespace BossMod.Shadowbringers.Alliance.A24TheCompound2P;

sealed class MechanicalLaceration(BossModule module) : Components.RaidwideCast(module, (uint)AID.MechanicalLaceration);
sealed class MechanicalDissection(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MechanicalDissection, new AOEShapeRect(85f, 5.5f));
sealed class MechanicalDecapitation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MechanicalDecapitation, new AOEShapeDonut(8f, 43f));
sealed class MechanicalContusionAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MechanicalContusionAOE, 6f);
sealed class MechanicalContusionSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MechanicalContusionSpread, 6f);
sealed class IncongruousSpin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncongruousSpin, new AOEShapeRect(80f, 75f));

sealed class MechanicalLacerationPhaseChange(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _kb;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _kb);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Compound2P)
        {
            _kb = new(Arena.Center.Quantized(), 10f, WorldState.FutureTime(3.8d), ignoreImmunes: true);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MechanicalLacerationPhaseChange)
        {
            _kb = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb is Knockback kb)
        {
            var center = Arena.Center;
            // square intentionally slightly smaller to prevent sus knockback
            hints.AddForbiddenZone(p =>
            {
                if ((p + 10f * (p - center).Normalized()).InSquare(center, 28f))
                {
                    return 1f;
                }
                return default;
            }, kb.Activation);
        }
    }
}
