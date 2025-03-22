namespace BossMod.Components;

// generic 'concentric aoes' component - a sequence of aoes (typically cone then donuts) with same origin and increasing size
public class ConcentricAOEs(BossModule module, AOEShape[] shapes, bool showall = false) : GenericAOEs(module)
{
    public struct Sequence
    {
        public WPos Origin;
        public Angle Rotation;
        public DateTime NextActivation;
        public int NumCastsDone;
    }

    public readonly AOEShape[] Shapes = shapes;
    public readonly List<Sequence> Sequences = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Sequences.Count;
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var s = Sequences[i];
            if (!showall)
                aoes[i] = new(Shapes[s.NumCastsDone], s.Origin, s.Rotation, s.NextActivation);
            else
            {
                var len = Shapes.Length;
                for (var j = s.NumCastsDone; j < len; ++j)
                    aoes[i] = new(Shapes[j], s.Origin, s.Rotation, s.NextActivation);
            }
        }
        return aoes;
    }

    public void AddSequence(WPos origin, DateTime activation = default, Angle rotation = default) => Sequences.Add(new() { Origin = origin, Rotation = rotation, NextActivation = activation });

    // return false if sequence was not found
    public bool AdvanceSequence(int order, WPos origin, DateTime activation = default, Angle rotation = default)
    {
        if (order < 0)
            return true; // allow passing negative as a no-op

        ++NumCasts;

        var index = Sequences.FindIndex(s => s.NumCastsDone == order && s.Origin.AlmostEqual(origin, 1f) && s.Rotation.AlmostEqual(rotation, 0.05f));
        if (index < 0)
            return false;

        ref var s = ref Sequences.AsSpan()[index];
        ++s.NumCastsDone;
        s.NextActivation = activation;
        if (s.NumCastsDone == Shapes.Length)
            Sequences.Remove(s);
        return true;
    }
}
