namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

// component dealing with tetra/octaflare mechanics (conceptual or not)
abstract class TetraOctaFlareCommon(BossModule module) : Components.UniformStackSpread(module, 3, 6, 2, 2)
{
    public enum Concept { None, Tetra, Octa }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.EmergentOctaflare or (uint)AID.EmergentTetraflare)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }

    protected void SetupMasks(Concept concept)
    {
        switch (concept)
        {
            case Concept.Tetra:
                // note that targets are either all dps or all tanks/healers, it seems to be unknown until actual cast, so for simplicity assume it will target tanks/healers (not that it matters much in practice)
                AddStacks(Raid.WithoutSlot(false, true, true).Where(a => a.Role is Role.Tank or Role.Healer));
                break;
            case Concept.Octa:
                AddSpreads(Raid.WithoutSlot(false, true, true));
                break;
        }
    }
}

sealed class TetraOctaFlareImmediate(BossModule module) : TetraOctaFlareCommon(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Octaflare:
                SetupMasks(Concept.Octa);
                break;
            case (uint)AID.Tetraflare:
                SetupMasks(Concept.Tetra);
                break;
        }
    }
}

sealed class TetraOctaFlareConceptual(BossModule module) : TetraOctaFlareCommon(module)
{
    private Concept _concept;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_concept != Concept.None)
            hints.Add(_concept == Concept.Tetra ? "Prepare to stack in pairs" : "Prepare to spread");
    }

    public void Show()
    {
        SetupMasks(_concept);
        _concept = Concept.None;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ConceptualOctaflare:
                _concept = Concept.Octa;
                break;
            case (uint)AID.ConceptualTetraflare:
                _concept = Concept.Tetra;
                break;
        }
    }
}
