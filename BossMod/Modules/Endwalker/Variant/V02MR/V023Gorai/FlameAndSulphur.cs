namespace BossMod.Endwalker.VariantCriterion.V02MR.V023Gorai;

class FlameAndSulphur(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeRect _shapeFlameExpand = new(46f, 5f);
    private static readonly AOEShapeRect _shapeFlameSplit = new(46f, 2.5f);
    private static readonly AOEShapeCircle _shapeRockExpand = new(11f);
    private static readonly AOEShapeDonut _shapeRockSplit = new(5f, 16f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell, 3.1f);
        switch (spell.Action.ID)
        {
            case (uint)AID.BrazenBalladExpanding:
                AddAOEs(_shapeFlameExpand, (uint)OID.FlameAndSulphurFlame);
                AddAOEs(_shapeRockExpand, (uint)OID.FlameAndSulphurRock);
                break;
            case (uint)AID.BrazenBalladSplitting:
                AddAOEs(_shapeFlameSplit, (uint)OID.FlameAndSulphurFlame, true);
                AddAOEs(_shapeRockSplit, (uint)OID.FlameAndSulphurRock);
                break;
        }

        void AddAOEs(AOEShape shape, uint actors, bool twice = false)
        {
            var enemies = Module.Enemies(actors);
            var count = enemies.Count;

            if (!twice)
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    AddAOE(shape, enemy.Position, enemy.Rotation);
                }
            else
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    var rot = enemy.Rotation;
                    var offset = rot.ToDirection().OrthoL() * 7.5f;
                    AddAOE(shape, enemy.Position + offset, rot);
                    AddAOE(shape, enemy.Position - offset, rot);
                }
            void AddAOE(AOEShape shape, WPos origin, Angle rotation) => _aoes.Add(new(shape, WPos.ClampToGrid(origin), rotation, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FireSpreadExpand or (uint)AID.FireSpreadSplit or (uint)AID.FallingRockExpand or (uint)AID.FallingRockSplit)
            _aoes.Clear();
    }
}
