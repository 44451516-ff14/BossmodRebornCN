namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME1TwoHeadedAevis;

[SkipLocalsInit]
sealed class HissingResonance(BossModule module) : Components.GenericKnockback(module)
{
    // knockbacks during CrossBlazeLoop, happens after BlazeFirst or BlazeFollowup cast (get knocked into donut / avoid cross)
    // knockback x2 only when color boss is casting
    private readonly Kind[] _green = new Kind[PartyState.MaxAllies];
    private readonly Kind[] _blue = new Kind[PartyState.MaxAllies];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var hissing = GetHissingInfo(slot);
        if (hissing.Kind == Kind.None || hissing.AOEs.Length < 2)
        {
            return [];
        }

        var posx = hissing.Kind == Kind.DirRight ? -880f : 920f;
        WPos origin = new(posx, Arena.Center.Z);
        var aoe1 = hissing.AOEs[0];
        Knockback[] kb = [new(origin, 10f, aoe1.Activation, kind: hissing.Kind)];

        return kb;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.GreenNoiseEasterly or (uint)SID.GreenNoiseWesterly or (uint)SID.BlueNoiseEasterly or (uint)SID.BlueNoiseWesterly)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot == -1)
            {
                return;
            }

            var direction = status.ID is (uint)SID.GreenNoiseEasterly or (uint)SID.BlueNoiseEasterly ? Kind.DirRight : Kind.DirLeft;
            var color = status.ID is (uint)SID.GreenNoiseEasterly or (uint)SID.GreenNoiseWesterly ? _green : _blue;
            color[slot] = direction;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Buffet1:
            case (uint)AID.Buffet2:
            case (uint)AID.Buffet3:
            case (uint)AID.Buffet4:
                ++NumCasts;
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = CalculateMovements(slot, actor);
        var count = movements.Count;
        for (var i = 0; i < count; ++i)
        {
            var movement = movements[i];
            if (DestinationUnsafe(slot, actor, movement.to))
            {
                hints.Add("Get moved into safety!");
                break;
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var hissing = GetHissingInfo(slot);
        if (hissing.Kind == Kind.None || hissing.AOEs.Length < 2)
        {
            return base.DestinationUnsafe(slot, actor, pos);
        }

        var aoe = hissing.AOEs[1];
        return base.DestinationUnsafe(slot, actor, pos) || aoe.Check(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var hissing = GetHissingInfo(slot);
        if (hissing.Kind == Kind.None || hissing.AOEs.Length < 2)
        {
            return;
        }

        var direction = new WDir(hissing.Kind == Kind.DirLeft ? 10f : -10f, 0f);
        var aoe1 = hissing.AOEs[0];
        var aoe2 = hissing.AOEs[1];

        if (!IsImmune(slot, aoe1.Activation))
        {
            switch (aoe2.Shape)
            {
                case AOEShapeDonut:
                    hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirectionIntoCircle(Arena.Center, direction, 19f, aoe2.Origin, 5f), aoe1.Activation);
                    break;
                case AOEShapeCross:
                    hints.AddForbiddenZone(new SDKnockbackInAABBSquareFixedDirectionPlusMixedAOEs(Arena.Center, direction, 19f, [aoe2], 1), aoe1.Activation);
                    break;
            }
        }
    }

    // return AOEInstance to use for cross kb shapedistance
    private (Kind Kind, Components.GenericAOEs.AOEInstance[] AOEs) GetHissingInfo(int slot)
    {
        var blazes = Module.FindComponent<CrossBlazeLoop>();

        if (blazes == null || blazes.ActiveCasters.Length < 2)
        {
            return (Kind.None, []);
        }

        var aoe1 = blazes.ActiveCasters[0];
        var casterId = aoe1.ActorID;
        var caster = WorldState.Actors.Find(casterId);
        if (caster?.OID is not ((uint)OID.GreenHead1) and not ((uint)OID.BlueHead1))
        {
            return (Kind.None, []);
        }

        var kind = caster.OID == (uint)OID.GreenHead1 ? _green : _blue;
        if (kind[slot] == Kind.None)
        {
            return (Kind.None, []);
        }

        var aoe2 = blazes.ActiveCasters[1];

        return (kind[slot], [aoe1, aoe2]);
    }
}
