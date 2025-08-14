namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

sealed class RuinousPseudoomen(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeRect rect = new(100f, 12f);
    private static readonly uint[] diabolicGates = [(uint)OID.DiabolicGate1, (uint)OID.DiabolicGate2, (uint)OID.DiabolicGate3, (uint)OID.DiabolicGate4];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = count > 2 ? 2 : count;
        if (count > 1)
        {
            aoes[0].Color = Colors.Danger;
            var time = WorldState.CurrentTime;
            for (var i = 0; i < max; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Risky = aoe.Activation.AddSeconds(-5d) <= time;
            }
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.RuinousPseudomenVisual1)
        {
            var rot = spell.Rotation;
            var pos = spell.LocXZ;
            var act = Module.CastFinishAt(spell, 1.5d);
            AddAOE(ref pos, ref rot, ref act);
            var gates = Module.Enemies(diabolicGates);
            for (var i = 0; i < 8; ++i)
            {
                if (rot == gates[i].Rotation)
                {
                    gates.RemoveAt(i);
                    break;
                }
            }
            var dir1 = rot.ToDirection();
            for (var i = 0; i < 7; ++i)
            {
                var gate = gates[i];
                if (gate.Position.InRect(pos, dir1, 100f, default, 3f))
                {
                    var oid = gate.OID;
                    var gates2 = Module.Enemies(oid);
                    gates2.Remove(gate);
                    var gate2 = gates2[0];
                    var pos2 = gate2.Position;
                    var rot2 = gate2.Rotation;
                    var act2 = act.AddSeconds(4d);
                    AddAOE(ref pos2, ref rot2, ref act2);
                    var dir2 = rot2.ToDirection();
                    for (var j = 0; j < 7; ++j)
                    {
                        var gate3 = gates[j];
                        if (gate3.OID != oid && gate3.Position.InRect(pos2, dir2, 100f, default, 3f))
                        {
                            var gates4 = Module.Enemies(gate3.OID);
                            gates4.Remove(gate3);
                            var gate4 = gates4[0];
                            var pos3 = gate4.Position;
                            var rot3 = gate4.Rotation;
                            var act3 = act2.AddSeconds(4.1d);
                            AddAOE(ref pos3, ref rot3, ref act3);
                            return; // all aoes added
                        }
                    }
                }
            }
        }
        else if (id == (uint)AID.RuinousPseudomen3)
        {
            var pos = spell.LocXZ;
            var act = Module.CastFinishAt(spell);
            AddAOE(ref pos, ref spell.Rotation, ref act);
        }
        void AddAOE(ref readonly WPos position, ref readonly Angle rotation, ref readonly DateTime activation) => _aoes.Add(new(rect, position.Quantized(), rotation, activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.RuinousPseudomen1 or (uint)AID.RuinousPseudomen2 or (uint)AID.RuinousPseudomen3)
        {
            _aoes.RemoveAt(0);
        }
    }
}
