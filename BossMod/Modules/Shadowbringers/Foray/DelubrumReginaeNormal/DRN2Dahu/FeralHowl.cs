namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN2Dahu;

sealed class FeralHowl(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FeralHowl, 30f, true, stopAtWall: true)
{
    private readonly HuntersClaw _aoe = module.FindComponent<HuntersClaw>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoe.Casters.Count;
        if (count == 0)
            return;
        if (Casters.Count != 0)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
            var forbidden = new Func<WPos, float>[count];
            var pos = Module.PrimaryActor.Position;
            for (var i = 0; i < count; ++i)
            {
                var a = aoes[i].Origin;
                forbidden[i] = ShapeDistance.Cone(pos, 100f, Module.PrimaryActor.AngleTo(a), Angle.Asin(8f / (a - pos).Length()));
            }
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Module.CastFinishAt(Casters[0].CastInfo));
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return false;
    }
}
