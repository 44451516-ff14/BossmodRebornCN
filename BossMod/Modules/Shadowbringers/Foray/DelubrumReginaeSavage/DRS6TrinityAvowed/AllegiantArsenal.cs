namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

sealed class AllegiantArsenal(BossModule module) : Components.GenericAOEs(module)
{
    public enum Order { Unknown, SwordSecond, BowSecond, StaffSecond, StaffSwordBow, BowSwordStaff, SwordBowStaff, StaffBowSword, SwordStaffBow, BowStaffSword }

    public Order Mechanics;
    private AOEInstance? _aoe;

    public bool Active => _aoe != null;

    private static readonly AOEShapeCone cone = new(70f, 135f.Degrees());
    private static readonly AOEShapeCircle circle = new(10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AllegiantArsenalSword:
                Activate(cone, Mechanics switch
                {
                    Order.Unknown => Order.SwordSecond,
                    Order.BowSecond => Order.StaffBowSword,
                    Order.StaffSecond => Order.BowStaffSword,
                    _ => Order.Unknown
                }, 180f.Degrees());
                break;
            case (uint)AID.AllegiantArsenalBow:
                Activate(cone, Mechanics switch
                {
                    Order.Unknown => Order.BowSecond,
                    Order.SwordSecond => Order.StaffSwordBow,
                    Order.StaffSecond => Order.SwordStaffBow,
                    _ => Order.Unknown
                });
                break;
            case (uint)AID.AllegiantArsenalStaff:
                Activate(circle, Mechanics switch
                {
                    Order.Unknown => Order.StaffSecond,
                    Order.SwordSecond => Order.BowSwordStaff,
                    Order.BowSecond => Order.SwordBowStaff,
                    _ => Order.Unknown
                });
                break;
        }
        void Activate(AOEShape shape, Order newOrder, Angle offset = default)
        {
            _aoe = new(shape, spell.LocXZ, spell.Rotation + offset, Module.CastFinishAt(spell, 5.2f));
            if (newOrder != Order.Unknown)
                Mechanics = newOrder;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.InfernalSlash or (uint)AID.Flashvane or (uint)AID.FuryOfBozja)
            _aoe = null;
    }
}
