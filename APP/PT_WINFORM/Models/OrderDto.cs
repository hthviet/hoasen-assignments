namespace PT_WINFORM.Models;

public sealed class OrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}

public sealed class OrderItemDto
{
    public int Quantity { get; set; }
}
