using System;
using System.Data;
using System.Windows;
using Npgsql;

namespace demo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = TextBox1.Text.Trim();
                string password = TextBox2.Text.Trim();

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Поля обязательны для заполнения",
                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string connString = "Host=localhost;Username=postgres;Password=root;Database=postgres";

                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    // Шаг 1: Получение данных пользователя
                    var checkUserCmd = new NpgsqlCommand(
                        "SELECT id, login, password, role, surname, count, date, active " +
                        "FROM \"Users\" WHERE login = @login",
                        conn);
                    checkUserCmd.Parameters.AddWithValue("login", login);

                    int userId = -1;
                    byte active = 0; // Для хранения bit(1)
                    string userDbPassword = "";
                    DateTime lastLogin = DateTime.MinValue;
                    int currentCount = 0;
                    string surname = "";
                    int role = -1;

                    using (var reader = checkUserCmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            MessageBox.Show("Неверный логин или пароль",
                                           "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        reader.Read();

                        userId = Convert.ToInt32(reader["id"]);
                        active = reader["active"] is byte[] bitArray
                            ? (byte)(bitArray as byte[])[0]
                            : Convert.ToByte(reader["active"]);
                        userDbPassword = reader["password"].ToString();
                        lastLogin = Convert.ToDateTime(reader["date"]);
                        currentCount = Convert.ToInt32(reader["count"]);
                        surname = reader["surname"].ToString();
                        role = Convert.ToInt32(reader["role"]);
                    } // Здесь закрывается DataReader

                    // Проверка блокировки (bit = 0 - активен, 1 - заблокирован)
                    if (active == 1)
                    {
                        MessageBox.Show("Вы заблокированы. Обратитесь к администратору",
                                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Шаг 2: Проверка пароля
                    var checkPassCmd = new NpgsqlCommand(
                        "SELECT password FROM \"Users\" WHERE id = @id",
                        conn);
                    checkPassCmd.Parameters.AddWithValue("id", userId);

                    string dbPassword = checkPassCmd.ExecuteScalar()?.ToString();

                    if (dbPassword != password)
                    {
                        // Шаг 3: Обновление счетчика и блокировка
                        var updateCmd = new NpgsqlCommand(
                            "UPDATE \"Users\" SET " +
                            "count = CASE WHEN count < 3 THEN count + 1 ELSE 3 END, " +
                            "active = CASE WHEN count >= 3 THEN B'1' ELSE active END " +
                            "WHERE id = @id",
                            conn);
                        updateCmd.Parameters.AddWithValue("id", userId);
                        updateCmd.ExecuteNonQuery();

                        MessageBox.Show($"Неверный пароль. Осталось попыток: {3 - currentCount}",
                                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Шаг 4: Проверка срока активности
                    if (lastLogin < DateTime.Now.AddMonths(-1))
                    {
                        var blockCmd = new NpgsqlCommand(
                            "UPDATE \"Users\" SET active = B'1' WHERE id = @id",
                            conn);
                        blockCmd.Parameters.AddWithValue("id", userId);
                        blockCmd.ExecuteNonQuery();

                        MessageBox.Show("Аккаунт заблокирован из-за долгого отсутствия",
                                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Шаг 5: Сброс счетчика и авторизация
                    var resetCmd = new NpgsqlCommand(
                        "UPDATE \"Users\" SET count = 0, date = @date WHERE id = @id",
                        conn);
                    resetCmd.Parameters.AddWithValue("date", DateTime.Now);
                    resetCmd.Parameters.AddWithValue("id", userId);
                    resetCmd.ExecuteNonQuery();

                    // Авторизация успешна
                    switch (role)
                    {
                        case 1:
                            MessageBox.Show($"Добро пожаловать как руководитель {surname}");
                            new Window1().Show();
                            break;
                        case 2:
                            MessageBox.Show($"Добро пожаловать как администратор {surname}");
                            new Window2().Show();
                            break;
                        case 3:
                            MessageBox.Show($"Добро пожаловать как клиент {surname}");
                            new UpdatePass(userId).Show();
                            break;
                        default:
                            MessageBox.Show("Неизвестная роль");
                            break;
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка БД: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}