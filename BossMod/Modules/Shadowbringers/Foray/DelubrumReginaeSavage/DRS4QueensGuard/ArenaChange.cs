namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(25f, 30f);
    private AOEInstance? _aoe;
    private bool startingArena = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void Update()
    {
        if (startingArena)
        {
            var features = Module.Enemies((uint)OID.ArenaFeatures);
            var count = features.Count;
            for (var i = 0; i < count; ++i)
            {
                var f = features[i];
                if (f.EventState == default && f.Position.AlmostEqual(new(244f, -129f), 1f))
                {
                    _aoe = new(donut, Arena.Center, default, WorldState.FutureTime(5d));
                    return;
                }
            }
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001u && index == 0x18u)
        {
            Arena.Bounds = QueensGuard.DefaultArena;
            Arena.Center = QueensGuard.DefaultArena.Center;
            _aoe = null;
            startingArena = false;
        }
    }
}
