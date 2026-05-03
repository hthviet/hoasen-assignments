namespace PT_WINFORM;

using Microsoft.Extensions.DependencyInjection;
using PT_WINFORM.Business;
using PT_WINFORM.Services;
using PT_WINFORM.UI;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();

        // Configure HttpClient for API communication
        services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5226");
        }).ConfigureHttpClient(client =>
        {
            // Token will be set after login in AuthService
        });

        services.AddHttpClient<IProductService, ProductService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5226");
        });

        services.AddHttpClient<IOrderService, OrderService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5226");
        });

        // Register business services
        services.AddSingleton<ICartManager, CartManager>();

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Initialize WinForms
        ApplicationConfiguration.Initialize();

        // Create and run main form with dependency injection
        var authService = serviceProvider.GetRequiredService<IAuthService>();
        var productService = serviceProvider.GetRequiredService<IProductService>();
        var orderService = serviceProvider.GetRequiredService<IOrderService>();
        var cartManager = serviceProvider.GetRequiredService<ICartManager>();
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5226") };

        var mainForm = new MainForm(authService, productService, orderService, cartManager, httpClient);
        
        Application.Run(mainForm);
    }
}
