namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoard.BoneBishop;

public enum OID : uint
{
    BoneKnight = 0x4B86, // R0.900, x?
    BoneBishop = 0x4B87, // R0.900, x?
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50784, // BoneKnight->player, no cast, single-target

    Blizzard = 50788, // BoneBishop->player, no cast, single-target
    DeathSpiral = 46867, // BoneBishop->self, 5.0s cast, single-target
    DeathSpiral1 = 46868, // Helper->self, 6.0s cast, range 4-40 donut
    Ossify = 46871, // BoneKnight->self, 8.0s cast, single-target
    ForwardGuard = 46864, // BoneKnight->self, 5.0s cast, single-target
    BlackEruption = 46873, // BoneBishop->self, 5.0+1.0s cast, single-target
    BlackEruption1 = 46874, // Helper->location, 6.0s cast, range 5 circle
    BlackEruption2 = 46900, // Helper->location, 1.5s cast, range 5 circle
    AncientAero = 46869, // BoneBishop->self, 5.0+0.7s cast, single-target
    AncientAero1 = 46870, // Helper->self, 5.7s cast, range 40 width 8 rect
    _Ability_ = 46865, // BoneKnight->self, no cast, single-target
    Tumulus = 46866, // BoneKnight->self, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    PhysicalDamageUp = 2074, // BoneKnight->BoneKnight, extra=0x0
    DirectionalParry = 680, // BoneKnight->BoneKnight, extra=0x1
    _Gen_ = 2552, // BoneKnight->BoneKnight, extra=0x425 : Ossify maybe?
    Rehabilitation = 1263, // BoneKnight->BoneKnight, extra=0x0
}

sealed class DeathSpiral(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathSpiral1, new AOEShapeDonut(4f, 40f));

// Puts a shield in front of himself. Player should have pet snarl why they get behind and beat him up.
sealed class ForwardGuard(BossModule module) : Components.DirectionalParry(module, [(uint)OID.BoneKnight])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ForwardGuard)
        {
            PredictParrySide(caster.InstanceID, Side.Front);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DirectionalParry)
        {
            UpdateState(actor.InstanceID, 0);
        }
    }
}

sealed class AncientAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientAero1, new AOEShapeRect(40f, 4f));

sealed class Tumulus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tumulus, new AOEShapeCircle(6f));
sealed class BlackEruption(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle _circle = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var count = aoes.Length;
        var max = count > 4 ? 4 : count;
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlackEruption1)
        {
            _aoes.Add(new(_circle, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlackEruption1)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
            if (!caster.IsDeadOrDestroyed)
            {
                var position = spell.LocXZ;
                var rotation = caster.Rotation;
                var distance = 3f;
                for (var i = 1; i <= 7; i++)
                {
                    for (var j = 0; j < 4; j++)
                    {
                        var rot = rotation + (j * 90f).Degrees();
                        var dir = rot.ToDirection() * distance * i;
                        _aoes.Add(new(_circle, position + dir, rot, Module.CastFinishAt(spell).AddSeconds(i * 2.5d)));
                    }
                }
            }
        }
        else if (spell.Action.ID == (uint)AID.BlackEruption2)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.BoneBishop)
        {
            _aoes.Clear();
        }
    }
}

sealed class BoneBishopStates : StateMachineBuilder
{
    public BoneBishopStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeathSpiral>()
            .ActivateOnEnter<ForwardGuard>()
            .ActivateOnEnter<BlackEruption>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<Tumulus>()
            .Raw.Update = () => AllDeadOrDestroyed([(uint)OID.BoneKnight, (uint)OID.BoneBishop]);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.BoneBishop, Contributors = "wen", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1088u, NameID = 14532u, SortOrder = 3)]
public sealed class BoneBishop(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.BoneKnight));
    }
}
