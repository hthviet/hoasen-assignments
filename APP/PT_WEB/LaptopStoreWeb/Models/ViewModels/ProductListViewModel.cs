namespace PT_WEB.Models.ViewModels;

public class ProductListViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public string? SortOrder { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
}