using System.ComponentModel.DataAnnotations;

namespace PT_WEB.Models.ViewModels;

public class CheckoutViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}