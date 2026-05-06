namespace PT_WINFORM.Services;

using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using PT_WINFORM.Models;

public interface IOrderService
{
    Task<List<OrderDto>> GetMyOrdersAsync();
    Task CheckoutAsync(string shippingAddress, string phoneNumber, List<(int ProductId, int Quantity)> items);
}

public class OrderService : IOrderService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrderService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync()
    {
        var token = _authService.GetToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new List<OrderDto>();
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/orders/my");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            return new List<OrderDto>();
        }

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<OrderDto>>(body, _jsonOptions) ?? new List<OrderDto>();
    }

    public async Task CheckoutAsync(string shippingAddress, string phoneNumber, List<(int ProductId, int Quantity)> items)
    {
        var token = _authService.GetToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Please login before checkout.");
        }

        var payload = JsonSerializer.Serialize(new
        {
            shippingAddress,
            phoneNumber,
            items = items.Select(i => new { productId = i.ProductId, quantity = i.Quantity }).ToList()
        });

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders/checkout")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Checkout failed: {errorBody}");
        }
    }
}
