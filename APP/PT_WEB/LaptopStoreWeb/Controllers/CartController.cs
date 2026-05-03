using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PT_WEB.Data;
using PT_WEB.Extensions;
using PT_WEB.Models.ViewModels;

namespace PT_WEB.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string CartSessionKey = "Cart";

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var cartItems = await BuildCartAsync();
        var viewModel = new CartPageViewModel
        {
            Items = cartItems,
            TotalAmount = cartItems.Sum(item => item.LineTotal)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        var product = await _context.Products.FirstOrDefaultAsync(item => item.Id == productId && item.IsActive);
        if (product is null)
        {
            return NotFound();
        }

        var cart = HttpContext.Session.GetObject<List<CartItemSessionModel>>(CartSessionKey) ?? new List<CartItemSessionModel>();
        var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

        if (existingItem is null)
        {
            cart.Add(new CartItemSessionModel
            {
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            existingItem.Quantity += quantity;
        }

        HttpContext.Session.SetObject(CartSessionKey, cart);
        TempData["Message"] = "Product added to cart.";

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, int quantity)
    {
        var cart = HttpContext.Session.GetObject<List<CartItemSessionModel>>(CartSessionKey) ?? new List<CartItemSessionModel>();
        var item = cart.FirstOrDefault(cartItem => cartItem.ProductId == productId);

        if (item is not null)
        {
            item.Quantity = Math.Max(1, quantity);
            HttpContext.Session.SetObject(CartSessionKey, cart);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        var cart = HttpContext.Session.GetObject<List<CartItemSessionModel>>(CartSessionKey) ?? new List<CartItemSessionModel>();
        var item = cart.FirstOrDefault(cartItem => cartItem.ProductId == productId);

        if (item is not null)
        {
            cart.Remove(item);
            HttpContext.Session.SetObject(CartSessionKey, cart);
        }

        return RedirectToAction("Index");
    }

    private async Task<List<CartItemViewModel>> BuildCartAsync()
    {
        var cart = HttpContext.Session.GetObject<List<CartItemSessionModel>>(CartSessionKey) ?? new List<CartItemSessionModel>();
        var productIds = cart.Select(item => item.ProductId).ToList();
        var products = await _context.Products.Where(product => productIds.Contains(product.Id)).ToListAsync();

        return cart.Select(item =>
        {
            var product = products.First(productItem => productItem.Id == item.ProductId);
            return new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                LineTotal = product.Price * item.Quantity
            };
        }).ToList();
    }
}