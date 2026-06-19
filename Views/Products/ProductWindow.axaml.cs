using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using DemoExam.Models;
using DemoExam.Views.Login;
using DemoExam.Views.Orders;

using Npgsql;

namespace DemoExam.Views.Products;

public partial class ProductWindow : Window
{
    private User currentUser;

    private readonly string connectionString =
        "Host=localhost;Port=5432;Username=postgres;Password=0000;Database=demoexam";

    private List<Product> allProducts = new();

    public ProductWindow()
    {
        InitializeComponent();
    }

    public ProductWindow(User user)
    {
        InitializeComponent();

        currentUser = user;

        UserTextBlock.Text =
            $"{user.FullName} ({user.Role})";

        SortComboBox.SelectedIndex = 0;
        DiscountComboBox.SelectedIndex = 0;

        SearchTextBox.TextChanged += (_, _) => ApplyFilters();

        SortComboBox.SelectionChanged += (_, _) => ApplyFilters();

        DiscountComboBox.SelectionChanged += (_, _) => ApplyFilters();

        ConfigureRole();

        LoadProducts();
    }
    private void LoadProducts()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        string query =
            "SELECT * FROM view_products";

        using var command =
            new NpgsqlCommand(query, connection);

        using var reader =
            command.ExecuteReader();

        allProducts.Clear();

        while (reader.Read())
        {
            Product product = new Product();

            product.Article = reader.GetString(0);
            product.Name = reader.GetString(1);
            product.Unit = reader.GetString(2);
            product.Price = reader.GetDecimal(3);

            product.Supplier = reader.GetString(4);
            product.Manufacturer = reader.GetString(5);
            product.Category = reader.GetString(6);

            product.Discount = reader.GetInt32(7);
            product.Quantity = reader.GetInt32(8);

            product.Description =
                reader.IsDBNull(9)
                    ? ""
                    : reader.GetString(9);

            product.Photo =
                reader.IsDBNull(10)
                    ? ""
                    : reader.GetString(10);

            allProducts.Add(product);
        }

        ApplyFilters();
    }
    private void ShowProducts(List<Product> products)
    {
        ProductsPanel.Children.Clear();

        CountTextBlock.Text =
            $"Найдено товаров: {products.Count}";

        foreach (var product in products)
        {
            Border card = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 10)
            };

            if (product.Quantity == 0)
            {
                card.Background = Brushes.LightGray;
            }
            else if (product.Discount > 15)
            {
                card.Background =
                    new SolidColorBrush(
                        Color.Parse("#D8F5D0"));
            }

            Grid grid = new Grid();

            grid.ColumnDefinitions =
                new ColumnDefinitions("220,*");

            // ------------------------
            // Фото
            // ------------------------

            Image image = new Image
            {
                Width = 220,
                Height = 220,
                Stretch = Stretch.UniformToFill
            };

            try
            {
                string imagePath =
                    string.IsNullOrWhiteSpace(product.Photo)
                    ? "avares://DemoExam/Assets/picture.png"
                    : $"avares://DemoExam/Assets/{product.Photo}";

                image.Source =
                    new Bitmap(
                        AssetLoader.Open(
                            new Uri(imagePath)));
            }
            catch
            {
                image.Source =
                    new Bitmap(
                        AssetLoader.Open(
                            new Uri(
                                "avares://DemoExam/Assets/picture.png")));
            }

            // ------------------------
            // Правая часть
            // ------------------------

            StackPanel info = new StackPanel
            {
                Margin = new Thickness(15, 0, 0, 0),
                Spacing = 4
            };

            Grid.SetColumn(info, 1);

            decimal finalPrice =
                product.Price -
                product.Price *
                product.Discount / 100m;

            TextBlock articleText = new()
            {
                Text = $"Артикул: {product.Article}",
                FontWeight = FontWeight.Bold
            };

            TextBlock nameText = new()
            {
                Text = product.Name,
                FontSize = 20,
                FontWeight = FontWeight.Bold,
                TextWrapping = TextWrapping.Wrap
            };

            TextBlock categoryText = new()
            {
                Text = $"Категория: {product.Category}"
            };

            TextBlock manufacturerText = new()
            {
                Text = $"Производитель: {product.Manufacturer}"
            };

            TextBlock supplierText = new()
            {
                Text = $"Поставщик: {product.Supplier}"
            };

            TextBlock quantityText = new()
            {
                Text = $"На складе: {product.Quantity}"
            };

            TextBlock discountText = new()
            {
                Text = $"Скидка: {product.Discount}%",
                FontWeight = FontWeight.Bold
            };

            TextBlock oldPriceText = new()
            {
                Text = $"Цена без скидки: {product.Price:F2} ₽"
            };

            TextBlock finalPriceText = new()
            {
                Text = $"Цена со скидкой: {finalPrice:F2} ₽",
                FontSize = 18,
                FontWeight = FontWeight.Bold
            };

            TextBlock descriptionText = new()
            {
                Text = product.Description,
                TextWrapping = TextWrapping.Wrap
            };

            // ------------------------
            // Кнопки
            // ------------------------

            StackPanel buttonsPanel = new()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Margin = new Thickness(0, 10, 0, 0)
            };

            Button editButton = new()
            {
                Content = "Изменить",
                Tag = product
            };

            editButton.Click += EditButton_Click;

            Button deleteButton = new()
            {
                Content = "Удалить",
                Tag = product
            };

            deleteButton.Click += DeleteButton_Click;

            buttonsPanel.Children.Add(editButton);
            buttonsPanel.Children.Add(deleteButton);

            // Если не админ — скрываем CRUD

            if (currentUser.Role != "Администратор")
            {
                editButton.IsVisible = false;
                deleteButton.IsVisible = false;
            }

            info.Children.Add(articleText);
            info.Children.Add(nameText);
            info.Children.Add(categoryText);
            info.Children.Add(manufacturerText);
            info.Children.Add(supplierText);
            info.Children.Add(quantityText);
            info.Children.Add(discountText);
            info.Children.Add(oldPriceText);
            info.Children.Add(finalPriceText);
            info.Children.Add(descriptionText);
            info.Children.Add(buttonsPanel);

            grid.Children.Add(image);
            grid.Children.Add(info);

            card.Child = grid;

            ProductsPanel.Children.Add(card);
        }
    }
    private void ConfigureRole()
    {
        switch (currentUser.Role)
        {
            case "Гость":

                AddButton.IsVisible = false;
                EditButton.IsVisible = false;
                DeleteButton.IsVisible = false;
                OrdersButton.IsVisible = false;
                SearchPanel.IsVisible = false;
                SortPanel.IsVisible = false;
                DisPanel.IsVisible = false;
                OrdersButton.IsVisible = false;

                break;

            case "Авторизированный клиент":

                AddButton.IsVisible = false;
                EditButton.IsVisible = false;
                DeleteButton.IsVisible = false;
                SearchPanel.IsVisible = false;
                SortPanel.IsVisible = false;
                DisPanel.IsVisible = false;
                OrdersButton.IsVisible = false;

                break;

            case "Менеджер":

                AddButton.IsVisible = false;
                EditButton.IsVisible = false;
                DeleteButton.IsVisible = false;
                OrdersButton.IsVisible = false;

                break;

            case "Администратор":

                break;
        }
    }
    private void ApplyFilters()
    {
        IEnumerable<Product> products =
            allProducts;

        string search =
            SearchTextBox.Text?.Trim() ?? "";

        if (!string.IsNullOrWhiteSpace(search))
        {
            products = products.Where(p =>

                p.Name.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)

                ||

                p.Manufacturer.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)

                ||

                p.Supplier.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase));
        }

        switch (DiscountComboBox.SelectedIndex)
        {
            case 1:

                products = products.Where(
                    p => p.Discount >= 0 &&
                         p.Discount < 12);

                break;

            case 2:

                products = products.Where(
                    p => p.Discount >= 12 &&
                         p.Discount < 19);

                break;

            case 3:

                products = products.Where(
                    p => p.Discount >= 19);

                break;
        }

        switch (SortComboBox.SelectedIndex)
        {
            case 1:

                products =
                    products.OrderBy(p => p.Price);

                break;

            case 2:

                products =
                    products.OrderByDescending(
                        p => p.Price);

                break;
        }

        ShowProducts(products.ToList());
    }
    private void ExitButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoginWindow loginWindow = new();

        loginWindow.Show();

        Close();
    }
    private void OrdersButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OrderWindow window = new();

        window.Show();
    }
    private async void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ProductEditWindow window = new();

        await window.ShowDialog(this);

        LoadProducts();
    }
    private async void EditButton_Click(
    object? sender,
    Avalonia.Interactivity.RoutedEventArgs e)
    {
        Product product =
            (Product)((Button)sender!).Tag!;

        ProductEditWindow window =
            new ProductEditWindow(product);

        await window.ShowDialog(this);

        LoadProducts();
    }
    private void DeleteButton_Click(
    object? sender,
    Avalonia.Interactivity.RoutedEventArgs e)
    {
        Product product =
            (Product)((Button)sender!).Tag!;

        DeleteProduct(product);

        LoadProducts();
    }
    private void DeleteProduct(Product product)
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        string query =
            "DELETE FROM products WHERE article = @article";

        using var command =
            new NpgsqlCommand(query, connection);

        command.Parameters.AddWithValue(
            "article",
            product.Article);

        command.ExecuteNonQuery();
    }
}