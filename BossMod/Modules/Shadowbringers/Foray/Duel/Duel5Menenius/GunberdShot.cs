﻿namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class GunberdShot(BossModule module) : BossComponent(module)
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
        switch ((AID)spell.Action.ID)
        {
            case AID.DarkShot:
                DarkShotLoaded = true;
                break;
            case AID.WindslicerShot:
                WindslicerLoaded = true;
                break;
            case AID.GunberdDark:
            case AID.GunberdWindslicer:
                Gunberding = true;
                _gunberdCaster = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GunberdDark:
                DarkShotLoaded = false;
                Gunberding = false;
                break;
            case AID.GunberdWindslicer:
                WindslicerLoaded = false;
                Gunberding = false;
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Gunberding && WindslicerLoaded)
        {
            var adjPos = Components.GenericKnockback.AwayFromSource(pc.Position, _gunberdCaster, 10);
            Components.GenericKnockback.DrawKnockback(pc, adjPos, Arena);
        }
    }
}
