using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PT_WEB.Data;

namespace PT_WEB.Controllers.Api;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 10;

    public ProductsApiController(ApplicationDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? categoryId, [FromQuery] string? sort, [FromQuery] int page = 1)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderBy(p => p.Name)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(p => new
            {
                p.Id, p.Name, p.Brand, p.Description,
                p.Price, p.ImageUrl,
                CategoryName = p.Category != null ? p.Category.Name : ""
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize = PageSize, totalPages = (int)Math.Ceiling(total / (double)PageSize), items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _context.Products
            .Include(product => product.Category)
            .Where(product => product.Id == id && product.IsActive)
            .Select(product => new
            {
                product.Id, product.Name, product.Brand, product.Description,
                product.Price, product.ImageUrl,
                CategoryName = product.Category != null ? product.Category.Name : ""
            })
            .FirstOrDefaultAsync();

        if (p is null) return NotFound();
        return Ok(p);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var cats = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();
        return Ok(cats);
    }
}
