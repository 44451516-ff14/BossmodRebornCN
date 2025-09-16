namespace BossMod;

// shapes can be defined by distance from point to shape's border; distance is positive for points outside shape and negative for points inside shape
// union is min, intersection is max
// NOTE: some of these are not returning the true distance, for example knockback related SDs and return only 0 for forbidden and 1 for allowed. best to add 1y safety margin to cover all points in a cell

public abstract class ShapeDistance
{
    public abstract float Distance(WPos p);
}
