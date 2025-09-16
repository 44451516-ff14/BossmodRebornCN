namespace BossMod.Dawntrail.Raid.M08NHowlingBlade;

sealed class WolvesReignRect1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WolvesReignRect1, new AOEShapeRect(36f, 5f));
sealed class WolvesReignRect2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WolvesReignRect2, new AOEShapeRect(28f, 5f));

sealed class WolvesReignCone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40f, 60f.Degrees());
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WolvesReignTeleport2 or (uint)AID.WolvesReignTeleport4)
        {
            var pos = spell.LocXZ;
            _aoe = [new(cone, pos, Angle.FromDirection(Arena.Center - pos), Module.CastFinishAt(spell, 5.1d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WolvesReignCone1 or (uint)AID.WolvesReignCone2)
        {
            _aoe = [];
        }
    }
}

sealed class WolvesReignCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WolvesReignCircle1, (uint)AID.WolvesReignCircle2,
(uint)AID.WolvesReignCircle3, (uint)AID.WolvesReignCircle4, (uint)AID.WolvesReignCircle5, (uint)AID.WolvesReignCircle6, (uint)AID.WolvesReignCircle7, (uint)AID.WolvesReignCircle8], 6f);
