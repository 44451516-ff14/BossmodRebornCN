namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

[SkipLocalsInit]
sealed class LightAndDark(BossModule module) : Endwalker.DeepDungeon.PilgrimsTraverse.LightAndDarkBase(module)
{
    private readonly Q1FinalVerse bossmod = (Q1FinalVerse)module;
    private bool boundsOfSinTowers;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DrainAether1:
                UpdateWantedStatus(0, true);
                break;
            case (uint)AID.DrainAether2:
                UpdateWantedStatus(1, false);
                break;
            case (uint)AID.DrainAether3:
                UpdateWantedStatus(0, false);
                break;
            case (uint)AID.DrainAether4:
                UpdateWantedStatus(1, true);
                break;
        }

        void UpdateWantedStatus(int slot, bool wantL)
        {
            wantLight[slot] = wantL;
            aetherdrainActive = true;
            activations[slot] = Module.CastFinishAt(spell, -1d);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DrainAether1 or (uint)AID.DrainAether2 or (uint)AID.DrainAether3 or (uint)AID.DrainAether4)
        {
            if ((++NumCasts & 1) == 0)
            {
                aetherdrainActive = false;
            }
            wantLight[0] = wantLight[1];
            activations[0] = activations[1];
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.LightVengeance:
                UpdateStatus(ref lightBuff, actor, status.ExpireAt);
                break;
            case (uint)SID.DarkVengeance:
                UpdateStatus(ref darkBuff, actor, status.ExpireAt);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.LightVengeance:
                UpdateStatus(ref lightBuff, actor, default);
                break;
            case (uint)SID.DarkVengeance:
                UpdateStatus(ref darkBuff, actor, default);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.StateMachine.ActivePhaseIndex == -1)
        {
            var countL = lightBuff.NumSetBits();
            var countD = darkBuff.NumSetBits();
            if (!combined[slot])
            {
                if (combined.NumSetBits() == 0 || countL == 1 && countD == 1)
                {
                    hints.Add(either);
                }
                else if (countL > countD)
                {
                    hints.Add(dark);
                }
                else
                {
                    hints.Add(light);
                }
            }
            else if (darkBuff[slot] && countD > 2 || lightBuff[slot] && countL > 2)
            {
                hints.Add(switchColor);
            }
        }
        else if (boundsOfSinTowers)
        {
            hints.Add(light, !lightBuff[slot]);
        }
        else if (aetherdrainActive)
        {
            if (wantLight[0])
            {
                hints.Add(light, !lightBuff[slot]);
            }
            else
            {
                hints.Add(dark, !darkBuff[slot]);
            }
        }
        else if (!aetherdrainActive)
        {
            if (eaterHP <= 1u && !lightBuff[slot])
            {
                hints.Add(light);
            }
            else if (griefHP <= 1u && !darkBuff[slot])
            {
                hints.Add(dark);
            }
            else if (Math.Abs(hpDifference) > 25f || !combined[slot])
            {
                if (hpDifference > 0f)
                {
                    hints.Add(dark);
                }
                else
                {
                    hints.Add(light);
                }
            }
            else
            {
                hints.Add($"Target {(darkBuff[slot] ? bossmod.BossEater?.Name : Module.PrimaryActor.Name)}!", false);
            }
        }
    }

    public override void Update()
    {
        if (bossmod.BossEater is Actor eater)
        {
            ref var eaterHPref = ref eater.HPMP;
            ref var primaryHPref = ref Module.PrimaryActor.HPMP;
            var eaterHPs = eaterHP = eaterHPref.CurHP;
            var griefHPs = griefHP = primaryHPref.CurHP;
            hpDifference = (int)(eaterHPs - griefHPs) * 100f / primaryHPref.MaxHP;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            var oid = e.Actor.OID;
            ref var hp = ref e.Actor.HPMP;
            e.Priority = hp.CurHP <= 1u ? AIHints.Enemy.PriorityInvincible : darkBuff[slot] && oid == (uint)OID.DevouredEater ? 0
                : lightBuff[slot] && oid == (uint)OID.EminentGrief ? 0 : oid is (uint)OID.VodorigaMinion or (uint)OID.BloodguardMinion ? 1 : AIHints.Enemy.PriorityInvincible;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x1B)
        {
            switch (state)
            {
                case 0x00020001u:
                    boundsOfSinTowers = true;
                    break;
                case 0x00080004u:
                    boundsOfSinTowers = false;
                    break;
            }
        }
    }
}

[SkipLocalsInit]
sealed class LightDarkNeutralize(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Dark)
        {
            Stacks.Add(new(actor, 2f, 2, 2, WorldState.FutureTime(5.1d)));
        }
    }

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        if (vfxID == 40u)
        {
            ++NumCasts;
        }
    }
}

[SkipLocalsInit]
sealed class BoundsOfSinTowers(BossModule module) : Components.GenericTowers(module, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private BitMask forbidden = ~(BitMask)default;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x1B)
        {
            switch (state)
            {
                case 0x00020001u:
                    var party = Raid.WithSlot(true, false, false);
                    var len = party.Length;
                    for (var i = 0; i < len; ++i)
                    {
                        ref var p = ref party[i];
                        if (p.Item2.FindStatus((uint)SID.LightVengeance) == null)
                        {
                            forbidden.Clear(p.Item1);
                        }
                    }
                    var act = WorldState.FutureTime(14.2d);
                    var center = Arena.Center;
                    var a45 = 45f.Degrees();
                    var a90 = 90f.Degrees();
                    for (var i = 0; i < 4; ++i)
                    {
                        Towers.Add(new(center + 11f * (i * a90 + a45).ToDirection(), 2f, 1, 1, forbidden, act));
                    }
                    break;
                case 0x00080004u:
                    ++NumCasts;
                    break;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.LightVengeance)
        {
            forbidden.Clear(Raid.FindSlot(actor.InstanceID));
            UpdateTowers();
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.LightVengeance)
        {
            forbidden.Set(Raid.FindSlot(actor.InstanceID));
            UpdateTowers();
        }
    }

    private void UpdateTowers()
    {
        var count = Towers.Count;
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            towers[i].ForbiddenSoakers = forbidden;
        }
    }
}
