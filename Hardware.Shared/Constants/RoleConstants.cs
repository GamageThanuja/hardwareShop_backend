namespace Hardware.Shared.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";
    public const string StoreKeeper = "StoreKeeper";
    public const string User = "User";

    public static readonly string[] All = [Admin, Manager, Cashier, StoreKeeper, User];

    public static readonly string[] AdminRoles = [Admin];
    public static readonly string[] ManagerRoles = [Admin, Manager];
    public static readonly string[] CashierRoles = [Admin, Manager, Cashier];
    public static readonly string[] StoreKeeperRoles = [Admin, Manager, StoreKeeper];
    public static readonly string[] StaffRoles = [Admin, Manager, Cashier, StoreKeeper];
}
