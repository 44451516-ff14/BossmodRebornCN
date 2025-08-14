namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class Stormcall(BossModule module) : Components.GenericAOEs(module, (uint)AID.Stormcall)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeCircle circle = new(35f);
    private NorthSouthwind? _kb = module.FindComponent<NorthSouthwind>();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Count == 0 ? [] : CollectionsMarshal.AsSpan(AOEs)[..1];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Stormcall)
        {
            AOEs.Add(new(circle, (source.Position + 48f * source.Rotation.ToDirection()).Quantized(), default, WorldState.FutureTime(source.OID == (uint)OID.VorticalOrb ? 20.9d : 13.7d), actorID: source.InstanceID));
            if (AOEs.Count == 2)
            {
                AOEs.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = AOEs.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    aoe = new(circle, spell.LocXZ, default, Module.CastFinishAt(spell), actorID: id);
                    return;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID == WatchedAction)
        {
            AOEs.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _kb ??= Module.FindComponent<NorthSouthwind>();
        var kbs = _kb!.ActiveKnockbacks(slot, actor);
        if (kbs.Length != 0 && !_kb.IsImmune(slot, kbs[0].Activation))
        { }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
