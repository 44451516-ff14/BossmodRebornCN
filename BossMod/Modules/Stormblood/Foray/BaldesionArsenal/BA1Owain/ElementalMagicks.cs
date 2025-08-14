namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Owain;

sealed class ElementalMagicks(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(13f);
    public readonly List<AOEInstance> AOEs = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOEs(uint sid)
        {
            var mundberg = Module.Enemies((uint)OID.Munderg);
            var activation = Module.CastFinishAt(spell);
            for (var i = 0; i < mundberg.Count; ++i)
            {
                var spear = mundberg[i];
                if (spear.FindStatus(sid) != null)
                    AOEs.Add(new(circle, spear.Position.Quantized(), default, activation));
            }
            AOEs.Add(new(circle, spell.LocXZ, default, activation));
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.ElementalMagicksFireBoss:
                AddAOEs((uint)SID.SoulOfFire);
                break;
            case (uint)AID.ElementalMagicksIceBoss:
                AddAOEs((uint)SID.SoulOfIce);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.ElementalMagicksFireBoss or (uint)AID.ElementalMagicksFireSpears or (uint)AID.ElementalMagicksIceBoss or (uint)AID.ElementalMagicksIceSpears)
            AOEs.RemoveAt(0);
    }
}
