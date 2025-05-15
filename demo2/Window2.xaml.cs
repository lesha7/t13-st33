using System;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace demo
{
    public partial class Window2 : Window
    {
        private ObservableCollection<User> _userList = new ObservableCollection<User>();
        private readonly string _connString = "Host=localhost;Username=postgres;Password=root;Database=postgres";

        public Window2()
        {
            InitializeComponent();
            InitializeRoles();
            LoadUsers();
        }

        // Инициализация ролей
        private void InitializeRoles()
        {
            var roles = new Dictionary<string, string>
            {
                { "1", "Руководитель (1)" },
                { "2", "Администратор (2)" },
                { "3", "Клиент (3)" }
            };

            typeRole.ItemsSource = roles;
            typeRole.DisplayMemberPath = "Value";
            typeRole.SelectedValuePath = "Key";
            typeRole.SelectedIndex = 0;
        }

        // Загрузка пользователей
        private void LoadUsers()
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand("SELECT id, surname, name, othcestvo, role, login, password, count, active::int AS active, date FROM \"Users\"", conn);
                    var reader = cmd.ExecuteReader();

                    _userList.Clear();
                    while (reader.Read())
                    {
                        _userList.Add(new User
                        {
                            ID = reader.GetInt32(0),
                            Surname = reader.GetString(1),
                            Name = reader.GetString(2),
                            Othcestvo = reader.GetString(3),
                            Role = reader.GetString(4),
                            Login = reader.GetString(5),
                            Password = reader.GetString(6),
                            Count = reader.GetInt32(7),
                            Active = reader.GetInt32(8) == 1,
                            Date = reader.GetDateTime(9)
                        });
                    }
                }
                GridUser.ItemsSource = _userList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        // Обработчик кнопки "Добавить" (active = 0 по умолчанию)
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (typeRole.SelectedValue == null ||
                string.IsNullOrEmpty(TextBox1.Text) ||
                string.IsNullOrEmpty(TextBox2.Text) ||
                string.IsNullOrEmpty(TextBox3.Text) ||
                string.IsNullOrEmpty(TextBox4.Text))
            {
                MessageBox.Show("Заполните все обязательные поля!");
                return;
            }

            try
            {
                using (var conn = new NpgsqlConnection(_connString))
                {
                    conn.Open();
                    var insertCmd = new NpgsqlCommand(
                        "INSERT INTO \"Users\" " +
                        "(surname, name, othcestvo, role, login, password, count, active, date) " +
                        "VALUES (@surname, @name, @othcestvo, @role, @login, @password, @count, @active::bit(1), @date)",
                        conn);

                    insertCmd.Parameters.AddWithValue("@surname", TextBox3.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@name", TextBox4.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@othcestvo", TextBox5.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@role", typeRole.SelectedValue.ToString());
                    insertCmd.Parameters.AddWithValue("@login", TextBox1.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@password", TextBox2.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@count", 0);
                    insertCmd.Parameters.AddWithValue("@active", "0"); // Установка active = 0
                    insertCmd.Parameters.AddWithValue("@date", DateTime.Now);

                    insertCmd.ExecuteNonQuery();
                }

                LoadUsers();
                MessageBox.Show("Пользователь добавлен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Обработчик кнопки "Сохранить изменения"
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connString))
                {
                    conn.Open();
                    foreach (var user in _userList)
                    {
                        var updateCmd = new NpgsqlCommand(
                            "UPDATE \"Users\" SET " +
                            "surname = @surname, " +
                            "name = @name, " +
                            "othcestvo = @othcestvo, " +
                            "role = @role, " +
                            "login = @login, " +
                            "password = @password, " +
                            "count = @count, " +
                            "active = @active::bit(1), " +
                            "date = @date " +
                            "WHERE id = @id",
                            conn);

                        updateCmd.Parameters.AddWithValue("@id", user.ID);
                        updateCmd.Parameters.AddWithValue("@surname", user.Surname);
                        updateCmd.Parameters.AddWithValue("@name", user.Name);
                        updateCmd.Parameters.AddWithValue("@othcestvo", user.Othcestvo);
                        updateCmd.Parameters.AddWithValue("@role", user.Role);
                        updateCmd.Parameters.AddWithValue("@login", user.Login);
                        updateCmd.Parameters.AddWithValue("@password", user.Password);
                        updateCmd.Parameters.AddWithValue("@count", user.Count);
                        updateCmd.Parameters.AddWithValue("@active", user.Active ? "1" : "0");
                        updateCmd.Parameters.AddWithValue("@date", user.Date);

                        updateCmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Изменения сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Обработчик изменения выбора роли
        private void typeRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Логика при необходимости
        }

        // Модель пользователя
        public class User
        {
            public int ID { get; set; }
            public string Surname { get; set; }
            public string Name { get; set; }
            public string Othcestvo { get; set; }
            public string Role { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public int Count { get; set; }
            public bool Active { get; set; }
            public DateTime Date { get; set; }
        }
    }
}