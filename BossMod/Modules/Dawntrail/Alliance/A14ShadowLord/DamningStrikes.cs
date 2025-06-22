namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class DamningStrikes(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DamningStrikesImpact1 or (uint)AID.DamningStrikesImpact2 or (uint)AID.DamningStrikesImpact3)
        {
            Towers.Add(new(spell.LocXZ, 3f, 8, 8, default, Module.CastFinishAt(spell), caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DamningStrikesImpact1 or (uint)AID.DamningStrikesImpact2 or (uint)AID.DamningStrikesImpact3)
        {
            ++NumCasts;

            var count = Towers.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (Towers[i].ActorID == id)
                {
                    Towers.RemoveAt(i);
                    break;
                }
            }

            var party = Raid.WithSlot(false, false, true);
            var lenP = party.Length;
            BitMask forbidden = default;
            var targets = spell.Targets;
            var countT = targets.Count;
            for (var i = 0; i < countT; ++i)
            {
                for (var j = 0; j < lenP; ++j)
                {
                    ref readonly var p = ref party[j];
                    if (targets[i].ID == p.Item2.InstanceID)
                    {
                        forbidden[p.Item1] = true;
                        break;
                    }
                }
            }

            var towers = CollectionsMarshal.AsSpan(Towers);
            var len = towers.Length;
            for (var i = 0; i < len; ++i)
            {
                towers[i].ForbiddenSoakers |= forbidden;
            }
        }
    }
}
