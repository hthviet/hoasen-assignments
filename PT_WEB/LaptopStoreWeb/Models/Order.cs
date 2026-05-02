using System.ComponentModel.DataAnnotations;

namespace PT_WEB.Models;

public class Order
{
    public int Id { get; set; }

    public int UserAccountId { get; set; }

    public UserAccount? UserAccount { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = OrderStatus.New;

    [StringLength(250)]
    public string ShippingAddress { get; set; } = string.Empty;

    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public List<OrderItem> OrderItems { get; set; } = new();
}