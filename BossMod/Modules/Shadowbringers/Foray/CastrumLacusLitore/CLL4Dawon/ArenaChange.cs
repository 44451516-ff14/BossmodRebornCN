using static BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon.CLL4Dawon;

namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donutDawon = new(30f, 35f);
    private static readonly AOEShapeDonut donutLyon = new(20f, 25f);
    private AOEInstance[] _aoe = [];
    private bool lyonDeathwall;
    private bool dawonDeathwall;
    public bool IsDawonArena = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MoltingPlumage && !dawonDeathwall)
        {
            AddAOE(donutDawon, 0.2d);
        }
        else if (!IsDawonArena && spell.Action.ID == (uint)AID.RagingWindsVisual1 && !lyonDeathwall)
        {
            AddAOE(donutLyon, 1.2d);
        }
        void AddAOE(AOEShapeDonut shape, double delay) => _aoe = [new(shape, Arena.Center, default, Module.CastFinishAt(spell, delay))];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DeathwallDawon)
        {
            Arena.Bounds = DawonDefaultArena;
            Arena.Center = DawonCenter;
            _aoe = [];
            dawonDeathwall = true;
        }
        else if (actor.OID == (uint)OID.DeathwallLyon)
        {
            lyonDeathwall = true;
            _aoe = [];
            if (!IsDawonArena)
                Arena.Bounds = LyonDefaultArena;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (!IsDawonArena && DawonStartingArena.Contains(pc.Position - DawonStartingArena.Center))
        {
            IsDawonArena = true;
            Arena.Center = DawonCenter;
            Arena.Bounds = DawonDefaultArena;
        }
        else if (IsDawonArena && LyonStartingArena.Contains(pc.Position - LyonStartingArena.Center))
        {
            IsDawonArena = false;
            Arena.Center = LyonCenter;
            Arena.Bounds = lyonDeathwall ? LyonDefaultArena : LyonStartingArena;
        }
    }
}
