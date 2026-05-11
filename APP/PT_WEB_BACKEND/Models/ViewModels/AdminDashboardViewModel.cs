namespace PT_WEB.Models.ViewModels;

public class AdminDashboardViewModel
{
    public DateTime SelectedDate { get; set; }
    public decimal DailyRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public List<string> ChartLabels { get; set; } = new();
    public List<decimal> ChartValues { get; set; } = new();
}