namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class UltraviolentRay(BossModule module) : Components.GenericBaitAway(module, (uint)AID.UltraviolentRay, onlyShowOutlines: true, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private static readonly AOEShapeRect rect = new(40f, 8.5f);
    private readonly M08SHowlingBlade bossmod = (M08SHowlingBlade)module;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.UltraviolentRay)
        {
            CurrentBaits.Add(new(bossmod.BossP2()!, actor, rect, WorldState.FutureTime(6.1d)));
        }
    }

    public override void Update()
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            for (var j = 0; j < 5; ++j)
            {
                var center = ArenaChanges.EndArenaPlatforms[j].Center;
                if (b.Target.Position.InCircle(center, 8f))
                {
                    b.CustomRotation = ArenaChanges.PlatformAngles[j];
                    break;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;

        Angle playerPlatform = default;
        for (var i = 0; i < 5; ++i)
        {
            if (actor.Position.InCircle(ArenaChanges.EndArenaPlatforms[i].Center, 8f))
            {
                playerPlatform = ArenaChanges.PlatformAngles[i];
                break;
            }
        }
        var occupiedPlatforms = new List<Angle>(5);
        for (var i = 0; i < len; ++i)
        {
            ref readonly var b = ref baits[i];
            for (var j = 0; j < 5; ++j)
            {
                if (b.Target.Position.InCircle(ArenaChanges.EndArenaPlatforms[j].Center, 8f))
                {
                    var count = occupiedPlatforms.Count;
                    var angle = ArenaChanges.PlatformAngles[j];
                    for (var k = 0; k < count; ++k)
                    {
                        if (occupiedPlatforms[k] == angle && playerPlatform == angle)
                        {
                            hints.Add("More than 1 defamation on your platform!");
                            return;
                        }
                    }
                    occupiedPlatforms.Add(angle);
                }
            }
        }
    }
}

sealed class GleamingBeam(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeRect Rect = new(31f, 4f);
    private readonly List<AOEInstance> _aoes = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D3 && actor.OID == (uint)OID.GleamingFangP21)
        {
            _aoes.Add(new(Rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(6.1d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GleamingBeam)
        {
            ++NumCasts;
        }
    }
}
