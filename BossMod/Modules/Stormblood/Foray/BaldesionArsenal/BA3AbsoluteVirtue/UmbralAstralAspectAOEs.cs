namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA3AbsoluteVirtue;

sealed class BrightDarkAurora(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(30f, 50f);
    public readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE() => _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        switch (spell.Action.ID)
        {
            case (uint)AID.DarkAurora1:
            case (uint)AID.DarkAurora2:
                if (caster.FindStatus((uint)SID.UmbralEssence) != null)
                    AddAOE();
                break;
            case (uint)AID.BrightAurora1:
            case (uint)AID.BrightAurora2:
                if (caster.FindStatus((uint)SID.AstralEssence) != null)
                    AddAOE();
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.BrightAurora1 or (uint)AID.BrightAurora2) // bright and dark always happen in a pair and we only add one of them to active AOEs
            _aoes.RemoveAt(0);
    }
}

sealed class BrightDarkAuroraCounter(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.DarkAurora1, (uint)AID.BrightAurora1,
(uint)AID.DarkAurora2, (uint)AID.BrightAurora2]);

sealed class AstralUmbralRays(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circleSmall = new(8f), circleBig = new(16f);
    public readonly List<AOEInstance> _aoes = new(9);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(bool big) => _aoes.Add(new(big ? circleBig : circleSmall, spell.LocXZ, default, Module.CastFinishAt(spell)));
        switch (spell.Action.ID)
        {
            case (uint)AID.UmbralRays1:
            case (uint)AID.UmbralRays2:
                AddAOE(Module.PrimaryActor.FindStatus((uint)SID.UmbralEssence) != null);
                break;
            case (uint)AID.AstralRays1:
            case (uint)AID.AstralRays2:
                AddAOE(Module.PrimaryActor.FindStatus((uint)SID.AstralEssence) != null);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.UmbralRays1 or (uint)AID.UmbralRays2 or (uint)AID.AstralRays1 or (uint)AID.AstralRays2)
            _aoes.Clear();
    }
}
