using Avalonia.Controls;
using System;
using System.Collections.Generic;
using Npgsql;
using Avalonia.Interactivity;
using DemoExam.Models;
using System.Linq;

namespace DemoExam.Views.Orders;

public partial class OrderWindow : Window
{
    public OrderWindow()
    {
        InitializeComponent();

        SearchTextBox.TextChanged += (_, _) =>
            ApplyFilter();

        LoadOrders();
    }
    private readonly string connectionString =
    "Host=localhost;Port=5432;Username=postgres;Password=0000;Database=demoexam";

    private List<Order> allOrders = new();

    private void LoadOrders()
    {
        try
        {
            using var connection =
                new NpgsqlConnection(connectionString);

            connection.Open();

            string query =
                "SELECT * FROM view_orders";

            using var command =
                new NpgsqlCommand(query, connection);

            using var reader =
                command.ExecuteReader();

            allOrders.Clear();

            while (reader.Read())
            {
                Order order = new();

                order.OrderId = reader.GetInt32(0);
                order.ProductArticle = reader.GetString(1);

                order.OrderDate = reader.GetDateTime(2);
                order.DeliveryDate = reader.GetDateTime(3);

                order.Address = reader.GetString(4);

                order.FullName = reader.GetString(5);

                order.ReceiveCode = reader.GetInt32(6);

                order.Status = reader.GetString(7);

                allOrders.Add(order);
            }

            OrdersDataGrid.ItemsSource = null;
            OrdersDataGrid.ItemsSource = allOrders;

            CountTextBlock.Text =
                $"Показано {allOrders.Count} заказов";
        }
        catch (Exception ex)
        {
            allOrders.Clear();
            OrdersDataGrid.ItemsSource = null;
            CountTextBlock.Text =
                $"Ошибка загрузки заказов: {ex.Message}";
        }
    }
    private void ApplyFilter()
    {
        string search =
            SearchTextBox.Text?.Trim() ?? "";

        var filtered =
            allOrders.Where(o =>

                o.ProductArticle.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)

                ||

                o.FullName.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)

                ||

                o.Status.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)

            ).ToList();

        OrdersDataGrid.ItemsSource = null;
        OrdersDataGrid.ItemsSource = filtered;

        CountTextBlock.Text =
            $"Показано {filtered.Count} из {allOrders.Count}";
    }
    private async void AddOrderButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        OrderEditWindow window =
            new OrderEditWindow();

        await window.ShowDialog(this);

        if (window.IsSaved)
            LoadOrders();
    }
    private async void EditOrderButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        if (OrdersDataGrid.SelectedItem == null)
            return;

        Order order =
            (Order)OrdersDataGrid.SelectedItem;

        OrderEditWindow window =
            new OrderEditWindow(order);

        await window.ShowDialog(this);

        if (window.IsSaved)
            LoadOrders();
    }
    private void DeleteOrderButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        if (OrdersDataGrid.SelectedItem == null)
            return;

        Order order =
            (Order)OrdersDataGrid.SelectedItem;

        try
        {
            DeleteOrder(order);
            LoadOrders();
        }
        catch (Exception ex)
        {
            CountTextBlock.Text =
                $"Ошибка удаления заказа: {ex.Message}";
        }
    }
    private void DeleteOrder(Order order)
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            using var deleteItems =
                new NpgsqlCommand(
                    "DELETE FROM order_items WHERE order_id = @id",
                    connection,
                    transaction);

            deleteItems.Parameters.AddWithValue(
                "id",
                order.OrderId);

            deleteItems.ExecuteNonQuery();

            using var deleteOrder =
                new NpgsqlCommand(
                    "DELETE FROM orders WHERE id = @id",
                    connection,
                    transaction);

            deleteOrder.Parameters.AddWithValue(
                "id",
                order.OrderId);

            deleteOrder.ExecuteNonQuery();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
    private void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        LoadOrders();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

}
