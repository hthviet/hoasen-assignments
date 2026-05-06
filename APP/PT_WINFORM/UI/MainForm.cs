namespace PT_WINFORM.UI;

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
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
    private Panel _searchBoxHost = null!;
    private Panel _categoryBoxHost = null!;
    private Panel _productsHostPanel = null!;
    private TableLayoutPanel _productsCardsTable = null!;
    private DataGridView _cartGrid = null!;
    private DataGridView _ordersGrid = null!;
    private Button _backHomeButton = null!;
    private Button _refreshButton = null!;
    private Button _cartTopButton = null!;
    private Button _checkoutButton = null!;
    private Label _topTitleLabel = null!;
    private Button _loginButton = null!;
    private Button _registerButton = null!;
    private Button _logoutButton = null!;
    private Label _dashboardProductsValue = null!;
    private Label _dashboardCartValue = null!;
    private Label _dashboardTotalValue = null!;
    private Label _cartTotalLabel = null!;
    private Label _userStatusLabel = null!;
    private Panel _dashboardMenuCard = null!;
    private FlowLayoutPanel _homeCardsPanel = null!;
    private System.Windows.Forms.Timer _searchDebounceTimer = null!;
    private List<Panel> _productCards = new();

    // State
    private List<ProductDto> _products = new();
    private List<CategoryDto> _categories = new();
    private bool _isAuthenticated;
    private bool _isAdmin;
    private int _loadingDepth;
    private Form? _loadingPopup;
    private Label? _loadingPopupLabel;

    private const int HomeTabIndex = 0;
    private const int DashboardTabIndex = 1;
    private const int ProductsTabIndex = 2;
    private const int CartTabIndex = 3;
    private const int OrdersTabIndex = 4;
    private const int ProductCardWidth = 320;
    private const int ProductColumnsPerRow = 4;
    private const int SearchDebounceMs = 350;
    private const int CategoryDebounceMs = 150;
    private static readonly HttpClient ImageHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(20)
    };
    private static readonly ConcurrentDictionary<string, Lazy<Task<Image?>>> ImageCache =
        new(StringComparer.OrdinalIgnoreCase);

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
            await RunWithLoadingAsync(async () =>
            {
                await LoadCategoriesAsync();
                await LoadProductsAsync();
                ApplyAuthState(null);
            }, "Initializing...");
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
        StartPosition = FormStartPosition.Manual;
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScroll = true;
        BackColor = Color.FromArgb(245, 247, 251);
        DoubleBuffered = true;

        var workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1600, 900);
        var width = Math.Max(1100, workingArea.Width / 2);
        var height = Math.Max(700, workingArea.Height / 2);

        MinimumSize = new Size(1100, 700);
        Size = new Size(width, height);
        Location = new Point(
            workingArea.Left + ((workingArea.Width - width) / 2),
            workingArea.Top + ((workingArea.Height - height) / 2));
        WindowState = FormWindowState.Maximized;

        var contentPanel = CreateContentPanel();

        Controls.Add(contentPanel);
        NormalizeControlLayout(this);
    }

    private void NormalizeControlLayout(Control root)
    {
        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case Label label when label.Dock != DockStyle.Fill:
                    label.AutoSize = true;
                    break;
                case Button button when button.Dock != DockStyle.Fill:
                    button.AutoSize = true;
                    button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                    if (button.MinimumSize == Size.Empty)
                    {
                        button.MinimumSize = new Size(90, 36);
                    }
                    break;
                case TextBox textBox:
                    textBox.MinimumSize = new Size(180, Math.Max(34, textBox.Height));
                    break;
                case ComboBox comboBox:
                    comboBox.MinimumSize = new Size(180, Math.Max(34, comboBox.Height));
                    break;
                case NumericUpDown numeric:
                    numeric.MinimumSize = new Size(80, Math.Max(34, numeric.Height));
                    break;
            }

            NormalizeControlLayout(control);
        }
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
            Height = 88,
            BackColor = Color.White
        };

        var leftHeaderPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.White,
            Padding = new Padding(24, 22, 0, 0),
            Margin = new Padding(0)
        };

        _backHomeButton = new Button
        {
            Text = "Back",
            Width = 110,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(15, 118, 110),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Visible = false,
            Margin = new Padding(0, 0, 16, 0)
        };
        _backHomeButton.FlatAppearance.BorderSize = 0;
        _backHomeButton.Click += (_, _) => NavigateToTab(HomeTabIndex);

        _topTitleLabel = new Label
        {
            Text = "Home",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 0),
            ForeColor = Color.FromArgb(25, 30, 48)
        };

        _searchBoxHost = new Panel
        {
            Width = 340,
            Height = 42,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(12, 0, 0, 0)
        };

        _searchBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Width = 308,
            Height = 24,
            AutoSize = false,
            Location = new Point(12, 9),
            PlaceholderText = "Search by product name"
        };
        _searchDebounceTimer = new System.Windows.Forms.Timer
        {
            Interval = SearchDebounceMs
        };
        _searchDebounceTimer.Tick += async (_, _) =>
        {
            _searchDebounceTimer.Stop();
            await LoadProductsAsync();
        };
        _searchBox.TextChanged += (_, _) =>
        {
            ScheduleProductsReload(SearchDebounceMs);
        };
        _searchBoxHost.Controls.Add(_searchBox);

        _categoryBoxHost = new Panel
        {
            Width = 220,
            Height = 42,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(12, 0, 0, 0)
        };

        _categoryBox = new ComboBox
        {
            Width = 216,
            Height = 38,
            Location = new Point(1, 1),
            IntegralHeight = false,
            FlatStyle = FlatStyle.Flat,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _categoryBox.SelectedIndexChanged += (_, _) => ScheduleProductsReload(CategoryDebounceMs);
        _categoryBoxHost.Controls.Add(_categoryBox);

        _refreshButton = new Button
        {
            Text = "Refresh",
            Width = 126,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        _refreshButton.FlatAppearance.BorderSize = 0;
        _refreshButton.Click += async (_, _) =>
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            await LoadOrdersAsync();
        };

        _cartTopButton = new Button
        {
            Text = "🛒 Cart (0)",
            Width = 136,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(29, 78, 216),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        _cartTopButton.FlatAppearance.BorderSize = 0;
        _cartTopButton.Click += (_, _) => NavigateToTab(CartTabIndex);

        _userStatusLabel = new Label
        {
            AutoSize = true,
            Text = "User: not logged in",
            ForeColor = Color.FromArgb(70, 80, 95),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(10, 8, 0, 0)
        };

        var rightControls = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.White,
            AutoScroll = true,
            Padding = new Padding(16, 22, 22, 10)
        };

        _loginButton = new Button
        {
            Text = "Login",
            Width = 110,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(15, 118, 110),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        _loginButton.FlatAppearance.BorderSize = 0;
        _loginButton.Click += async (_, _) => await LoginFromDialogAsync();

        _registerButton = new Button
        {
            Text = "Register",
            Width = 132,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        _registerButton.FlatAppearance.BorderSize = 0;
        _registerButton.Click += async (_, _) => await RegisterFromDialogAsync();

        _logoutButton = new Button
        {
            Text = "Logout",
            Width = 86,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(220, 38, 38),
            ForeColor = Color.White,
            Visible = false
        };
        _logoutButton.FlatAppearance.BorderSize = 0;
        _logoutButton.Click += (_, _) => LogoutCurrentUser();

        _refreshButton.Margin = new Padding(12, 0, 0, 0);
        _cartTopButton.Margin = new Padding(8, 0, 0, 0);
        _loginButton.Margin = new Padding(8, 0, 0, 0);
        _registerButton.Margin = new Padding(8, 0, 0, 0);
        _logoutButton.Margin = new Padding(12, 0, 0, 0);

        rightControls.Controls.Add(_userStatusLabel);
        rightControls.Controls.Add(_logoutButton);
        rightControls.Controls.Add(_registerButton);
        rightControls.Controls.Add(_loginButton);
        rightControls.Controls.Add(_cartTopButton);
        rightControls.Controls.Add(_refreshButton);
        rightControls.Controls.Add(_categoryBoxHost);
        rightControls.Controls.Add(_searchBoxHost);

        leftHeaderPanel.Controls.Add(_backHomeButton);
        leftHeaderPanel.Controls.Add(_topTitleLabel);

        topBar.Controls.Add(leftHeaderPanel);
        topBar.Controls.Add(rightControls);

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

        var homeTab = new TabPage("Home") { BackColor = Color.FromArgb(245, 247, 251) };
        var dashboardTab = new TabPage("Dashboard") { BackColor = Color.FromArgb(245, 247, 251) };
        var productsTab = new TabPage("Products") { BackColor = Color.FromArgb(245, 247, 251) };
        var cartTab = new TabPage("Cart") { BackColor = Color.FromArgb(245, 247, 251) };
        var ordersTab = new TabPage("Orders") { BackColor = Color.FromArgb(245, 247, 251) };

        BuildHomeTab(homeTab);
        BuildDashboardTab(dashboardTab);
        BuildProductsTab(productsTab);
        BuildCartTab(cartTab);
        BuildOrdersTab(ordersTab);

        tabControl.TabPages.Add(homeTab);
        tabControl.TabPages.Add(dashboardTab);
        tabControl.TabPages.Add(productsTab);
        tabControl.TabPages.Add(cartTab);
        tabControl.TabPages.Add(ordersTab);
        tabControl.SelectedIndexChanged += (_, _) => UpdateTopBarState();

        return tabControl;
    }

    private void BuildHomeTab(TabPage tab)
    {
        var wrapper = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 20, 28, 28),
            BackColor = Color.FromArgb(245, 247, 251)
        };

        var cardsHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };

        _homeCardsPanel = new FlowLayoutPanel
        {
            AutoSize = false,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        _homeCardsPanel.Controls.Add(CreateMenuCard("Products", Color.FromArgb(0, 120, 215), () => NavigateToTab(ProductsTabIndex)));
        _homeCardsPanel.Controls.Add(CreateMenuCard("Cart", Color.FromArgb(16, 124, 16), () => NavigateToTab(CartTabIndex)));
        _homeCardsPanel.Controls.Add(CreateMenuCard("Orders", Color.FromArgb(194, 57, 33), () => NavigateToTab(OrdersTabIndex)));
        _dashboardMenuCard = CreateMenuCard("Dashboard", Color.FromArgb(104, 33, 122), () => NavigateToTab(DashboardTabIndex));
        _dashboardMenuCard.Visible = false;
        _homeCardsPanel.Controls.Add(_dashboardMenuCard);

        cardsHost.Controls.Add(_homeCardsPanel);
        cardsHost.Resize += (_, _) => UpdateHomeCardsAlignment();

        wrapper.Controls.Add(cardsHost);
        tab.Controls.Add(wrapper);
        UpdateHomeCardsAlignment();
    }

    private Panel CreateMenuCard(string title, Color accent, Action onOpen)
    {
        var card = new Panel
        {
            Width = 280,
            Height = 170,
            Margin = new Padding(10),
            BackColor = accent,
            BorderStyle = BorderStyle.None,
            Cursor = Cursors.Hand
        };

        var titleLabel = new Label
        {
            Text = title,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };

        card.Click += (_, _) => onOpen();
        titleLabel.Click += (_, _) => onOpen();

        card.Controls.Add(titleLabel);
        return card;
    }

    private void UpdateHomeCardsAlignment()
    {
        if (_homeCardsPanel is null || _homeCardsPanel.Parent is null)
        {
            return;
        }

        var visibleCards = _homeCardsPanel.Controls
            .Cast<Control>()
            .Where(c => c.Visible)
            .ToList();

        if (visibleCards.Count == 0)
        {
            return;
        }

        var totalWidth = visibleCards.Sum(c => c.Width + c.Margin.Left + c.Margin.Right);
        var totalHeight = visibleCards.Max(c => c.Height + c.Margin.Top + c.Margin.Bottom);
        var hostSize = _homeCardsPanel.Parent.ClientSize;

        _homeCardsPanel.Size = new Size(totalWidth, totalHeight);
        _homeCardsPanel.Location = new Point(
            Math.Max(0, (hostSize.Width - totalWidth) / 2),
            Math.Max(20, (hostSize.Height - totalHeight) / 2));
    }

    private void NavigateToTab(int tabIndex)
    {
        if (tabIndex == DashboardTabIndex && !_isAdmin)
        {
            MessageBox.Show(
                "Dashboard is admin-only.",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        _mainTabs.SelectedIndex = tabIndex;
        UpdateTopBarState();
    }

    private void UpdateTopBarState()
    {
        var activeTab = _mainTabs.SelectedIndex;

        _backHomeButton.Visible = activeTab != HomeTabIndex;
        _searchBoxHost.Visible = activeTab == ProductsTabIndex;
        _categoryBoxHost.Visible = activeTab == ProductsTabIndex;
        _refreshButton.Visible = true;

        _topTitleLabel.Text = activeTab switch
        {
            HomeTabIndex => "Home",
            DashboardTabIndex => "Dashboard",
            ProductsTabIndex => "Products",
            CartTabIndex => "Cart",
            OrdersTabIndex => "Orders",
            _ => "Laptop Store"
        };
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
        _productsHostPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.FromArgb(245, 247, 251)
        };

        _productsCardsTable = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Padding = new Padding(10),
            Margin = Padding.Empty,
            Location = new Point(10, 12)
        };

        _productsHostPanel.Controls.Add(_productsCardsTable);
        _productsHostPanel.Resize += (_, _) => RelayoutProductsTable();

        tab.Controls.Add(_productsHostPanel);
    }

    private Panel CreateProductCard(ProductDto product)
    {
        var card = new Panel
        {
            Width = 320,
            Height = 500,
            Margin = new Padding(10),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Tag = product.Id
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 1,
            RowCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 190));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 146));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        var imageBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(248, 250, 252)
        };
        _ = BindProductImageAsync(imageBox, product.ImageUrl);

        var nameLabel = new Label
        {
            Text = product.Name,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            Dock = DockStyle.Fill,
            Padding = new Padding(2, 4, 2, 0),
            AutoEllipsis = true,
            TextAlign = ContentAlignment.TopCenter
        };

        var attributesTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Margin = new Padding(2, 2, 2, 8)
        };
        attributesTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        attributesTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        attributesTable.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
        attributesTable.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
        attributesTable.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));

        var categoryTitleLabel = new Label
        {
            Text = "Category",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 65, 81),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var categoryLabel = new Label
        {
            Text = product.CategoryName,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = Color.FromArgb(107, 114, 128),
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var priceTitleLabel = new Label
        {
            Text = "Price",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 65, 81),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var priceLabel = new Label
        {
            Text = FormatVnd(product.Price),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(37, 99, 235),
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var quantityTitleLabel = new Label
        {
            Text = "Quantity",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(55, 65, 81),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var quantityInput = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 99,
            Value = 1,
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            Width = 96,
            Height = 34,
            Anchor = AnchorStyles.Left,
            MinimumSize = new Size(96, 34)
        };

        var addButton = new Button
        {
            Text = "Add To Cart",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        addButton.FlatAppearance.BorderSize = 0;
        addButton.Click += (_, _) =>
        {
            _cartManager.AddProduct(product, (int)quantityInput.Value);
            RefreshCartGrid();
            RefreshDashboard();
        };

        attributesTable.Controls.Add(categoryTitleLabel, 0, 0);
        attributesTable.Controls.Add(categoryLabel, 1, 0);
        attributesTable.Controls.Add(priceTitleLabel, 0, 1);
        attributesTable.Controls.Add(priceLabel, 1, 1);
        attributesTable.Controls.Add(quantityTitleLabel, 0, 2);
        attributesTable.Controls.Add(quantityInput, 1, 2);

        layout.Controls.Add(imageBox, 0, 0);
        layout.Controls.Add(nameLabel, 0, 1);
        layout.Controls.Add(attributesTable, 0, 2);
        layout.Controls.Add(addButton, 0, 3);

        card.Controls.Add(layout);
        return card;
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
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None
        };

        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", Visible = false });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Product", DataPropertyName = "Name", FillWeight = 42 });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Qty", DataPropertyName = "Quantity", FillWeight = 10 });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Unit Price", DataPropertyName = "UnitPriceText", FillWeight = 18 });
        _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Line Total", DataPropertyName = "LineTotalText", FillWeight = 18 });
        _cartGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "", Text = "Remove", UseColumnTextForButtonValue = true, FillWeight = 12 });
        _cartGrid.CellContentClick += CartGrid_CellContentClick;

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 128,
            BackColor = Color.White
        };

        var footerActions = new Panel
        {
            Dock = DockStyle.Right,
            Width = 184,
            BackColor = Color.White,
            Padding = new Padding(12, 28, 20, 28)
        };

        _cartTotalLabel = new Label
        {
            AutoSize = true,
            Text = "Total: 0 VND",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 30, 48),
            Location = new Point(20, 30)
        };

        _checkoutButton = new Button
        {
            Text = "Checkout",
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = Padding.Empty,
            Enabled = false
        };
        _checkoutButton.FlatAppearance.BorderSize = 0;
        _checkoutButton.Click += async (_, _) => await CheckoutAsync();

        footerActions.Controls.Add(_checkoutButton);
        footer.Controls.Add(_cartTotalLabel);
        footer.Controls.Add(footerActions);

        tab.Controls.Add(_cartGrid);
        tab.Controls.Add(footer);
    }

    private void BuildOrdersTab(TabPage tab)
    {
        _ordersGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None
        };

        _ordersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Order ID", DataPropertyName = "OrderId", FillWeight = 12 });
        _ordersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Date", DataPropertyName = "OrderDateText", FillWeight = 28 });
        _ordersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status", FillWeight = 16 });
        _ordersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Items", DataPropertyName = "ItemCount", FillWeight = 12 });
        _ordersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "TotalText", FillWeight = 22 });

        tab.Padding = new Padding(16);
        tab.Controls.Add(_ordersGrid);
    }

    private async Task LoginFromDialogAsync()
    {
        var dialog = ShowAuthDialog(isRegister: false);
        if (dialog is null)
        {
            return;
        }

        try
        {
            await RunWithLoadingAsync(async () =>
            {
                var login = await _authService.LoginAsync(dialog.Email, dialog.Password);
                ApplyAuthState(login);
                await LoadOrdersAsync();
            }, "Signing in...");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Login failed.\n{ex.Message}",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private async Task RegisterFromDialogAsync()
    {
        var dialog = ShowAuthDialog(isRegister: true);
        if (dialog is null)
        {
            return;
        }

        try
        {
            await RunWithLoadingAsync(async () =>
            {
                var login = await _authService.RegisterAsync(dialog.FullName, dialog.Email, dialog.Password);
                ApplyAuthState(login);
                await LoadOrdersAsync();
            }, "Creating account...");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Register failed.\n{ex.Message}",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void LogoutCurrentUser()
    {
        _authService.Logout();
        _ordersGrid.DataSource = null;
        ApplyAuthState(null);
    }

    private void ApplyAuthState(LoginResponse? login)
    {
        _isAuthenticated = login is not null;
        _isAdmin = false;

        if (_isAuthenticated)
        {
            var token = _authService.GetToken();
            _isAdmin = IsAdminToken(token);
            _userStatusLabel.Text = $"User: {login!.FullName} ({(_isAdmin ? "Admin" : "Customer")})";
        }
        else
        {
            _userStatusLabel.Text = "User: guest";
        }

        _dashboardMenuCard.Visible = _isAdmin;
        UpdateHomeCardsAlignment();
        _loginButton.Visible = !_isAuthenticated;
        _registerButton.Visible = !_isAuthenticated;
        _logoutButton.Visible = _isAuthenticated;

        if (!_isAdmin && _mainTabs.SelectedIndex == DashboardTabIndex)
        {
            NavigateToTab(HomeTabIndex);
        }

        UpdateTopBarState();
    }

    private static bool IsAdminToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return false;
            }

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    break;
                case 3:
                    payload += "=";
                    break;
            }

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("role", out var roleClaim))
            {
                return string.Equals(roleClaim.GetString(), "Admin", StringComparison.OrdinalIgnoreCase);
            }

            if (doc.RootElement.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var schemaRoleClaim))
            {
                return string.Equals(schemaRoleClaim.GetString(), "Admin", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private AuthDialogResult? ShowAuthDialog(bool isRegister)
    {
        using var dialog = new Form
        {
            AutoScaleMode = AutoScaleMode.Dpi,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(560, isRegister ? 320 : 270),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            Text = isRegister ? "Register" : "Login"
        };

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = isRegister ? 4 : 3,
            Padding = new Padding(18),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var fullNameLabel = new Label
        {
            Text = "Full Name",
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 12, 14, 8),
            AutoSize = true,
            Visible = isRegister
        };
        var fullNameText = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8),
            MinimumSize = new Size(320, 0),
            Visible = isRegister
        };

        var emailLabel = new Label
        {
            Text = "Email",
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 12, 14, 8),
            AutoSize = true
        };
        var emailText = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8),
            MinimumSize = new Size(320, 0)
        };

        var passwordLabel = new Label
        {
            Text = "Password",
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 12, 14, 8),
            AutoSize = true
        };
        var passwordText = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8),
            MinimumSize = new Size(320, 0),
            UseSystemPasswordChar = true
        };

        var okButton = new Button
        {
            Text = isRegister ? "Register" : "Login",
            AutoSize = true,
            DialogResult = DialogResult.OK
        };
        var cancelButton = new Button
        {
            Text = "Cancel",
            AutoSize = true,
            DialogResult = DialogResult.Cancel
        };

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0)
        };
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Controls.Add(okButton);

        if (isRegister)
        {
            content.Controls.Add(fullNameLabel, 0, 0);
            content.Controls.Add(fullNameText, 1, 0);
            content.Controls.Add(emailLabel, 0, 1);
            content.Controls.Add(emailText, 1, 1);
            content.Controls.Add(passwordLabel, 0, 2);
            content.Controls.Add(passwordText, 1, 2);
            content.Controls.Add(buttonsPanel, 1, 3);
        }
        else
        {
            content.Controls.Add(emailLabel, 0, 0);
            content.Controls.Add(emailText, 1, 0);
            content.Controls.Add(passwordLabel, 0, 1);
            content.Controls.Add(passwordText, 1, 1);
            content.Controls.Add(buttonsPanel, 1, 2);
        }

        dialog.Controls.Add(content);
        dialog.AcceptButton = okButton;
        dialog.CancelButton = cancelButton;

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return null;
        }

        var fullName = fullNameText.Text.Trim();
        var email = emailText.Text.Trim();
        var password = passwordText.Text;

        if (isRegister && string.IsNullOrWhiteSpace(fullName))
        {
            MessageBox.Show("Full name is required.", "Laptop Store", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Email and password are required.", "Laptop Store", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        return new AuthDialogResult(fullName, email, password);
    }

    private CheckoutDialogResult? ShowCheckoutDialog()
    {
        using var dialog = new Form
        {
            AutoScaleMode = AutoScaleMode.Dpi,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(620, 280),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            Text = "Checkout"
        };

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(18),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var addressLabel = new Label
        {
            Text = "Address",
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 12, 14, 8),
            AutoSize = true
        };
        var addressText = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8),
            MinimumSize = new Size(360, 0)
        };

        var phoneLabel = new Label
        {
            Text = "Mobile",
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 12, 14, 8),
            AutoSize = true
        };
        var phoneText = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8),
            MinimumSize = new Size(360, 0)
        };

        var okButton = new Button
        {
            Text = "Place Order",
            AutoSize = true,
            DialogResult = DialogResult.OK
        };
        var cancelButton = new Button
        {
            Text = "Cancel",
            AutoSize = true,
            DialogResult = DialogResult.Cancel
        };

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0)
        };
        buttonsPanel.Controls.Add(cancelButton);
        buttonsPanel.Controls.Add(okButton);

        content.Controls.Add(addressLabel, 0, 0);
        content.Controls.Add(addressText, 1, 0);
        content.Controls.Add(phoneLabel, 0, 1);
        content.Controls.Add(phoneText, 1, 1);
        content.Controls.Add(buttonsPanel, 1, 2);

        dialog.Controls.Add(content);
        dialog.AcceptButton = okButton;
        dialog.CancelButton = cancelButton;

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return null;
        }

        var shippingAddress = addressText.Text.Trim();
        var phoneNumber = phoneText.Text.Trim();

        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            MessageBox.Show(this, "Address is required.", "Laptop Store", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            MessageBox.Show(this, "Mobile is required.", "Laptop Store", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        return new CheckoutDialogResult(shippingAddress, phoneNumber);
    }

    private async Task LoadCategoriesAsync()
    {
        await RunWithLoadingAsync(async () =>
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
        }, "Loading categories...");
    }

    private async Task LoadProductsAsync()
    {
        await RunWithLoadingAsync(async () =>
        {
            var search = _searchBox.Text.Trim();
            var selectedCategory = (_categoryBox.SelectedItem as CategoryFilterItem)?.Id;

            var allProducts = new List<ProductDto>();
            var page = 1;
            ProductsResponse response;

            do
            {
                response = await _productService.GetProductsAsync(page, search, selectedCategory);
                allProducts.AddRange(response.Items);
                page++;
            }
            while (page <= Math.Max(1, response.TotalPages));

            _products = allProducts;

            _productCards = _products.Select(CreateProductCard).ToList();
            RelayoutProductsTable();

            RefreshDashboard();
        }, "Loading products...");
    }

    private void ScheduleProductsReload(int debounceMs)
    {
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Interval = debounceMs;
        _searchDebounceTimer.Start();
    }

    private void RelayoutProductsTable()
    {
        if (_productsHostPanel is null || _productsCardsTable is null)
        {
            return;
        }

        var columnCount = ProductColumnsPerRow;

        _productsCardsTable.SuspendLayout();
        _productsCardsTable.Controls.Clear();
        _productsCardsTable.ColumnStyles.Clear();
        _productsCardsTable.RowStyles.Clear();

        _productsCardsTable.ColumnCount = columnCount;
        for (var col = 0; col < columnCount; col++)
        {
            _productsCardsTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        }

        var rowCount = _productCards.Count == 0 ? 1 : (int)Math.Ceiling((double)_productCards.Count / columnCount);
        _productsCardsTable.RowCount = rowCount;
        for (var row = 0; row < rowCount; row++)
        {
            _productsCardsTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        for (var i = 0; i < _productCards.Count; i++)
        {
            var row = i / columnCount;
            var col = i % columnCount;
            _productsCardsTable.Controls.Add(_productCards[i], col, row);
        }

        _productsCardsTable.ResumeLayout();

        var centeredX = Math.Max(10, (_productsHostPanel.ClientSize.Width - _productsCardsTable.PreferredSize.Width) / 2);
        _productsCardsTable.Location = new Point(centeredX, 12);
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
        _checkoutButton.Enabled = rows.Count > 0;
    }

    private async Task CheckoutAsync()
    {
        if (!_checkoutButton.Enabled)
        {
            return;
        }

        var cartItems = _cartManager.GetItems();
        
        if (cartItems.Count == 0)
        {
            MessageBox.Show(
                this,
                "Your cart is empty.",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (!_isAuthenticated)
        {
            await LoginFromDialogAsync();
            if (!_isAuthenticated)
            {
                return;
            }
        }

        var checkoutInfo = ShowCheckoutDialog();
        if (checkoutInfo is null)
        {
            return;
        }

        try
        {
            await RunWithLoadingAsync(async () =>
            {
                var items = cartItems
                    .Select(c => (c.Product.Id, c.Quantity))
                    .ToList();

                await _orderService.CheckoutAsync(checkoutInfo.ShippingAddress, checkoutInfo.PhoneNumber, items);

                _cartManager.Clear();
                RefreshCartGrid();
                RefreshDashboard();
                await LoadOrdersAsync();
            }, "Placing order...");

            NavigateToTab(OrdersTabIndex);

            MessageBox.Show(
                this,
                "Checkout successful.",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"Checkout failed.\n{ex.Message}",
                "Laptop Store",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async Task LoadOrdersAsync()
    {
        await RunWithLoadingAsync(async () =>
        {
            var orders = await _orderService.GetMyOrdersAsync();

            var rows = orders
                .Select(order => new
                {
                    OrderId = order.Id,
                    OrderDateText = order.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                    Status = order.Status,
                    ItemCount = order.Items.Sum(i => i.Quantity),
                    TotalText = FormatVnd(order.TotalAmount)
                })
                .ToList();

            _ordersGrid.DataSource = rows;
        }, "Loading orders...");
    }

    private async Task RunWithLoadingAsync(Func<Task> action, string message)
    {
        ShowLoadingPopup(message);
        try
        {
            await action();
        }
        finally
        {
            HideLoadingPopup();
        }
    }

    private void ShowLoadingPopup(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowLoadingPopup(message)));
            return;
        }

        _loadingDepth++;
        if (_loadingPopup is not null)
        {
            if (_loadingPopupLabel is not null)
            {
                _loadingPopupLabel.Text = message;
            }

            return;
        }

        var popup = new Form
        {
            AutoScaleMode = AutoScaleMode.Dpi,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            ShowInTaskbar = false,
            MaximizeBox = false,
            MinimizeBox = false,
            ControlBox = false,
            TopMost = true,
            Width = 320,
            Height = 140,
            Text = "Please wait"
        };

        var label = new Label
        {
            Dock = DockStyle.Top,
            Height = 52,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Text = message
        };

        var progress = new ProgressBar
        {
            Dock = DockStyle.Top,
            Height = 26,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 30,
            Margin = new Padding(16, 0, 16, 0)
        };

        var host = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 16, 16, 16)
        };
        host.Controls.Add(progress);
        host.Controls.Add(label);
        popup.Controls.Add(host);

        _loadingPopup = popup;
        _loadingPopupLabel = label;

        UseWaitCursor = true;
        Enabled = false;
        popup.Show(this);
        popup.BringToFront();
    }

    private void HideLoadingPopup()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(HideLoadingPopup));
            return;
        }

        if (_loadingDepth == 0)
        {
            return;
        }

        _loadingDepth--;
        if (_loadingDepth > 0)
        {
            return;
        }

        var popup = _loadingPopup;
        _loadingPopup = null;
        _loadingPopupLabel = null;

        if (popup is not null)
        {
            popup.Close();
            popup.Dispose();
        }

        Enabled = true;
        UseWaitCursor = false;
    }

    private void RefreshDashboard()
    {
        _dashboardProductsValue.Text = _products.Count.ToString();
        var cartItemCount = _cartManager.GetItemCount();
        _dashboardCartValue.Text = cartItemCount.ToString();
        _cartTopButton.Text = $"🛒 Cart ({cartItemCount})";
        _dashboardTotalValue.Text = FormatVnd(_cartManager.GetTotal());
    }

    private async Task BindProductImageAsync(PictureBox imageBox, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            imageBox.BackColor = Color.FromArgb(232, 236, 243);
            return;
        }

        try
        {
            var cachedImage = await GetCachedImageAsync(imageUrl);
            if (imageBox.IsDisposed)
            {
                return;
            }

            if (cachedImage is null)
            {
                // Fallback path for hosts that reject raw HttpClient requests intermittently.
                StartPictureBoxFallbackLoad(imageBox, imageUrl);
                return;
            }

            var imageForControl = new Bitmap(cachedImage);

            if (imageBox.IsDisposed)
            {
                imageForControl.Dispose();
                return;
            }

            if (imageBox.InvokeRequired)
            {
                imageBox.BeginInvoke(new Action(() => SetImageOnPictureBox(imageBox, imageForControl)));
            }
            else
            {
                SetImageOnPictureBox(imageBox, imageForControl);
            }
        }
        catch
        {
            ImageCache.TryRemove(imageUrl, out _);

            if (!imageBox.IsDisposed)
            {
                StartPictureBoxFallbackLoad(imageBox, imageUrl);
            }
        }
    }

    private static void StartPictureBoxFallbackLoad(PictureBox imageBox, string imageUrl)
    {
        void StartLoad()
        {
            if (imageBox.IsDisposed)
            {
                return;
            }

            imageBox.LoadCompleted += OnFallbackLoadCompleted;

            try
            {
                imageBox.LoadAsync(imageUrl);
            }
            catch
            {
                imageBox.LoadCompleted -= OnFallbackLoadCompleted;
                imageBox.BackColor = Color.FromArgb(232, 236, 243);
            }
        }

        void OnFallbackLoadCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs args)
        {
            imageBox.LoadCompleted -= OnFallbackLoadCompleted;

            if (args.Error is not null || imageBox.Image is null)
            {
                imageBox.BackColor = Color.FromArgb(232, 236, 243);
            }
            else
            {
                imageBox.BackColor = Color.FromArgb(248, 250, 252);
            }
        }

        if (imageBox.InvokeRequired)
        {
            imageBox.BeginInvoke(new Action(StartLoad));
        }
        else
        {
            StartLoad();
        }
    }

    private static void SetImageOnPictureBox(PictureBox imageBox, Image newImage)
    {
        if (imageBox.IsDisposed)
        {
            newImage.Dispose();
            return;
        }

        var oldImage = imageBox.Image;
        imageBox.Image = newImage;
        imageBox.BackColor = Color.FromArgb(248, 250, 252);
        oldImage?.Dispose();
    }

    private static async Task<Image?> GetCachedImageAsync(string imageUrl)
    {
        var lazyTask = ImageCache.GetOrAdd(
            imageUrl,
            static url => new Lazy<Task<Image?>>(() => DownloadImageAsync(url), true));

        var image = await lazyTask.Value;
        if (image is null)
        {
            ImageCache.TryRemove(imageUrl, out _);
        }

        return image;
    }

    private static async Task<Image?> DownloadImageAsync(string imageUrl)
    {
        using var response = await ImageHttpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var memoryStream = new MemoryStream();
        await responseStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var originalImage = Image.FromStream(memoryStream);
        return new Bitmap(originalImage);
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

    private sealed class AuthDialogResult
    {
        public AuthDialogResult(string fullName, string email, string password)
        {
            FullName = fullName;
            Email = email;
            Password = password;
        }

        public string FullName { get; }
        public string Email { get; }
        public string Password { get; }
    }

    private sealed class CheckoutDialogResult
    {
        public CheckoutDialogResult(string shippingAddress, string phoneNumber)
        {
            ShippingAddress = shippingAddress;
            PhoneNumber = phoneNumber;
        }

        public string ShippingAddress { get; }
        public string PhoneNumber { get; }
    }
}
