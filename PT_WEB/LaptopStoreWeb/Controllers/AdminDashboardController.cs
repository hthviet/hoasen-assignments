using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PT_WEB.Data;
using PT_WEB.Models;
using PT_WEB.Models.ViewModels;

namespace PT_WEB.Controllers;

[Authorize(Roles = "Admin")]
public class AdminDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? selectedDate)
    {
        var date = (selectedDate ?? DateTime.Today).Date;
        var paidOrders = _context.Orders.Where(order => order.Status == OrderStatus.Paid);

        var dailyRevenue = await paidOrders
            .Where(order => order.OrderDate.Date == date)
            .SumAsync(order => (decimal?)order.TotalAmount) ?? 0;

        var monthlyRevenue = await paidOrders
            .Where(order => order.OrderDate.Year == date.Year && order.OrderDate.Month == date.Month)
            .SumAsync(order => (decimal?)order.TotalAmount) ?? 0;

        var yearlyRevenue = await paidOrders
            .Where(order => order.OrderDate.Year == date.Year)
            .SumAsync(order => (decimal?)order.TotalAmount) ?? 0;

        var labels = new List<string>();
        var values = new List<decimal>();

        for (var i = 6; i >= 0; i--)
        {
            var currentDate = date.AddDays(-i);
            var revenue = await paidOrders
                .Where(order => order.OrderDate.Date == currentDate)
                .SumAsync(order => (decimal?)order.TotalAmount) ?? 0;

            labels.Add(currentDate.ToString("dd/MM"));
            values.Add(revenue);
        }

        var viewModel = new AdminDashboardViewModel
        {
            SelectedDate = date,
            DailyRevenue = dailyRevenue,
            MonthlyRevenue = monthlyRevenue,
            YearlyRevenue = yearlyRevenue,
            ChartLabels = labels,
            ChartValues = values
        };

        return View(viewModel);
    }
}