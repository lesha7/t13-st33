using System;
using System.Data;
using System.Windows;
using Npgsql;

namespace demo
{
    public partial class UpdatePass : Window
    {
        private int userId;

        public UpdatePass(int userId)
        {
            InitializeComponent();
            this.userId = userId;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string oldPass = TextBox3.Text.Trim();
                string newPass = TextBox4.Text.Trim();
                string confirmPass = TextBox5.Text.Trim();

                // Проверка обязательных полей
                if (string.IsNullOrEmpty(oldPass) ||
                    string.IsNullOrEmpty(newPass) ||
                    string.IsNullOrEmpty(confirmPass))
                {
                    MessageBox.Show("Все поля обязательны для заполнения!");
                    return;
                }

                // Проверка совпадения новых паролей
                if (newPass != confirmPass)
                {
                    MessageBox.Show("Новые пароли не совпадают!");
                    return;
                }

                // Подключение к БД
                string connString = "Host=localhost;Username=postgres;Password=root;Database=postgres";

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    // Проверка старого пароля
                    var checkCmd = new NpgsqlCommand(
                        "SELECT password FROM \"Users\" WHERE id = @id", conn);
                    checkCmd.Parameters.AddWithValue("id", userId);

                    string dbPassword = checkCmd.ExecuteScalar()?.ToString();

                    if (dbPassword != oldPass)
                    {
                        MessageBox.Show("Старый пароль неверен!");
                        return;
                    }

                    // Обновление пароля
                    var updateCmd = new NpgsqlCommand(
                        "UPDATE \"Users\" SET password = @newPass WHERE id = @id", conn);
                    updateCmd.Parameters.AddWithValue("newPass", newPass);
                    updateCmd.Parameters.AddWithValue("id", userId);

                    int rowsAffected = updateCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Пароль успешно изменен!");

                        // Возвращаемся на главный экран авторизации
                        new MainWindow().Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при изменении пароля");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}