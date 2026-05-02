namespace PT_WEB.Models;

public static class OrderStatus
{
    public const string New = "New";
    public const string Shipped = "Shipped";
    public const string Paid = "Paid";

    public static readonly string[] All = [New, Shipped, Paid];
}