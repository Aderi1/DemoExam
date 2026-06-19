using Avalonia.Controls;
using Avalonia.Interactivity;
using DemoExam.Models;
using Npgsql;
using System;
using System.Collections.Generic;

namespace DemoExam.Views.Orders;

public partial class OrderEditWindow : Window
{
    private readonly string connectionString =
        "Host=localhost;Port=5432;Username=postgres;Password=0000;Database=demoexam";

    private Order? currentOrder;

    public bool IsSaved { get; private set; }

    public OrderEditWindow()
    {
        InitializeComponent();

        LoadComboBoxes();

        SetDefaultValues();
    }

    public OrderEditWindow(Order order)
    {
        InitializeComponent();

        currentOrder = order;

        LoadComboBoxes();

        LoadOrderData(order.OrderId);
    }
    private void LoadComboBoxes()
    {
        LoadProducts();
        LoadUsers();
        LoadPickupPoints();
        LoadStatuses();
    }
    private void LoadProducts()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        string query =
            """
        SELECT article, name
        FROM products
        ORDER BY name
        """;

        using var command =
            new NpgsqlCommand(query, connection);

        using var reader =
            command.ExecuteReader();

        List<ProductShort> products = new();

        while (reader.Read())
        {
            products.Add(new ProductShort
            {
                Article = reader.GetString(0),
                Name = reader.GetString(1)
            });
        }

        ProductComboBox.ItemsSource = products;
    }
    private void LoadUsers()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        string query =
            """
        SELECT id, full_name
        FROM users
        ORDER BY full_name
        """;

        using var command =
            new NpgsqlCommand(query, connection);

        using var reader =
            command.ExecuteReader();

        List<UserShort> users = new();

        while (reader.Read())
        {
            users.Add(new UserShort
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1)
            });
        }

        UserComboBox.ItemsSource = users;
    }
    private void LoadPickupPoints()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        string query =
            """
        SELECT id, address
        FROM pickup_points
        ORDER BY address
        """;

        using var command =
            new NpgsqlCommand(query, connection);

        using var reader =
            command.ExecuteReader();

        List<PickupPoint> points = new();

        while (reader.Read())
        {
            points.Add(new PickupPoint
            {
                Id = reader.GetInt32(0),
                Address = reader.GetString(1)
            });
        }

        PickupPointComboBox.ItemsSource = points;
    }
    private void LoadStatuses()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        string query =
            """
        SELECT id, name
        FROM order_statuses
        ORDER BY id
        """;

        using var command =
            new NpgsqlCommand(query, connection);

        using var reader =
            command.ExecuteReader();

        List<OrderStatus> statuses = new();

        while (reader.Read())
        {
            statuses.Add(new OrderStatus
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        StatusComboBox.ItemsSource = statuses;
    }
    private void SaveButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        try
        {
            HideError();

            if (!ValidateForm())
                return;

            if (currentOrder == null)
            {
                InsertOrder();
            }
            else
            {
                UpdateOrder();
            }

            IsSaved = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowError($"Не удалось сохранить заказ: {ex.Message}");
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SetDefaultValues()
    {
        OrderDatePicker.SelectedDate = DateTime.Today;
        DeliveryDatePicker.SelectedDate = DateTime.Today;
        QuantityTextBox.Text = "1";
        ReceiveCodeTextBox.Text = "";

        SelectFirstItem(ProductComboBox);
        SelectFirstItem(UserComboBox);
        SelectFirstItem(PickupPointComboBox);
        SelectFirstItem(StatusComboBox);
    }

    private static void SelectFirstItem(ComboBox comboBox)
    {
        if (comboBox.ItemCount > 0)
            comboBox.SelectedIndex = 0;
    }

    private bool ValidateForm()
    {
        if (ProductComboBox.SelectedItem is not ProductShort)
            return ShowValidationError("Выберите товар.");

        if (UserComboBox.SelectedItem is not UserShort)
            return ShowValidationError("Выберите клиента.");

        if (PickupPointComboBox.SelectedItem is not PickupPoint)
            return ShowValidationError("Выберите пункт выдачи.");

        if (StatusComboBox.SelectedItem is not OrderStatus)
            return ShowValidationError("Выберите статус заказа.");

        if (OrderDatePicker.SelectedDate == null)
            return ShowValidationError("Выберите дату заказа.");

        if (DeliveryDatePicker.SelectedDate == null)
            return ShowValidationError("Выберите дату доставки.");

        if (DeliveryDatePicker.SelectedDate.Value.Date <
            OrderDatePicker.SelectedDate.Value.Date)
        {
            return ShowValidationError(
                "Дата доставки не может быть раньше даты заказа.");
        }

        if (!int.TryParse(QuantityTextBox.Text, out int quantity) ||
            quantity <= 0)
        {
            return ShowValidationError(
                "Количество должно быть целым положительным числом.");
        }

        if (!int.TryParse(ReceiveCodeTextBox.Text, out int receiveCode) ||
            receiveCode <= 0)
        {
            return ShowValidationError(
                "Код получения должен быть целым положительным числом.");
        }

        return true;
    }

    private bool ShowValidationError(string message)
    {
        ShowError(message);
        return false;
    }

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorTextBlock.IsVisible = true;
    }

    private void HideError()
    {
        ErrorTextBlock.Text = "";
        ErrorTextBlock.IsVisible = false;
    }
    private void FillFields()
    {
        if (currentOrder == null)
            return;

        OrderDatePicker.SelectedDate =
            currentOrder.OrderDate;

        DeliveryDatePicker.SelectedDate =
            currentOrder.DeliveryDate;

        ReceiveCodeTextBox.Text =
            currentOrder.ReceiveCode.ToString();

        QuantityTextBox.Text =
            currentQuantity.ToString();

        SelectComboBoxes();
    }
    private void SelectComboBoxes()
    {
        foreach (UserShort user in UserComboBox.ItemsSource!)
        {
            if (user.Id == currentUserId)
            {
                UserComboBox.SelectedItem = user;
                break;
            }
        }

        foreach (PickupPoint point in PickupPointComboBox.ItemsSource!)
        {
            if (point.Id == currentPickupPointId)
            {
                PickupPointComboBox.SelectedItem = point;
                break;
            }
        }

        foreach (OrderStatus status in StatusComboBox.ItemsSource!)
        {
            if (status.Id == currentStatusId)
            {
                StatusComboBox.SelectedItem = status;
                break;
            }
        }

        foreach (ProductShort product in ProductComboBox.ItemsSource!)
        {
            if (product.Article == currentArticle)
            {
                ProductComboBox.SelectedItem = product;
                break;
            }
        }
    }
    private int GetNextOrderId()
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        using var command =
            new NpgsqlCommand(
                "SELECT COALESCE(MAX(id),0)+1 FROM orders",
                connection);

        return Convert.ToInt32(
            command.ExecuteScalar());
    }
    private void InsertOrder()
    {
        int newOrderId =
            GetNextOrderId();

        ProductShort product =
            (ProductShort)ProductComboBox.SelectedItem!;

        UserShort user =
            (UserShort)UserComboBox.SelectedItem!;

        PickupPoint point =
            (PickupPoint)PickupPointComboBox.SelectedItem!;

        OrderStatus status =
            (OrderStatus)StatusComboBox.SelectedItem!;

        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            using (var command =
                new NpgsqlCommand(
                    """
                INSERT INTO orders
                (
                    id,
                    order_date,
                    delivery_date,
                    pickup_point_id,
                    user_id,
                    receive_code,
                    status_id
                )
                VALUES
                (
                    @id,
                    @order_date,
                    @delivery_date,
                    @pickup_point_id,
                    @user_id,
                    @receive_code,
                    @status_id
                )
                """,
                    connection,
                    transaction))
            {
                command.Parameters.AddWithValue(
                    "id",
                    newOrderId);

                command.Parameters.AddWithValue(
                    "order_date",
                    OrderDatePicker.SelectedDate!.Value.Date);

                command.Parameters.AddWithValue(
                    "delivery_date",
                    DeliveryDatePicker.SelectedDate!.Value.Date);

                command.Parameters.AddWithValue(
                    "pickup_point_id",
                    point.Id);

                command.Parameters.AddWithValue(
                    "user_id",
                    user.Id);

                command.Parameters.AddWithValue(
                    "receive_code",
                    int.Parse(ReceiveCodeTextBox.Text!));

                command.Parameters.AddWithValue(
                    "status_id",
                    status.Id);

                command.ExecuteNonQuery();
            }

            using (var command =
                new NpgsqlCommand(
                    """
                INSERT INTO order_items
                (
                    order_id,
                    product_article,
                    quantity
                )
                VALUES
                (
                    @order_id,
                    @article,
                    @quantity
                )
                """,
                    connection,
                    transaction))
            {
                command.Parameters.AddWithValue(
                    "order_id",
                    newOrderId);

                command.Parameters.AddWithValue(
                    "article",
                    product.Article);

                command.Parameters.AddWithValue(
                    "quantity",
                    int.Parse(QuantityTextBox.Text!));

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            if (transaction.Connection != null)
                transaction.Rollback();

            throw;
        }
    }
    private int currentUserId;
    private int currentPickupPointId;
    private int currentStatusId;
    private string currentArticle = "";
    private int currentQuantity;
    private void LoadOrderData(int orderId)
    {
        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        string query =
        """
    SELECT
        o.user_id,
        o.pickup_point_id,
        o.status_id,
        oi.product_article,
        oi.quantity
    FROM orders o
    JOIN order_items oi
        ON oi.order_id = o.id
    WHERE o.id = @id
    """;

        using var command =
            new NpgsqlCommand(query, connection);

        command.Parameters.AddWithValue("id", orderId);

        using var reader =
            command.ExecuteReader();

        if (!reader.Read())
            return;

        currentUserId = reader.GetInt32(0);
        currentPickupPointId = reader.GetInt32(1);
        currentStatusId = reader.GetInt32(2);
        currentArticle = reader.GetString(3);
        currentQuantity = reader.GetInt32(4);

        FillFields();
    }

    private void UpdateOrder()
    {
        ProductShort product =
            (ProductShort)ProductComboBox.SelectedItem!;

        UserShort user =
            (UserShort)UserComboBox.SelectedItem!;

        PickupPoint point =
            (PickupPoint)PickupPointComboBox.SelectedItem!;

        OrderStatus status =
            (OrderStatus)StatusComboBox.SelectedItem!;

        using var connection =
            new NpgsqlConnection(connectionString);

        connection.Open();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            using (var command =
                new NpgsqlCommand(
                    """
                UPDATE orders
                SET
                    order_date = @order_date,
                    delivery_date = @delivery_date,
                    pickup_point_id = @pickup_point_id,
                    user_id = @user_id,
                    receive_code = @receive_code,
                    status_id = @status_id
                WHERE id = @id
                """,
                    connection,
                    transaction))
            {
                command.Parameters.AddWithValue(
                    "id",
                    currentOrder!.OrderId);

                command.Parameters.AddWithValue(
                    "order_date",
                    OrderDatePicker.SelectedDate!.Value.Date);

                command.Parameters.AddWithValue(
                    "delivery_date",
                    DeliveryDatePicker.SelectedDate!.Value.Date);

                command.Parameters.AddWithValue(
                    "pickup_point_id",
                    point.Id);

                command.Parameters.AddWithValue(
                    "user_id",
                    user.Id);

                command.Parameters.AddWithValue(
                    "receive_code",
                    int.Parse(ReceiveCodeTextBox.Text!));

                command.Parameters.AddWithValue(
                    "status_id",
                    status.Id);

                command.ExecuteNonQuery();
            }

            using (var command =
                new NpgsqlCommand(
                    """
                UPDATE order_items
                SET
                    product_article = @article,
                    quantity = @quantity
                WHERE order_id = @order_id
                """,
                    connection,
                    transaction))
            {
                command.Parameters.AddWithValue(
                    "order_id",
                    currentOrder.OrderId);

                command.Parameters.AddWithValue(
                    "article",
                    product.Article);

                command.Parameters.AddWithValue(
                    "quantity",
                    int.Parse(QuantityTextBox.Text!));

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch
        {
            if (transaction.Connection != null)
                transaction.Rollback();

            throw;
        }
    }
}
