namespace PT_WEB.Models.ViewModels;

public class CartPageViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}