//TODO: Account for Dead Wake's shortening arena.

namespace BossMod.Modules.Dawntrail.Raid.M09NVampFatale;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    private static readonly AOEShapeRect rect = new(50f, 5f);
    private static readonly WPos left = new(85f, 80f);
    private static readonly WPos right = new(115f, 80f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SadisticScreech)
        {
            _aoe = [new(rect, left, default, WorldState.FutureTime(7d)), new(rect, right, default, WorldState.FutureTime(7d))];
        }
    }

    private float length = 20f;
    private float centerz = 100f;
    private int wakecount = 0;

    // Each time Dead Wake goes off, we need to shorten the arena by 10 and shift our center -5 Z
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeadWake1)
        {
            wakecount += 1;
            if (wakecount < 4) // that said if the 4th one happens that'd remove the arena entirely, let's not do that.
            {
                length -= 5f;
                centerz += 5f;
                Arena.Bounds = new ArenaBoundsRect(10f, length);
                Arena.Center = new WPos(100f, centerz);
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsRect(10f, 20f);
            _aoe = [];
            Arena.Center = new WPos(100f, 100f);
            //Set our wake variables when we enter or leave the smaller arena, just in case.
            length = 20f;
            centerz = 100f;
            wakecount = 0;
        }
        else if ((index == 0x00 || index == 0x10) && state == 0x00080004u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            Arena.Center = new WPos(100f, 100f);
            _aoe = [];
            //Set our wake variables when we enter or leave the smaller arena, just in case.
            length = 20f;
            centerz = 100f;
            wakecount = 0;
        }
        else if (index == 0x10 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsCircle(20f);
            Arena.Center = new WPos(100f, 100f);
        }
    }
}
