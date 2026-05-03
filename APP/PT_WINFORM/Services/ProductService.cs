namespace PT_WINFORM.Services;

using System.Text;
using System.Text.Json;
using PT_WINFORM.Models;

public interface IProductService
{
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<ProductsResponse> GetProductsAsync(int page, string search = "", int? categoryId = null);
}

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        using var response = await _httpClient.GetAsync("/api/products/categories");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<CategoryDto>>(body, _jsonOptions) ?? new List<CategoryDto>();
    }

    public async Task<ProductsResponse> GetProductsAsync(int page, string search = "", int? categoryId = null)
    {
        var query = new StringBuilder($"/api/products?page={page}");
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Append("&search=").Append(Uri.EscapeDataString(search));
        }
        
        if (categoryId.HasValue)
        {
            query.Append("&categoryId=").Append(categoryId.Value);
        }

        using var response = await _httpClient.GetAsync(query.ToString());
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProductsResponse>(body, _jsonOptions) ?? new ProductsResponse();
    }
}
