namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

sealed class HissingReprise(BossModule module) : Components.GenericKnockback(module)
{
    private readonly LightningIcePoison lip = module.FindComponent<LightningIcePoison>()!;
    private readonly HypothermalCombustionShock hyposhock = module.FindComponent<HypothermalCombustionShock>()!;
    private readonly StormsBreath storm = module.FindComponent<StormsBreath>()!;
    private DateTime activation = default;
    private BitMask easterly;
    private BitMask westerly;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        List<Knockback> kb = [with(2)];

        if (easterly[slot] || westerly[slot])
        {
            var posx = easterly[slot] ? -880f : -920f;
            var kind = easterly[slot] ? Kind.DirRight : Kind.DirLeft;
            kb.Add(new(new(posx, Arena.Center.Z), 21f, activation, kind: kind));
            var s = storm.ActiveKnockbacks(slot, actor);
            if (s.Length != 0)
            {
                kb.Add(s[0]);
            }
        }

        return CollectionsMarshal.AsSpan(kb);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.EasterlyReprise or (uint)SID.WesterlyReprise)
        {
            activation = status.ExpireAt;
            var slot = Raid.FindSlot(actor.InstanceID);
            switch (status.ID)
            {
                case (uint)SID.EasterlyReprise:
                    easterly.Set(slot);
                    break;
                case (uint)SID.WesterlyReprise:
                    westerly.Set(slot);
                    break;
            }
        }
        base.OnStatusGain(actor, ref status);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BuffetEastern or (uint)AID.BuffetWestern)
        {
            easterly.Reset();
            westerly.Reset();
            activation = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = CalculateMovements(slot, actor);
        var count = movements.Count;
        for (var i = 0; i < count; ++i)
        {
            var movement = movements[i];
            if (DestinationUnsafe(slot, actor, movement.to) || InsideAOE(slot, actor, movement.to))
            {
                hints.Add("About to be knocked into danger!");
                break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kbs = ActiveKnockbacks(slot, actor);
        var count = kbs.Length;
        if (count != 0)
        {
            var kb = kbs[0];
            var direction = new WDir(kb.Kind == Kind.DirLeft ? 20f : -20f, 0f);
            if (count == 2)
            {
                hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirectionIntoCircle(Arena.Center, direction, 19f, Arena.Center, 5f), kbs[0].Activation);
                return;
            }
            // knockback can happen by itself, poison breath, or clusters
            // rect/circ slightly larger to avoid sus knockback            
            if (!IsImmune(slot, kb.Activation))
            {
                var aoeinfo = GetCircleAOEInfo(slot, actor);
                var origins = aoeinfo.Origins;
                var aoecount = origins.Length;
                if (aoeinfo.Origins.Length == 0)
                {
                    hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirection(Arena.Center, direction, 19f), kb.Activation);
                }
                else
                {
                    hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirectionPlusAOECircles(Arena.Center, direction, 19f, origins, aoeinfo.Radius, aoecount), kb.Activation);
                }
            }
        }
    }
    private (WPos[] Origins, float Radius) GetCircleAOEInfo(int slot, Actor actor)
    {
        // poison never happens at same time as ice/lightning
        List<WPos> pos = [];
        var radius = float.MinValue;
        var aoes = lip.ActiveAOEs(slot, actor);
        var count = aoes.Length;
        for (var i = 0; i < count; i++)
        {
            var aoe = aoes[i];
            var shape = aoe.Shape as AOEShapeCircle;
            pos.Add(aoe.Origin);
            radius = shape?.Radius > radius ? shape.Radius : radius;
        }

        var orbs = hyposhock.ActiveAOEs(slot, actor);
        var orbCount = orbs.Length;
        for (var i = 0; i < orbCount; i++)
        {
            var aoe = orbs[i];
            var shape = aoe.Shape as AOEShapeCircle;
            pos.Add(aoe.Origin);
            radius = shape?.Radius > radius ? shape.Radius : radius;
        }

        return (pos.ToArray(), radius);
    }
    private bool InsideAOE(int slot, Actor actor, WPos to)
    {
        var aoes = GetCircleAOEInfo(slot, actor);
        var count = aoes.Origins.Length;
        var radius = aoes.Radius;
        for (var i = 0; i < count; i++)
        {
            var origin = aoes.Origins[i];
            if (to.InCircle(origin, radius))
            {
                return true;
            }
        }

        return false;
    }
}
