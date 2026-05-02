using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PT_WEB.Data;
using PT_WEB.Models.ViewModels;

namespace PT_WEB.Controllers;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 8;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, string? sortOrder, int page = 1)
    {
        var query = _context.Products
            .Include(product => product.Category)
            .Where(product => product.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(product => product.Name.Contains(searchTerm));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        query = sortOrder switch
        {
            "price_desc" => query.OrderByDescending(product => product.Price),
            "price_asc" => query.OrderBy(product => product.Price),
            _ => query.OrderBy(product => product.Name)
        };

        var totalItems = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var categories = await _context.Categories.OrderBy(category => category.Name).ToListAsync();

        var viewModel = new ProductListViewModel
        {
            Products = products,
            Categories = categories,
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            SortOrder = sortOrder,
            Page = page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize)
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

        if (product is null)
        {
            return NotFound();
        }

        return View(product);
    }
}