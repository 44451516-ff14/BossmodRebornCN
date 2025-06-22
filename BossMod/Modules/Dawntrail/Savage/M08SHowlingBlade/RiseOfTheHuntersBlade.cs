namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class ProwlingGaleLast(BossModule module) : Components.GenericTowers(module, (uint)AID.ProwlingGaleLast1)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = Towers.Count;
        if (count > 0)
        {
            var sb = new StringBuilder(45);
            var towers = CollectionsMarshal.AsSpan(Towers);

            for (var j = 0; j < 5; ++j)
            {
                var center = ArenaChanges.EndArenaPlatforms[j].Center;

                for (var i = 0; i < count; ++i)
                {
                    ref readonly var t = ref towers[i];

                    if (t.Position.InCircle(center, 8f))
                    {
                        sb.Append($"P{j + 1}: {t.NumInside(Module)}/{t.MinSoakers}");
                        sb.Append(", ");
                        break;
                    }
                }
            }

            if (sb.Length >= 2)
                sb.Length -= 2;

            hints.Add(sb.ToString());
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var soakers = spell.Action.ID switch
        {
            (uint)AID.ProwlingGaleLast1 => 1,
            (uint)AID.ProwlingGaleLast2 => 2,
            (uint)AID.ProwlingGaleLast3 => 3,
            _ => default
        };
        if (soakers != default)
            Towers.Add(new(spell.LocXZ, 2f, soakers, soakers, default, Module.CastFinishAt(spell)));
    }
}

sealed class LamentOfTheCloseDistant(BossModule module) : BossComponent(module)
{
    private readonly (Actor, bool close)[] _partner = new (Actor, bool)[PartyState.MaxPartySize];
    public bool TethersAssigned;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_partner[slot] != default)
            hints.Add(_partner[slot].close ? "Stay close to partner!" : "Stay away from partner!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _partner[pcSlot].Item1 == player ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.GreenChains or (uint)TetherID.BlueChains)
        {
            TethersAssigned = true;
            var target = WorldState.Actors.Find(tether.Target);
            var isClose = tether.ID == (uint)TetherID.GreenChains;
            if (target != null)
            {
                SetPartner(source.InstanceID, (target, isClose));
                SetPartner(target.InstanceID, (source, isClose));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_partner[pcSlot].Item1 is var partner && partner != default)
            Arena.AddLine(pc.Position, partner.Position);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.GreenChains or (uint)TetherID.BlueChains)
        {
            SetPartner(source.InstanceID, default);
            SetPartner(tether.Target, default);
        }
    }

    private void SetPartner(ulong source, (Actor, bool) target)
    {
        var slot = Raid.FindSlot(source);
        if (slot >= 0)
            _partner[slot] = target;
    }
}
