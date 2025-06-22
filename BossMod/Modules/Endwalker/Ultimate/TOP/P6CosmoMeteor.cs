namespace BossMod.Endwalker.Ultimate.TOP;

class P6CosmoMeteorPuddles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CosmoMeteorAOE, 10f);

class P6CosmoMeteorAddComet(BossModule module) : Components.Adds(module, (uint)OID.CosmoComet);

class P6CosmoMeteorAddMeteor(BossModule module) : Components.Adds(module, (uint)OID.CosmoMeteor);

class P6CosmoMeteorSpread : Components.UniformStackSpread
{
    public int NumCasts;

    public P6CosmoMeteorSpread(BossModule module) : base(module, 0, 5)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.CosmoMeteorSpread)
            ++NumCasts;
    }
}

class P6CosmoMeteorFlares(BossModule module) : Components.UniformStackSpread(module, 6f, 20f, 5, alwaysShowSpreads: true) // TODO: verify flare falloff
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.OptimizedMeteor)
        {
            AddSpread(actor, WorldState.FutureTime(8.1d));
            if (Spreads.Count == 3)
            {
                // TODO: how is the stack target selected?
                var stackTarget = Raid.WithoutSlot(false, true, true).FirstOrDefault(p => !IsSpreadTarget(p));
                if (stackTarget != null)
                    AddStack(stackTarget, WorldState.FutureTime(8.1f));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CosmoMeteorStack or (uint)AID.CosmoMeteorFlare)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}
