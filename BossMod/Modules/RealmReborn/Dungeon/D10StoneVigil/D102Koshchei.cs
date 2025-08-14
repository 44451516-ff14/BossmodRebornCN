namespace BossMod.RealmReborn.Dungeon.D10StoneVigil.D102Koshchei;

public enum OID : uint
{
    Boss = 0x38C7, // R2.88
    MaelstromVisual = 0x38C8, // R0.8
    MaelstromHelper = 0x38D0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    SpikedTail = 28732, // Boss->player, 5.0s cast, single-target, tankbuster
    SonicStorm = 29053, // Boss->location, 3.0s cast, range 6 circle
    Typhoon = 28730, // Boss->self, 3.0s cast, single-target, visual
    TyphoonAOE = 28731 // MaelstromHelper->self, no cast, range 3 circle
}

class SpikedTail(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SpikedTail);
class SonicStorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SonicStorm, 6f);

class Typhoon(BossModule module) : Components.Exaflare(module, 3f)
{
    private readonly List<Actor> _maelstroms = module.Enemies((uint)OID.MaelstromVisual);

    public override void Update()
    {
        var count = _maelstroms.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var m = _maelstroms[i];
            var line = FindLine(m.Position.Z);
            if (m.IsDead && line != null)
                Lines.Remove(line);
            else if (!m.IsDead && line == null)
                Lines.Add(new(m.Position, new(-1.745f, default), default, 0.6d, 4, 4));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TyphoonAOE && caster.Position.X < 56f)
        {
            var line = FindLine(caster.Position.Z);
            if (line == null)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X} @ {caster.Position}");
                return;
            }

            if (line.MaxShownExplosions <= 4)
            {
                // first move
                line.MaxShownExplosions = 10;
                line.ExplosionsLeft = 15;
            }
            AdvanceLine(line, caster.Position);
        }
    }

    private Line? FindLine(float z) => Lines.Find(l => Math.Abs(l.Next.Z - z) < 1f);
}

class D102KoshcheiStates : StateMachineBuilder
{
    public D102KoshcheiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpikedTail>()
            .ActivateOnEnter<SonicStorm>()
            .ActivateOnEnter<Typhoon>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 11, NameID = 1678)]
public class D102Koshchei(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly Shape[] union = [new Rectangle(new(44f, -80f), 13.5f, 10.5f), new Rectangle(new(30.1f, -80), 0.4f, 4.5f),
    new Square(new(30.4f, -75.4f), 0.2f), new Square(new(30.4f, -84.6f), 0.2f)];
    public static readonly ArenaBoundsComplex arena = new(union);
}
