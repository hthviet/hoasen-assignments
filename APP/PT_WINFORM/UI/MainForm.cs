namespace PT_WINFORM.UI;

using System.Net.Http.Headers;
using PT_WINFORM.Business;
using PT_WINFORM.Models;
using PT_WINFORM.Services;

public partial class MainForm : Form
{
    private readonly IAuthService _authService;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly ICartManager _cartManager;

    // UI Components
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

    // State
    private List<ProductDto> _products = new();
    private List<CategoryDto> _categories = new();

    public MainForm(
        IAuthService authService,
        IProductService productService,
        IOrderService orderService,
        ICartManager cartManager,
        HttpClient httpClient)
    {
        _authService = authService;
        _productService = productService;
        _orderService = orderService;
        _cartManager = cartManager;

        InitializeComponent();
        BuildStoreUi();
        RefreshCartGrid();
        RefreshDashboard();

        Shown += async (_, _) => await InitializeAsync();
    }

    private async Task InitializeAsync()
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
            MessageBox.Show(
                $"Unable to initialize from API.\n{ex.Message}",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void BuildStoreUi()
    {
        Text = "Laptop Store Desktop";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1200, 760);
        BackColor = Color.FromArgb(245, 247, 251);

        var sidebar = CreateSidebar();
        var contentPanel = CreateContentPanel();

        Controls.Add(contentPanel);
        Controls.Add(sidebar);
    }

    private Panel CreateSidebar()
    {
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

        return sidebar;
    }

    private Panel CreateContentPanel()
    {
        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 251)
        };

        var topBar = CreateTopBar();
        _mainTabs = CreateTabControl();

        contentPanel.Controls.Add(_mainTabs);
        contentPanel.Controls.Add(topBar);

        return contentPanel;
    }

    private Panel CreateTopBar()
    {
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

        return topBar;
    }

    private TabControl CreateTabControl()
    {
        var tabControl = new TabControl
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

        tabControl.TabPages.Add(dashboardTab);
        tabControl.TabPages.Add(productsTab);
        tabControl.TabPages.Add(cartTab);
        tabControl.TabPages.Add(ordersTab);

        return tabControl;
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
        var login = await _authService.LoginAsync("customer1@gmail.com", "Customer@123");
        
        if (string.IsNullOrWhiteSpace(login.Token))
        {
            throw new InvalidOperationException("Login failed. Check backend status and seed account.");
        }

        // Note: AuthService handles token in HttpClient headers
        _userStatusLabel.Text = $"User: {login.FullName}";
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _productService.GetCategoriesAsync();
        _categories = categories;

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
        var search = _searchBox.Text.Trim();
        var selectedCategory = (_categoryBox.SelectedItem as CategoryFilterItem)?.Id;

        var response = await _productService.GetProductsAsync(1, search, selectedCategory);
        _products = response.Items;

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
            MessageBox.Show(
                "Please select a product first.",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var product = _products.FirstOrDefault(p => p.Id == productId);
        if (product is null)
        {
            return;
        }

        var qty = (int)_quantityInput.Value;
        _cartManager.AddProduct(product, qty);

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
            _cartManager.RemoveProduct(productId);
            RefreshCartGrid();
            RefreshDashboard();
        }
    }

    private void RefreshCartGrid()
    {
        var rows = _cartManager.GetItems()
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
        _cartTotalLabel.Text = $"Total: {FormatVnd(_cartManager.GetTotal())}";
    }

    private async Task CheckoutAsync()
    {
        var cartItems = _cartManager.GetItems();
        
        if (cartItems.Count == 0)
        {
            MessageBox.Show(
                "Your cart is empty.",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        try
        {
            var items = cartItems
                .Select(c => (c.Product.Id, c.Quantity))
                .ToList();

            await _orderService.CheckoutAsync("123 Desktop Client Street", "0900000000", items);

            _cartManager.Clear();
            RefreshCartGrid();
            RefreshDashboard();
            await LoadOrdersAsync();
            _mainTabs.SelectedIndex = 3;

            MessageBox.Show(
                "Checkout successful.",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Checkout failed.\n{ex.Message}",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async Task LoadOrdersAsync()
    {
        var orders = await _orderService.GetMyOrdersAsync();

        _ordersList.Items.Clear();
        foreach (var order in orders)
        {
            var itemCount = order.Items.Sum(i => i.Quantity);
            _ordersList.Items.Add(
                $"#{order.Id} | {order.OrderDate:dd/MM/yyyy HH:mm} | {order.Status,-8} | Items: {itemCount,2} | Total: {FormatVnd(order.TotalAmount)}");
        }
    }

    private void RefreshDashboard()
    {
        _dashboardProductsValue.Text = _products.Count.ToString();
        _dashboardCartValue.Text = _cartManager.GetItemCount().ToString();
        _dashboardTotalValue.Text = FormatVnd(_cartManager.GetTotal());
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
}
