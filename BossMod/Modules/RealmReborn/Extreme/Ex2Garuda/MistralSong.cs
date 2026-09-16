namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

abstract class MistralSong : Components.GenericLineOfSightAOE
{
    private readonly WPos _predictedPosition;

    public MistralSong(BossModule module, WPos predictedPosition) : base(module, (uint)AID.MistralSong, 31.7f, true)
    {
        _predictedPosition = predictedPosition;
        Modify(_predictedPosition, ActiveBlockers());
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Modify(caster.Position, ActiveBlockers());
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Modify(null, ActiveBlockers());
    }

    private IEnumerable<(WPos, float)> ActiveBlockers() => Module.Enemies((uint)OID.Monolith).Where(a => !a.IsDead).Select(a => (a.Position, a.HitboxRadius - 0.5f));
}
sealed class MistralSong1(BossModule module) : MistralSong(module, new(0f, -13f));
sealed class MistralSong2(BossModule module) : MistralSong(module, new(13f, 0f));
