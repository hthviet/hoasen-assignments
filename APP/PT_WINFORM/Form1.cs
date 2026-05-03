namespace PT_WINFORM;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public partial class Form1 : Form
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5226")
    };

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly List<ProductDto> _products = new();
    private readonly List<CategoryDto> _categories = new();
    private readonly List<CartLine> _cart = new();

    private TabControl _mainTabs = null!;
    private TextBox _searchBox = null!;
    private ComboBox _categoryBox = null!;
    private DataGridView _productsGrid = null!;
    private DataGridView _cartGrid = null!;
    private NumericUpDown _quantityInput = null!;
    private Label _dashboardProductsValue = null!;
    private Label _dashboardCartValue = null!;
    private Label _dashboardTotalValue = null!;
    private Label _cartTotalLabel = null!;
    private ListBox _ordersList = null!;
    private Label _userStatusLabel = null!;

    private string _token = string.Empty;

    public Form1()
    {
        InitializeComponent();
        BuildStoreUi();
        RefreshCartGrid();
        RefreshDashboard();

        Shown += async (_, _) => await InitializeFromApiAsync();
    }

    private async Task InitializeFromApiAsync()
    {
        try
        {
            await LoginAsDefaultCustomerAsync();
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to initialize from API.\n{ex.Message}", "Laptop Store", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void BuildStoreUi()
    {
        Text = "Laptop Store Desktop";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1200, 760);
        BackColor = Color.FromArgb(245, 247, 251);

        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 210,
            BackColor = Color.FromArgb(25, 30, 48)
        };

        var brandLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 90,
            Text = "LaptopStore\nDesktop",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        sidebar.Controls.Add(CreateNavButton("Orders", 3));
        sidebar.Controls.Add(CreateNavButton("Cart", 2));
        sidebar.Controls.Add(CreateNavButton("Products", 1));
        sidebar.Controls.Add(CreateNavButton("Dashboard", 0));
        sidebar.Controls.Add(brandLabel);

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 251)
        };

        var topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 78,
            BackColor = Color.White
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            Text = "Laptop Catalog",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(24, 22),
            ForeColor = Color.FromArgb(25, 30, 48)
        };

        _searchBox = new TextBox
        {
            Width = 260,
            Location = new Point(680, 25),
            PlaceholderText = "Search by product name"
        };
        _searchBox.TextChanged += async (_, _) => await LoadProductsAsync();

        _categoryBox = new ComboBox
        {
            Width = 180,
            Location = new Point(955, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _categoryBox.SelectedIndexChanged += async (_, _) => await LoadProductsAsync();

        var refreshButton = new Button
        {
            Text = "Refresh",
            Width = 80,
            Height = 32,
            Location = new Point(1148, 23),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White
        };
        refreshButton.FlatAppearance.BorderSize = 0;
        refreshButton.Click += async (_, _) =>
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            await LoadOrdersAsync();
        };

        _userStatusLabel = new Label
        {
            AutoSize = true,
            Text = "User: not logged in",
            ForeColor = Color.FromArgb(70, 80, 95),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(1240, 30)
        };

        topBar.Controls.Add(titleLabel);
        topBar.Controls.Add(_searchBox);
        topBar.Controls.Add(_categoryBox);
        topBar.Controls.Add(refreshButton);
        topBar.Controls.Add(_userStatusLabel);

        _mainTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Appearance = TabAppearance.Normal,
            ItemSize = new Size(0, 1),
            SizeMode = TabSizeMode.Fixed
        };

        var dashboardTab = new TabPage("Dashboard") { BackColor = Color.FromArgb(245, 247, 251) };
        var productsTab = new TabPage("Products") { BackColor = Color.FromArgb(245, 247, 251) };
        var cartTab = new TabPage("Cart") { BackColor = Color.FromArgb(245, 247, 251) };
        var ordersTab = new TabPage("Orders") { BackColor = Color.FromArgb(245, 247, 251) };

        BuildDashboardTab(dashboardTab);
        BuildProductsTab(productsTab);
        BuildCartTab(cartTab);
        BuildOrdersTab(ordersTab);

        _mainTabs.TabPages.Add(dashboardTab);
        _mainTabs.TabPages.Add(productsTab);
        _mainTabs.TabPages.Add(cartTab);
        _mainTabs.TabPages.Add(ordersTab);

        contentPanel.Controls.Add(_mainTabs);
        contentPanel.Controls.Add(topBar);

        Controls.Add(contentPanel);
        Controls.Add(sidebar);
    }

    private Button CreateNavButton(string text, int tabIndex)
    {
        var button = new Button
        {
            Dock = DockStyle.Top,
            Height = 52,
            FlatStyle = FlatStyle.Flat,
            Text = text,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(35, 42, 66),
            FlatAppearance = { BorderSize = 0 }
        };

        button.Click += (_, _) => _mainTabs.SelectedIndex = tabIndex;
        return button;
    }

    private void BuildDashboardTab(TabPage tab)
    {
        var cardsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 210,
            Padding = new Padding(24),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        cardsPanel.Controls.Add(CreateMetricCard("Total Products", out _dashboardProductsValue, Color.FromArgb(52, 152, 219)));
        cardsPanel.Controls.Add(CreateMetricCard("Cart Items", out _dashboardCartValue, Color.FromArgb(46, 204, 113)));
        cardsPanel.Controls.Add(CreateMetricCard("Cart Value", out _dashboardTotalValue, Color.FromArgb(241, 196, 15)));

        var welcomePanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24)
        };

        var welcomeText = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Welcome to Laptop Store Desktop.\nUse the Products tab to browse catalog and add items to cart, then checkout from the Cart tab.",
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            ForeColor = Color.FromArgb(55, 65, 81)
        };

        welcomePanel.Controls.Add(welcomeText);
        tab.Controls.Add(welcomePanel);
        tab.Controls.Add(cardsPanel);
    }

    private Panel CreateMetricCard(string title, out Label valueLabel, Color accent)
    {
        var card = new Panel
        {
            Width = 300,
            Height = 150,
            Margin = new Padding(0, 0, 18, 0),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            Text = title,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 65, 81),
            Location = new Point(18, 20)
        };

        valueLabel = new Label
        {
            AutoSize = true,
            Text = "0",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = accent,
            Location = new Point(18, 55)
        };

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);
        return card;
    }

    private void BuildProductsTab(TabPage tab)
    {
        _productsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None
        };

        _productsGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", Visible = false });
        _productsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", DataPropertyName = "Name", Width = 280 });
        _productsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Brand", DataPropertyName = "Brand", Width = 120 });
        _productsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", DataPropertyName = "CategoryName", Width = 150 });
        _productsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Price", DataPropertyName = "PriceText", Width = 140 });

        var actionPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            BackColor = Color.White
        };

        var quantityLabel = new Label
        {
            Text = "Quantity:",
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(18, 24)
        };

        _quantityInput = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 99,
            Value = 1,
            Location = new Point(88, 21),
            Width = 70
        };

        var addButton = new Button
        {
            Text = "Add Selected To Cart",
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(180, 18),
            Width = 190,
            Height = 34
        };
        addButton.FlatAppearance.BorderSize = 0;
        addButton.Click += (_, _) => AddSelectedProductToCart();

        actionPanel.Controls.Add(quantityLabel);
        actionPanel.Controls.Add(_quantityInput);
        actionPanel.Controls.Add(addButton);

        tab.Controls.Add(_productsGrid);
        tab.Controls.Add(actionPanel);
    }

    private void BuildCartTab(TabPage tab)
    {
        _cartGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None
        };

        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", Visible = false });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Product", DataPropertyName = "Name", Width = 300 });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Qty", DataPropertyName = "Quantity", Width = 80 });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit Price", DataPropertyName = "UnitPriceText", Width = 140 });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Line Total", DataPropertyName = "LineTotalText", Width = 140 });
        _cartGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "", Text = "Remove", UseColumnTextForButtonValue = true, Width = 90 });
        _cartGrid.CellContentClick += CartGrid_CellContentClick;

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 78,
            BackColor = Color.White
        };

        _cartTotalLabel = new Label
        {
            AutoSize = true,
            Text = "Total: 0 VND",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 30, 48),
            Location = new Point(20, 24)
        };

        var checkoutButton = new Button
        {
            Text = "Checkout",
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Width = 140,
            Height = 38,
            Location = new Point(980, 18)
        };
        checkoutButton.FlatAppearance.BorderSize = 0;
        checkoutButton.Click += async (_, _) => await CheckoutAsync();

        footer.Controls.Add(_cartTotalLabel);
        footer.Controls.Add(checkoutButton);

        tab.Controls.Add(_cartGrid);
        tab.Controls.Add(footer);
    }

    private void BuildOrdersTab(TabPage tab)
    {
        _ordersList = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 11, FontStyle.Regular),
            BorderStyle = BorderStyle.None,
            BackColor = Color.White
        };

        tab.Padding = new Padding(16);
        tab.Controls.Add(_ordersList);
    }

    private async Task LoginAsDefaultCustomerAsync()
    {
        var payload = JsonSerializer.Serialize(new
        {
            email = "customer1@gmail.com",
            password = "Customer@123"
        });

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("/api/auth/login", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Login failed. Check backend status and seed account.");
        }

        var body = await response.Content.ReadAsStringAsync();
        var login = JsonSerializer.Deserialize<LoginResponse>(body, _jsonOptions);
        if (login is null || string.IsNullOrWhiteSpace(login.Token))
        {
            throw new InvalidOperationException("Login token missing from API response.");
        }

        _token = login.Token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _userStatusLabel.Text = $"User: {login.FullName}";
    }

    private async Task LoadCategoriesAsync()
    {
        using var response = await _httpClient.GetAsync("/api/products/categories");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var categories = JsonSerializer.Deserialize<List<CategoryDto>>(body, _jsonOptions) ?? new List<CategoryDto>();

        _categories.Clear();
        _categories.AddRange(categories);

        var selected = (_categoryBox.SelectedItem as CategoryFilterItem)?.Id;
        _categoryBox.Items.Clear();
        _categoryBox.Items.Add(new CategoryFilterItem(null, "All Categories"));

        foreach (var category in _categories.OrderBy(c => c.Name))
        {
            _categoryBox.Items.Add(new CategoryFilterItem(category.Id, category.Name));
        }

        var selectedItem = _categoryBox.Items.Cast<CategoryFilterItem>().FirstOrDefault(c => c.Id == selected);
        _categoryBox.SelectedItem = selectedItem ?? _categoryBox.Items[0];
    }

    private async Task LoadProductsAsync()
    {
        var search = Uri.EscapeDataString(_searchBox.Text.Trim());
        var selectedCategory = (_categoryBox.SelectedItem as CategoryFilterItem)?.Id;

        var query = new StringBuilder("/api/products?page=1");
        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Append("&search=").Append(search);
        }
        if (selectedCategory.HasValue)
        {
            query.Append("&categoryId=").Append(selectedCategory.Value);
        }

        using var response = await _httpClient.GetAsync(query.ToString());
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var productsResponse = JsonSerializer.Deserialize<ProductsResponse>(body, _jsonOptions);

        _products.Clear();
        if (productsResponse?.Items is not null)
        {
            _products.AddRange(productsResponse.Items);
        }

        var filtered = _products
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Brand,
                p.CategoryName,
                PriceText = FormatVnd(p.Price)
            })
            .ToList();

        _productsGrid.DataSource = filtered;
        RefreshDashboard();
    }

    private void AddSelectedProductToCart()
    {
        if (_productsGrid.CurrentRow?.Cells["Id"].Value is not int productId)
        {
            MessageBox.Show("Please select a product first.", "Laptop Store", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product is null)
        {
            return;
        }

        var qty = (int)_quantityInput.Value;
        var existing = _cart.FirstOrDefault(c => c.Product.Id == productId);

        if (existing is null)
        {
            _cart.Add(new CartLine(product, qty));
        }
        else
        {
            existing.Quantity += qty;
        }

        RefreshCartGrid();
        RefreshDashboard();
        _mainTabs.SelectedIndex = 2;
    }

    private void CartGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 5)
        {
            return;
        }

        var idValue = _cartGrid.Rows[e.RowIndex].Cells["Id"].Value;
        if (idValue is int productId)
        {
            _cart.RemoveAll(line => line.Product.Id == productId);
            RefreshCartGrid();
            RefreshDashboard();
        }
    }

    private void RefreshCartGrid()
    {
        var rows = _cart
            .Select(line => new
            {
                line.Product.Id,
                Name = line.Product.Name,
                line.Quantity,
                UnitPriceText = FormatVnd(line.Product.Price),
                LineTotalText = FormatVnd(line.LineTotal)
            })
            .ToList();

        _cartGrid.DataSource = rows;
        _cartTotalLabel.Text = $"Total: {FormatVnd(_cart.Sum(c => c.LineTotal))}";
    }

    private async Task CheckoutAsync()
    {
        if (_cart.Count == 0)
        {
            MessageBox.Show("Your cart is empty.", "Laptop Store", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(_token))
        {
            MessageBox.Show("Not authenticated. Please restart app and ensure API login works.", "Laptop Store", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            shippingAddress = "123 Desktop Client Street",
            phoneNumber = "0900000000",
            items = _cart.Select(c => new { productId = c.Product.Id, quantity = c.Quantity }).ToList()
        });

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("/api/orders/checkout", content);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            MessageBox.Show($"Checkout failed.\n{err}", "Laptop Store", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _cart.Clear();
        RefreshCartGrid();
        RefreshDashboard();
        await LoadOrdersAsync();
        _mainTabs.SelectedIndex = 3;

        MessageBox.Show("Checkout successful.", "Laptop Store", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task LoadOrdersAsync()
    {
        if (string.IsNullOrWhiteSpace(_token))
        {
            return;
        }

        using var response = await _httpClient.GetAsync("/api/orders/my");
        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<OrderDto>>(body, _jsonOptions) ?? new List<OrderDto>();

        _ordersList.Items.Clear();
        foreach (var order in orders)
        {
            var itemCount = order.Items.Sum(i => i.Quantity);
            _ordersList.Items.Add($"#{order.Id} | {order.OrderDate:dd/MM/yyyy HH:mm} | {order.Status,-8} | Items: {itemCount,2} | Total: {FormatVnd(order.TotalAmount)}");
        }
    }

    private void RefreshDashboard()
    {
        _dashboardProductsValue.Text = _products.Count.ToString();
        _dashboardCartValue.Text = _cart.Sum(c => c.Quantity).ToString();
        _dashboardTotalValue.Text = FormatVnd(_cart.Sum(c => c.LineTotal));
    }

    private static string FormatVnd(decimal amount)
    {
        return $"{amount:N0} VND";
    }

    private sealed class CategoryFilterItem
    {
        public CategoryFilterItem(int? id, string name)
        {
            Id = id;
            Name = name;
        }

        public int? Id { get; }
        public string Name { get; }

        public override string ToString() => Name;
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private sealed class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class ProductsResponse
    {
        public List<ProductDto> Items { get; set; } = new();
    }

    private sealed class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }

    private sealed class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }

    private sealed class OrderItemDto
    {
        public int Quantity { get; set; }
    }

    private sealed class CartLine
    {
        public CartLine(ProductDto product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public ProductDto Product { get; }
        public int Quantity { get; set; }
        public decimal LineTotal => Quantity * Product.Price;
    }
}
