using Avalonia.Controls;
using Avalonia.Interactivity;
using DemoExam.Models;
using DemoExam.Views.Products;
using Npgsql;
using System;

namespace DemoExam.Views.Login;

public partial class LoginWindow : Window
{
    private readonly string connectionString =
        "Host=localhost;Port=5432;Username=postgres;Password=0000;Database=demoexam";

    public LoginWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionString);

            connection.Open();

            string query =
            """
            SELECT
                u.id,
                u.full_name,
                u.login,
                u.password,
                r.name
            FROM users u
            JOIN roles r
                ON r.id = u.role_id
            WHERE
                u.login = @login
                AND u.password = @password
            """;

            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "login",
                LoginTextBox.Text ?? ""
            );

            command.Parameters.AddWithValue(
                "password",
                PasswordTextBox.Text ?? ""
            );

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                User user = new User();

                user.Id = reader.GetInt32(0);
                user.FullName = reader.GetString(1);
                user.Login = reader.GetString(2);
                user.Password = reader.GetString(3);
                user.Role = reader.GetString(4);

                ProductWindow productWindow =
                    new ProductWindow(user);

                productWindow.Show();

                Close();
            }
            else
            {
                ErrorTextBlock.Text =
                    "Неверный логин или пароль";

                ErrorTextBlock.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ErrorTextBlock.Text =
                $"Ошибка подключения: {ex.Message}";

            ErrorTextBlock.IsVisible = true;
        }
    }

    private void GuestButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        User guest = new User();

        guest.FullName = "Гость";
        guest.Role = "Гость";

        ProductWindow productWindow =
            new ProductWindow(guest);

        productWindow.Show();

        Close();
    }
}