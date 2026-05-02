using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PT_WEB.Data;
using PT_WEB.Extensions;
using PT_WEB.Models;
using PT_WEB.Models.ViewModels;

namespace PT_WEB.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string CartSessionKey = "Cart";

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cartItems = await BuildCartAsync();
        if (cartItems.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        var userId = User.GetUserId();
        var user = await _context.UserAccounts.FindAsync(userId);
        if (user is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var viewModel = new CheckoutViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            Items = cartItems,
            TotalAmount = cartItems.Sum(item => item.LineTotal)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel viewModel)
    {
        var cartItems = await BuildCartAsync();
        if (cartItems.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty.");
        }

        if (!ModelState.IsValid)
        {
            viewModel.Items = cartItems;
            viewModel.TotalAmount = cartItems.Sum(item => item.LineTotal);
            return View(viewModel);
        }

        var order = new Order
        {
            UserAccountId = User.GetUserId(),
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.New,
            ShippingAddress = viewModel.ShippingAddress,
            PhoneNumber = viewModel.PhoneNumber,
            TotalAmount = cartItems.Sum(item => item.LineTotal),
            OrderItems = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                LineTotal = item.LineTotal
            }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        HttpContext.Session.Remove(CartSessionKey);
        return RedirectToAction(nameof(Success), new { id = order.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Success(int id)
    {
        var order = await _context.Orders
            .Include(item => item.OrderItems)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserAccountId == User.GetUserId());

        if (order is null)
        {
            return NotFound();
        }

        return View(order);
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