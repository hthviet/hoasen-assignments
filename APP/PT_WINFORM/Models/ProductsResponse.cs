namespace PT_WINFORM.Models;

public sealed class ProductsResponse
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; } = 1;
    public List<ProductDto> Items { get; set; } = new();
}
