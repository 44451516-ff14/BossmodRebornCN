namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

sealed class GunberdShot(BossModule module) : BossComponent(module)
{
    private Actor? _gunberdCaster;

    public bool DarkShotLoaded;
    public bool WindslicerLoaded;

    public bool Gunberding;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Gunberding)
        {
            if (DarkShotLoaded)
                hints.Add("Maintain Distance");
            if (WindslicerLoaded)
                hints.Add("Knockback");
        }
        else
        {
            if (DarkShotLoaded)
                hints.Add("Dark Loaded");
            if (WindslicerLoaded)
                hints.Add("Windslicer Loaded");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DarkShot:
                DarkShotLoaded = true;
                break;
            case (uint)AID.WindslicerShot:
                WindslicerLoaded = true;
                break;
            case (uint)AID.GunberdDark:
            case (uint)AID.GunberdWindslicer:
                Gunberding = true;
                _gunberdCaster = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.GunberdDark:
                DarkShotLoaded = false;
                Gunberding = false;
                break;
            case (uint)AID.GunberdWindslicer:
                WindslicerLoaded = false;
                Gunberding = false;
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Gunberding && WindslicerLoaded)
        {
            var adjPos = Components.GenericKnockback.AwayFromSource(pc.Position, _gunberdCaster, 10f);
            Components.GenericKnockback.DrawKnockback(pc, adjPos, Arena);
        }
    }
}
