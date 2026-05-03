namespace PT_WINFORM.Services;

using System.Text;
using System.Text.Json;
using PT_WINFORM.Models;

public interface IOrderService
{
    Task<List<OrderDto>> GetMyOrdersAsync();
    Task CheckoutAsync(string shippingAddress, string phoneNumber, List<(int ProductId, int Quantity)> items);
}

public class OrderService : IOrderService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync()
    {
        using var response = await _httpClient.GetAsync("/api/orders/my");
        
        if (!response.IsSuccessStatusCode)
        {
            return new List<OrderDto>();
        }

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<OrderDto>>(body, _jsonOptions) ?? new List<OrderDto>();
    }

    public async Task CheckoutAsync(string shippingAddress, string phoneNumber, List<(int ProductId, int Quantity)> items)
    {
        var payload = JsonSerializer.Serialize(new
        {
            shippingAddress,
            phoneNumber,
            items = items.Select(i => new { productId = i.ProductId, quantity = i.Quantity }).ToList()
        });

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("/api/orders/checkout", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Checkout failed: {errorBody}");
        }
    }
}
