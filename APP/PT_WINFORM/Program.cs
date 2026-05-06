namespace PT_WINFORM;

using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using PT_WINFORM.Business;
using PT_WINFORM.Services;
using PT_WINFORM.UI;

internal static class Program
{
    private static readonly Uri ApiBaseUri = new("http://localhost:5226");

    [STAThread]
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();

        // Configure named HttpClients for API communication
        services.AddHttpClient("AuthApi", client =>
        {
            client.BaseAddress = ApiBaseUri;
        });

        // Share one auth service instance so login token is reused by checkout/order calls.
        services.AddSingleton<IAuthService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new AuthService(factory.CreateClient("AuthApi"));
        });

        services.AddHttpClient<IProductService, ProductService>(client =>
        {
            client.BaseAddress = ApiBaseUri;
        });

        services.AddHttpClient<IOrderService, OrderService>(client =>
        {
            client.BaseAddress = ApiBaseUri;
        });

        // Register business services
        services.AddSingleton<ICartManager, CartManager>();

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Initialize WinForms with explicit PerMonitorV2 DPI awareness so Windows
        // does not bitmap-scale the UI on high-scale displays.
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetDefaultFont(new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point));

        // Create and run main form with dependency injection
        var authService = serviceProvider.GetRequiredService<IAuthService>();
        var productService = serviceProvider.GetRequiredService<IProductService>();
        var orderService = serviceProvider.GetRequiredService<IOrderService>();
        var cartManager = serviceProvider.GetRequiredService<ICartManager>();
        var httpClient = new HttpClient { BaseAddress = ApiBaseUri };

        var mainForm = new MainForm(authService, productService, orderService, cartManager, httpClient);
        
        Application.Run(mainForm);
    }
}
