namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for first lightwave (2 waves, 4 crystals) mechanic
// first we wait until we find two helpers with Z=70 - these are our lightwaves
class Lightwave1(BossModule module) : LightwaveCommon(module)
{
    private WPos _safeCrystal;
    private WPos _firstHitCrystal;
    private WPos _secondHitCrystal;
    private WPos _thirdHitCrystal;

    public override void Update()
    {
        // try to find two helpers with Z=70 before first cast
        if (Waves.Count == 0)
        {
            foreach (var wave in Module.Enemies((uint)OID.Helper).Where(a => a.PosRot.Z < 71f))
            {
                Waves.Add(wave);
            }

            if (Waves.Count > 0)
            {
                var leftWave = Waves.Any(w => w.PosRot.X < 90f);
                _safeCrystal = new(leftWave ? 110f : 90f, 92f);
                _firstHitCrystal = new(100f, 86f);
                _secondHitCrystal = new(leftWave ? 90f : 110f, 92f);
                _thirdHitCrystal = new(100f, 116f);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Waves.Count == 0)
            return;

        if (Waves.Any(w => WaveAOE.Check(actor.Position, w)))
            hints.Add("GTFO from wave!");

        var safe = NumCasts switch
        {
            0 => InSafeCone(_firstHitCrystal, _safeCrystal, actor.Position),
            1 => InSafeCone(_secondHitCrystal, _safeCrystal, actor.Position),
            2 => InSafeCone(_thirdHitCrystal, _safeCrystal, actor.Position) || InSafeCone(_thirdHitCrystal, _firstHitCrystal, actor.Position) || InSafeCone(_thirdHitCrystal, _secondHitCrystal, actor.Position),
            _ => true
        };
        if (!safe)
            hints.Add("Hide behind crystal!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Waves.Count == 0)
            return;

        foreach (var wave in Waves)
            WaveAOE.Draw(Arena, wave);

        switch (NumCasts)
        {
            case 0:
                DrawSafeCone(_firstHitCrystal, _safeCrystal);
                break;
            case 1:
                DrawSafeCone(_secondHitCrystal, _safeCrystal);
                break;
            case 2:
                DrawSafeCone(_thirdHitCrystal, _safeCrystal);
                DrawSafeCone(_thirdHitCrystal, _firstHitCrystal);
                DrawSafeCone(_thirdHitCrystal, _secondHitCrystal);
                break;
        }
    }
}
