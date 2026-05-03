namespace PT_WINFORM.Business;

using PT_WINFORM.Models;

public sealed class CartLine
{
    public CartLine(ProductDto product, int quantity)
    {
        Product = product;
        Quantity = quantity;
    }

    public ProductDto Product { get; }
    public int Quantity { get; set; }
    public decimal LineTotal => Quantity * Product.Price;
}

public interface ICartManager
{
    void AddProduct(ProductDto product, int quantity);
    void RemoveProduct(int productId);
    void Clear();
    List<CartLine> GetItems();
    decimal GetTotal();
    int GetItemCount();
}

public class CartManager : ICartManager
{
    private readonly List<CartLine> _items = new();

    public void AddProduct(ProductDto product, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));
        }

        var existing = _items.FirstOrDefault(c => c.Product.Id == product.Id);
        
        if (existing is null)
        {
            _items.Add(new CartLine(product, quantity));
        }
        else
        {
            existing.Quantity += quantity;
        }
    }

    public void RemoveProduct(int productId)
    {
        _items.RemoveAll(line => line.Product.Id == productId);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public List<CartLine> GetItems() => _items;

    public decimal GetTotal() => _items.Sum(c => c.LineTotal);

    public int GetItemCount() => _items.Sum(c => c.Quantity);
}
