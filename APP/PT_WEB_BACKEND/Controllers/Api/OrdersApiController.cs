using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PT_WEB.Data;
using PT_WEB.Models;

namespace PT_WEB.Controllers.Api;

[ApiController]
[Route("api/orders")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrdersApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersApiController(ApplicationDbContext context) => _context = context;

    public record CartItemRequest(int ProductId, int Quantity);
    public record CheckoutRequest(string ShippingAddress, string PhoneNumber, List<CartItemRequest> Items);

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var productIds = req.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        if (products.Count != req.Items.Count)
            return BadRequest(new { message = "Một số sản phẩm không hợp lệ." });

        var orderItems = req.Items.Select(item =>
        {
            var product = products.First(p => p.Id == item.ProductId);
            return new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                LineTotal = product.Price * item.Quantity
            };
        }).ToList();

        var order = new Order
        {
            UserAccountId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.New,
            ShippingAddress = req.ShippingAddress,
            PhoneNumber = req.PhoneNumber,
            TotalAmount = orderItems.Sum(i => i.LineTotal),
            OrderItems = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(new { orderId = order.Id, totalAmount = order.TotalAmount, status = order.Status });
    }

    [HttpGet("my")]
    public async Task<IActionResult> MyOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserAccountId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.TotalAmount,
                o.Status,
                o.ShippingAddress,
                o.PhoneNumber,
                Items = o.OrderItems.Select(i => new
                {
                    i.ProductName, i.UnitPrice, i.Quantity, i.LineTotal
                })
            })
            .ToListAsync();

        return Ok(orders);
    }
}
