using Avalonia.Controls;
using DemoExam.Models;
using System.Collections.Generic;
using Npgsql;
using System;

namespace DemoExam.Views.Products;

public partial class ProductEditWindow : Window
{
    private Product? currentProduct;

    private readonly string connectionString =
        "Host=localhost;Port=5432;Username=postgres;Password=0000;Database=demoexam";

    public ProductEditWindow()
    {
        InitializeComponent();

        LoadComboBoxes();
    }

    public ProductEditWindow(Product product)
    {
        InitializeComponent();

        currentProduct = product;

        LoadComboBoxes();

        FillFields();
    }
    private void LoadComboBoxes()
    {
        LoadCategories();
        LoadSuppliers();
        LoadManufacturers();
    }
    private void LoadCategories()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        string query =
            "SELECT id, name FROM categories ORDER BY name";

        using var command =
            new NpgsqlCommand(query, connection);

        using var reader =
            command.ExecuteReader();

        List<Category> categories = new();

        while (reader.Read())
        {
            Category category = new();

            category.Id = reader.GetInt32(0);
            category.Name = reader.GetString(1);

            categories.Add(category);
        }

        CategoryComboBox.ItemsSource = categories;
    }
    private void LoadSuppliers()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        string query =
            "SELECT id, name FROM suppliers ORDER BY name";

        using var command =
            new NpgsqlCommand(query, connection);

        using var reader =
            command.ExecuteReader();

        List<Supplier> suppliers = new();

        while (reader.Read())
        {
            Supplier supplier = new();

            supplier.Id = reader.GetInt32(0);
            supplier.Name = reader.GetString(1);

            suppliers.Add(supplier);
        }

        SupplierComboBox.ItemsSource = suppliers;
    }
    private void LoadManufacturers()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        string query =
            "SELECT id, name FROM manufacturers ORDER BY name";

        using var command =
            new NpgsqlCommand(query, connection);

        using var reader =
            command.ExecuteReader();

        List<Manufacturer> manufacturers = new();

        while (reader.Read())
        {
            Manufacturer manufacturer = new();

            manufacturer.Id = reader.GetInt32(0);
            manufacturer.Name = reader.GetString(1);

            manufacturers.Add(manufacturer);
        }

        ManufacturerComboBox.ItemsSource =
            manufacturers;
    }
    private void SaveButton_Click(
    object? sender,
    Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            if (currentProduct == null)
            {
                InsertProduct();
            }
            else
            {
                UpdateProduct();
            }

            Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private void UpdateProduct()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        Category category =
            (Category)CategoryComboBox.SelectedItem!;

        Supplier supplier =
            (Supplier)SupplierComboBox.SelectedItem!;

        Manufacturer manufacturer =
            (Manufacturer)ManufacturerComboBox.SelectedItem!;

        string query =
        """
    UPDATE products
    SET
        name = @name,
        unit = @unit,
        price = @price,
        supplier_id = @supplier_id,
        manufacturer_id = @manufacturer_id,
        category_id = @category_id,
        discount = @discount,
        quantity = @quantity,
        description = @description,
        photo = @photo
    WHERE article = @article
    """;

        using var command =
            new NpgsqlCommand(query, connection);

        command.Parameters.AddWithValue(
            "article",
            ArticleTextBox.Text ?? "");

        command.Parameters.AddWithValue(
            "name",
            NameTextBox.Text ?? "");

        command.Parameters.AddWithValue(
            "unit",
            UnitTextBox.Text ?? "");

        command.Parameters.AddWithValue(
            "price",
            decimal.Parse(PriceTextBox.Text ?? "0"));

        command.Parameters.AddWithValue(
            "supplier_id",
            supplier.Id);

        command.Parameters.AddWithValue(
            "manufacturer_id",
            manufacturer.Id);

        command.Parameters.AddWithValue(
            "category_id",
            category.Id);

        command.Parameters.AddWithValue(
            "discount",
            int.Parse(DiscountTextBox.Text ?? "0"));

        command.Parameters.AddWithValue(
            "quantity",
            int.Parse(QuantityTextBox.Text ?? "0"));

        command.Parameters.AddWithValue(
            "description",
            DescriptionTextBox.Text ?? "");

        command.Parameters.AddWithValue(
            "photo",
            PhotoTextBox.Text ?? "");

        command.ExecuteNonQuery();
    }
    private void InsertProduct()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        Category category =
            (Category)CategoryComboBox.SelectedItem!;

        Supplier supplier =
            (Supplier)SupplierComboBox.SelectedItem!;

        Manufacturer manufacturer =
            (Manufacturer)ManufacturerComboBox.SelectedItem!;

        string query =
        """
    INSERT INTO products
    (
        article,
        name,
        unit,
        price,
        supplier_id,
        manufacturer_id,
        category_id,
        discount,
        quantity,
        description,
        photo
    )
    VALUES
    (
        @article,
        @name,
        @unit,
        @price,
        @supplier_id,
        @manufacturer_id,
        @category_id,
        @discount,
        @quantity,
        @description,
        @photo
    )
    """;

        using var command =
            new NpgsqlCommand(query, connection);

        command.Parameters.AddWithValue(
            "article",
            ArticleTextBox.Text ?? "");

        command.Parameters.AddWithValue(
            "name",
            NameTextBox.Text ?? "");

        command.Parameters.AddWithValue(
            "unit",
            UnitTextBox.Text ?? "");

        command.Parameters.AddWithValue(
            "price",
            decimal.Parse(PriceTextBox.Text ?? "0"));

        command.Parameters.AddWithValue(
            "supplier_id",
            supplier.Id);

        command.Parameters.AddWithValue(
            "manufacturer_id",
            manufacturer.Id);

        command.Parameters.AddWithValue(
            "category_id",
            category.Id);

        command.Parameters.AddWithValue(
            "discount",
            int.Parse(DiscountTextBox.Text ?? "0"));

        command.Parameters.AddWithValue(
            "quantity",
            int.Parse(QuantityTextBox.Text ?? "0"));

        command.Parameters.AddWithValue(
            "description",
            DescriptionTextBox.Text ?? "");

        command.Parameters.AddWithValue(
            "photo",
            PhotoTextBox.Text ?? "");

        command.ExecuteNonQuery();
    }
    private void FillFields()
    {
        if (currentProduct == null)
            return;

        ArticleTextBox.Text = currentProduct.Article;

        NameTextBox.Text = currentProduct.Name;

        UnitTextBox.Text = currentProduct.Unit;

        PriceTextBox.Text =
            currentProduct.Price.ToString();

        DiscountTextBox.Text =
            currentProduct.Discount.ToString();

        QuantityTextBox.Text =
            currentProduct.Quantity.ToString();

        DescriptionTextBox.Text =
            currentProduct.Description;

        PhotoTextBox.Text =
            currentProduct.Photo;

        ArticleTextBox.IsEnabled = false;

        SelectComboBoxValues();
    }
    private void SelectComboBoxValues()
    {
        foreach (Category category in CategoryComboBox.ItemsSource!)
        {
            if (category.Name == currentProduct!.Category)
            {
                CategoryComboBox.SelectedItem = category;
                break;
            }
        }

        foreach (Supplier supplier in SupplierComboBox.ItemsSource!)
        {
            if (supplier.Name == currentProduct!.Supplier)
            {
                SupplierComboBox.SelectedItem = supplier;
                break;
            }
        }

        foreach (Manufacturer manufacturer in ManufacturerComboBox.ItemsSource!)
        {
            if (manufacturer.Name == currentProduct!.Manufacturer)
            {
                ManufacturerComboBox.SelectedItem = manufacturer;
                break;
            }
        }
    }
}