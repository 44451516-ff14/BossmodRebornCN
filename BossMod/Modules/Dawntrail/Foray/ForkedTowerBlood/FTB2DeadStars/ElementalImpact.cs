namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class ElementalImpact(BossModule module) : Components.GenericTowersOpenWorld(module)
{
    private readonly HashSet<Actor> tanks = TankSoakers(module);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ElementalImpact1 or (uint)AID.ElementalImpact2 or (uint)AID.ElementalImpact3)
        {
            Towers.Add(new(spell.LocXZ, 5f, 1, 1, tanks, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ElementalImpact1 or (uint)AID.ElementalImpact2 or (uint)AID.ElementalImpact3)
        {
            ++NumCasts;
            Towers.Clear();
        }
    }
}

sealed class FireSpread(BossModule module) : Components.GenericBaitAway(module, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private static readonly AOEShapeCone cone = new(60f, 60f.Degrees());
    private readonly List<(WPos, DateTime)> towerPositions = new(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ElementalImpact1 or (uint)AID.ElementalImpact2 or (uint)AID.ElementalImpact3)
        {
            towerPositions.Add((spell.LocXZ, Module.CastFinishAt(spell, 0.5d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FireSpread)
        {
            CurrentBaits.Clear();
            towerPositions.Clear();
            ++NumCasts;
        }
    }

    public override void Update()
    {
        var count = towerPositions.Count;
        if (count != 0)
        {
            CurrentBaits.Clear();
            List<Actor> players = new(48);
            // note: this is problematic because player culling messes this up and leads to incorrect results (eg not finding the actual bait target)
            // in any frame players can be removed or added to the object table and will likely never contain all 48 players at the same time
            // caching removed players is also pointless since the player position no longer gets updated when this happens
            foreach (var a in Module.WorldState.Actors.Actors.Values)
            {
                if (a.OID == default && a.Role != Role.Tank && !a.IsDead)
                {
                    players.Add(a);
                }
            }

            var countP = players.Count;
            for (var i = 0; i < count; ++i)
            {
                Actor? closestHealer = null;
                Actor? closestDD = null;
                var minDistSqHealer = float.MaxValue;
                var minDistSqDD = float.MaxValue;
                var tower = towerPositions[i];
                var towerPosition = tower.Item1;
                var act = tower.Item2;

                for (var j = 0; j < countP; ++j)
                {
                    var actor = players[j];
                    var distSq = (actor.Position - towerPosition).LengthSq();
                    if (actor.Role == Role.Healer && distSq < minDistSqHealer)
                    {
                        minDistSqHealer = distSq;
                        closestHealer = actor;
                    }
                    else if (actor.Class.IsDD() && distSq < minDistSqDD)
                    {
                        minDistSqDD = distSq;
                        closestDD = actor;
                    }
                }
                if (closestDD != null)
                {
                    CurrentBaits.Add(new(towerPosition, closestDD, cone, act));
                }
                if (closestHealer != null)
                {
                    CurrentBaits.Add(new(towerPosition, closestHealer, cone, act));
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // we don't want to risk AI messing this up by trying to run out of it, TODO: reconsider
        if (actor.Role == Role.Tank)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}

sealed class PhaseChange(BossModule module) : BossComponent(module)
{
    public bool PhaseChanged;

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (id == 18519u)
        {
            PhaseChanged = true;
        }
    }
}
