using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using demo;
using Npgsql;
using System.Xml.Linq;

namespace demo2
{
    /// <summary>
    /// Логика взаимодействия для Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        public Window3()
        {
            InitializeComponent();
        }
    }
}











//MainWindow.xaml.cs :
//using System;
//using System.Data;
//using System.Windows;
//using Npgsql;

//namespace demo
//{
//    public partial class MainWindow : Window
//    {
//        public MainWindow()
//        {
//            InitializeComponent();
//        }

//        private void Button1_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                string login = TextBox1.Text.Trim();
//                string password = TextBox2.Text.Trim();

//                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
//                {
//                    MessageBox.Show("Поля обязательны для заполнения",
//                                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//                    return;
//                }

//                string connString = "Host=localhost;Username=postgres;Password=root;Database=postgres";

//                using (var conn = new NpgsqlConnection(connString))
//                {
//                    conn.Open();

//                    // Шаг 1: Получение данных пользователя
//                    var checkUserCmd = new NpgsqlCommand(
//                        "SELECT id, login, password, role, surname, count, date, active " +
//                        "FROM \"Users\" WHERE login = @login",
//                        conn);
//                    checkUserCmd.Parameters.AddWithValue("login", login);

//                    int userId = -1;
//                    byte active = 0; // Для хранения bit(1)
//                    string userDbPassword = "";
//                    DateTime lastLogin = DateTime.MinValue;
//                    int currentCount = 0;
//                    string surname = "";
//                    int role = -1;

//                    using (var reader = checkUserCmd.ExecuteReader())
//                    {
//                        if (!reader.HasRows)
//                        {
//                            MessageBox.Show("Неверный логин или пароль",
//                                           "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//                            return;
//                        }

//                        reader.Read();

//                        userId = Convert.ToInt32(reader["id"]);
//                        active = reader["active"] is byte[] bitArray
//                            ? (byte)(bitArray as byte[])[0]
//                            : Convert.ToByte(reader["active"]);
//                        userDbPassword = reader["password"].ToString();
//                        lastLogin = Convert.ToDateTime(reader["date"]);
//                        currentCount = Convert.ToInt32(reader["count"]);
//                        surname = reader["surname"].ToString();
//                        role = Convert.ToInt32(reader["role"]);
//                    } // Здесь закрывается DataReader

//                    // Проверка блокировки (bit = 0 - активен, 1 - заблокирован)
//                    if (active == 1)
//                    {
//                        MessageBox.Show("Вы заблокированы. Обратитесь к администратору",
//                                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//                        return;
//                    }

//                    // Шаг 2: Проверка пароля
//                    var checkPassCmd = new NpgsqlCommand(
//                        "SELECT password FROM \"Users\" WHERE id = @id",
//                        conn);
//                    checkPassCmd.Parameters.AddWithValue("id", userId);

//                    string dbPassword = checkPassCmd.ExecuteScalar()?.ToString();

//                    if (dbPassword != password)
//                    {
//                        // Шаг 3: Обновление счетчика и блокировка
//                        var updateCmd = new NpgsqlCommand(
//                            "UPDATE \"Users\" SET " +
//                            "count = CASE WHEN count < 3 THEN count + 1 ELSE 3 END, " +
//                            "active = CASE WHEN count >= 3 THEN B'1' ELSE active END " +
//                            "WHERE id = @id",
//                            conn);
//                        updateCmd.Parameters.AddWithValue("id", userId);
//                        updateCmd.ExecuteNonQuery();

//                        MessageBox.Show($"Неверный пароль. Осталось попыток: {3 - currentCount}",
//                                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//                        return;
//                    }

//                    // Шаг 4: Проверка срока активности
//                    if (lastLogin < DateTime.Now.AddMonths(-1))
//                    {
//                        var blockCmd = new NpgsqlCommand(
//                            "UPDATE \"Users\" SET active = B'1' WHERE id = @id",
//                            conn);
//                        blockCmd.Parameters.AddWithValue("id", userId);
//                        blockCmd.ExecuteNonQuery();

//                        MessageBox.Show("Аккаунт заблокирован из-за долгого отсутствия",
//                                       "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//                        return;
//                    }

//                    // Шаг 5: Сброс счетчика и авторизация
//                    var resetCmd = new NpgsqlCommand(
//                        "UPDATE \"Users\" SET count = 0, date = @date WHERE id = @id",
//                        conn);
//                    resetCmd.Parameters.AddWithValue("date", DateTime.Now);
//                    resetCmd.Parameters.AddWithValue("id", userId);
//                    resetCmd.ExecuteNonQuery();

//                    // Авторизация успешна
//                    switch (role)
//                    {
//                        case 1:
//                            MessageBox.Show($"Добро пожаловать как руководитель {surname}");
//                            new Window1().Show();
//                            break;
//                        case 2:
//                            MessageBox.Show($"Добро пожаловать как администратор {surname}");
//                            new Window2().Show();
//                            break;
//                        case 3:
//                            MessageBox.Show($"Добро пожаловать как клиент {surname}");
//                            new UpdatePass(userId).Show();
//                            break;
//                        default:
//                            MessageBox.Show("Неизвестная роль");
//                            break;
//                    }
//                }
//            }
//            catch (NpgsqlException ex)
//            {
//                MessageBox.Show($"Ошибка БД: {ex.Message}",
//                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Ошибка: {ex.Message}",
//                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }
//    }
//}

//MainWindow.xaml :
//< Window x: Class = "demo.MainWindow"
//        xmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation"
//        xmlns: x = "http://schemas.microsoft.com/winfx/2006/xaml"
//        xmlns: d = "http://schemas.microsoft.com/expression/blend/2008"
//        xmlns: mc = "http://schemas.openxmlformats.org/markup-compatibility/2006"
//        xmlns: local = "clr-namespace:demo"
//        mc: Ignorable = "d"
//        Title = "MainWindow" Height = "450" Width = "800" >
//    < Grid >

//        < Label  x: Name = "Label1" Content = "Введите логин" HorizontalAlignment = "Left" Height = "40"
//Margin = "148,46,0,0" VerticalAlignment = "Top" ></ Label >
//        < Label  x: Name = "Label2" Content = "Введите пароль" HorizontalAlignment = "Left" Height = "43"
//Margin = "148,150,0,0" VerticalAlignment = "Top" >
//        </ Label >
//        < TextBox x: Name = "TextBox1" HorizontalAlignment = "Left" Height = "33" Width = "250"
//Margin = "148,86,0,0" TextWrapping = "Wrap" VerticalAlignment = "Top" ></ TextBox >
//        < TextBox x: Name = "TextBox2" HorizontalAlignment = "Left" Height = "33" Width = "250"
//Margin = "148,190,0,0" TextWrapping = "Wrap" VerticalAlignment = "Top" ></ TextBox >
//        < Button x: Name = "button1" Content = "Войти" HorizontalAlignment = "Left" Height = "45" Width = "90"
//Margin = "300,250,0,0" VerticalAlignment = "Top" Click = "Button1_Click" >
//        </ Button >

//    </ Grid >
//</ Window >

//Window1.xaml :
//< Window x: Class = "demo.Window1"
//        xmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation"
//        xmlns: x = "http://schemas.microsoft.com/winfx/2006/xaml"
//        xmlns: d = "http://schemas.microsoft.com/expression/blend/2008"
//        xmlns: mc = "http://schemas.openxmlformats.org/markup-compatibility/2006"
//        xmlns: local = "clr-namespace:demo"
//        mc: Ignorable = "d"
//        Title = "Window1" Height = "450" Width = "800" >
//    < Grid >
//        < Label  x: Name = "Label2" Content = "Вы авторизовались как руководитель"
//HorizontalAlignment = "Left" Height = "40" Margin = "148,46,0,0" VerticalAlignment = "Top" ></ Label >
//    </ Grid >
//</ Window >

//Window2.xaml.cs :
//using System;
//using System.Windows;
//using System.Windows.Controls;
//using Npgsql;
//using System.Collections.ObjectModel;
//using System.Collections.Generic;

//namespace demo
//{
//    public partial class Window2 : Window
//    {
//        private ObservableCollection<User> _userList = new ObservableCollection<User>();
//        private readonly string _connString = "Host=localhost;Username=postgres;Password=root;Database=postgres";

//        public Window2()
//        {
//            InitializeComponent();
//            InitializeRoles();
//            LoadUsers();
//        }

//        // Инициализация ролей
//        private void InitializeRoles()
//        {
//            var roles = new Dictionary<string, string>
//            {
//                { "1", "Руководитель (1)" },
//                { "2", "Администратор (2)" },
//                { "3", "Клиент (3)" }
//            };

//            typeRole.ItemsSource = roles;
//            typeRole.DisplayMemberPath = "Value";
//            typeRole.SelectedValuePath = "Key";
//            typeRole.SelectedIndex = 0;
//        }

//        // Загрузка пользователей
//        private void LoadUsers()
//        {
//            try
//            {
//                using (var conn = new NpgsqlConnection(_connString))
//                {
//                    conn.Open();
//                    var cmd = new NpgsqlCommand("SELECT id, surname, name, othcestvo, role, login, password, count, active::int AS active, date FROM \"Users\"", conn);
//                    var reader = cmd.ExecuteReader();

//                    _userList.Clear();
//                    while (reader.Read())
//                    {
//                        _userList.Add(new User
//                        {
//                            ID = reader.GetInt32(0),
//                            Surname = reader.GetString(1),
//                            Name = reader.GetString(2),
//                            Othcestvo = reader.GetString(3),
//                            Role = reader.GetString(4),
//                            Login = reader.GetString(5),
//                            Password = reader.GetString(6),
//                            Count = reader.GetInt32(7),
//                            Active = reader.GetInt32(8) == 1,
//                            Date = reader.GetDateTime(9)
//                        });
//                    }
//                }
//                GridUser.ItemsSource = _userList;
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
//            }
//        }

//        // Обработчик кнопки "Добавить" (active = 0 по умолчанию)
//        private void button3_Click(object sender, RoutedEventArgs e)
//        {
//            if (typeRole.SelectedValue == null ||
//                string.IsNullOrEmpty(TextBox1.Text) ||
//                string.IsNullOrEmpty(TextBox2.Text) ||
//                string.IsNullOrEmpty(TextBox3.Text) ||
//                string.IsNullOrEmpty(TextBox4.Text))
//            {
//                MessageBox.Show("Заполните все обязательные поля!");
//                return;
//            }

//            try
//            {
//                using (var conn = new NpgsqlConnection(_connString))
//                {
//                    conn.Open();
//                    var insertCmd = new NpgsqlCommand(
//                        "INSERT INTO \"Users\" " +
//                        "(surname, name, othcestvo, role, login, password, count, active, date) " +
//                        "VALUES (@surname, @name, @othcestvo, @role, @login, @password, @count, @active::bit(1), @date)",
//                        conn);

//                    insertCmd.Parameters.AddWithValue("@surname", TextBox3.Text.Trim());
//                    insertCmd.Parameters.AddWithValue("@name", TextBox4.Text.Trim());
//                    insertCmd.Parameters.AddWithValue("@othcestvo", TextBox5.Text.Trim());
//                    insertCmd.Parameters.AddWithValue("@role", typeRole.SelectedValue.ToString());
//                    insertCmd.Parameters.AddWithValue("@login", TextBox1.Text.Trim());
//                    insertCmd.Parameters.AddWithValue("@password", TextBox2.Text.Trim());
//                    insertCmd.Parameters.AddWithValue("@count", 0);
//                    insertCmd.Parameters.AddWithValue("@active", "0"); // Установка active = 0
//                    insertCmd.Parameters.AddWithValue("@date", DateTime.Now);

//                    insertCmd.ExecuteNonQuery();
//                }

//                LoadUsers();
//                MessageBox.Show("Пользователь добавлен!");
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Ошибка: {ex.Message}");
//            }
//        }

//        // Обработчик кнопки "Сохранить изменения"
//        private void button4_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                using (var conn = new NpgsqlConnection(_connString))
//                {
//                    conn.Open();
//                    foreach (var user in _userList)
//                    {
//                        var updateCmd = new NpgsqlCommand(
//                            "UPDATE \"Users\" SET " +
//                            "surname = @surname, " +
//                            "name = @name, " +
//                            "othcestvo = @othcestvo, " +
//                            "role = @role, " +
//                            "login = @login, " +
//                            "password = @password, " +
//                            "count = @count, " +
//                            "active = @active::bit(1), " +
//                            "date = @date " +
//                            "WHERE id = @id",
//                            conn);

//                        updateCmd.Parameters.AddWithValue("@id", user.ID);
//                        updateCmd.Parameters.AddWithValue("@surname", user.Surname);
//                        updateCmd.Parameters.AddWithValue("@name", user.Name);
//                        updateCmd.Parameters.AddWithValue("@othcestvo", user.Othcestvo);
//                        updateCmd.Parameters.AddWithValue("@role", user.Role);
//                        updateCmd.Parameters.AddWithValue("@login", user.Login);
//                        updateCmd.Parameters.AddWithValue("@password", user.Password);
//                        updateCmd.Parameters.AddWithValue("@count", user.Count);
//                        updateCmd.Parameters.AddWithValue("@active", user.Active ? "1" : "0");
//                        updateCmd.Parameters.AddWithValue("@date", user.Date);

//                        updateCmd.ExecuteNonQuery();
//                    }
//                }
//                MessageBox.Show("Изменения сохранены!");
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Ошибка: {ex.Message}");
//            }
//        }

//        // Обработчик изменения выбора роли
//        private void typeRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            // Логика при необходимости
//        }

//        // Модель пользователя
//        public class User
//        {
//            public int ID { get; set; }
//            public string Surname { get; set; }
//            public string Name { get; set; }
//            public string Othcestvo { get; set; }
//            public string Role { get; set; }
//            public string Login { get; set; }
//            public string Password { get; set; }
//            public int Count { get; set; }
//            public bool Active { get; set; }
//            public DateTime Date { get; set; }
//        }
//    }
//}

//Window2.xaml :
//< Window x: Class = "demo.Window2"
//        xmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation"
//        xmlns: x = "http://schemas.microsoft.com/winfx/2006/xaml"
//        xmlns: d = "http://schemas.microsoft.com/expression/blend/2008"
//        xmlns: mc = "http://schemas.openxmlformats.org/markup-compatibility/2006"
//        xmlns: local = "clr-namespace:demo"
//        mc: Ignorable = "d"
//        Title = "РАЗДЕЛ АДМИНИСТРАТОРА СИСТЕМЫ" Height = "450" Width = "800" >
//    < Grid >
//        < Label  x: Name = "Label1" Content = "Добавление пользователей. Изменение данных 
//пользователей."  
//                HorizontalAlignment = "Left" Height = "30" Margin = "48,6,0,0"
//VerticalAlignment = "Top" ></ Label >
//        < Label  x: Name = "Label2" Content = "Имя нового пользователя" HorizontalAlignment = "Left"
//Height = "40" Margin = "48,46,0,0"
//                VerticalAlignment = "Top" ></ Label >
//        < Label  x: Name = "Label3" Content = "Введите пароль" HorizontalAlignment = "Left" Height = "30"
//Margin = "48,90,0,0"
//                VerticalAlignment = "Top" ></ Label >
//        < Label  x: Name = "Label4" Content = "Выберите роль пользователя" HorizontalAlignment = "Left"
//Height = "30" Margin = "48,135,0,0"
//         VerticalAlignment = "Top" ></ Label >
//        < Label  x: Name = "Label5" Content = "Фамилия" HorizontalAlignment = "Left" Height = "30"
//Margin = "48,180,0,0"
// VerticalAlignment = "Top" ></ Label >
//        < Label  x: Name = "Label6" Content = "Имя" HorizontalAlignment = "Left" Height = "30"
//Margin = "300,180,0,0"
//VerticalAlignment = "Top" ></ Label >
//        < Label  x: Name = "Label7" Content = "Отчество" HorizontalAlignment = "Left" Height = "30"
//Margin = "500,180,0,0"
//VerticalAlignment = "Top" ></ Label >
//        < TextBox x: Name = "TextBox1" HorizontalAlignment = "Left" Height = "33" Width = "250"
//Margin = "248,46,0,0"
//                 TextWrapping = "Wrap" VerticalAlignment = "Top" ></ TextBox >
//        < TextBox x: Name = "TextBox2" HorizontalAlignment = "Left" Height = "33" Width = "250"
//Margin = "248,90,0,0"
//                 TextWrapping = "Wrap" VerticalAlignment = "Top" ></ TextBox >
//        < TextBox x: Name = "TextBox3" HorizontalAlignment = "Left" Height = "33" Width = "150"
//Margin = "120,180,0,0"
//         TextWrapping = "Wrap" VerticalAlignment = "Top" ></ TextBox >
//        < TextBox x: Name = "TextBox4" HorizontalAlignment = "Left" Height = "33" Width = "150"
//Margin = "340,180,0,0"
// TextWrapping = "Wrap" VerticalAlignment = "Top" ></ TextBox >
//        < TextBox x: Name = "TextBox5" HorizontalAlignment = "Left" Height = "33" Width = "150"
//Margin = "570,180,0,0"
// TextWrapping = "Wrap" VerticalAlignment = "Top" ></ TextBox >
//        < ComboBox x: Name = "typeRole" FontSize = "14" HorizontalAlignment = "Left" Height = "33"
//Width = "250"
//                  VerticalAlignment = "Top" Margin = "248,135,0,0" SelectionChanged = "typeRole_SelectionChanged" >
//        </ ComboBox >
//        < Button x: Name = "button3" Content = "Добавить"  Click = "button3_Click" HorizontalAlignment = "Left" Height = "30"
//Width = "90"
//                Margin = "550,90,0,0" VerticalAlignment = "Top" >
//        </ Button >
//        < Button x: Name = "button4" Content = "Сохранить изменения"  Click = "button4_Click" HorizontalAlignment = "Left"
//Height = "30" Width = "130"
//        Margin = "550,140,0,0" VerticalAlignment = "Top" >
//        </ Button >

//        < StackPanel Orientation = "Vertical" HorizontalAlignment = "Center" VerticalAlignment = "Top"
//Margin = "5,230,5,5" >
//            < DataGrid Name = "GridUser" AutoGenerateColumns = "False" Height = "190" MaxWidth = "750" >
//                < DataGrid.Columns >
//                    < DataGridTextColumn Header = "ID" Width = "*" Binding = "{Binding ID}" />
//                    < DataGridTextColumn Header = "Фамилия" Width = "*" Binding = "{Binding Surname}" />
//                    < DataGridTextColumn Header = "Имя" Width = "*" Binding = "{Binding Name}" />
//                    < DataGridTextColumn Header = "Отчество" Width = "*" Binding = "{Binding Othcestvo}" />
//                    < DataGridTextColumn Header = "Роль" Width = "*" Binding = "{Binding Role}" />
//                    < DataGridTextColumn Header = "Логин" Width = "*" Binding = "{Binding Login}" />
//                    < DataGridTextColumn Header = "Пароль" Width = "*" Binding = "{Binding Password}" />
//                    < DataGridTextColumn Header = "Кол-во вводов" Width = "*" Binding = "{Binding Count}" />
//                    < DataGridTextColumn Header = "Активность" Width = "*" Binding = "{Binding Active}" />
//                    < DataGridTextColumn Header = "Дата входа" Width = "*" Binding = "{Binding Date}" />
//                </ DataGrid.Columns >
//            </ DataGrid >
//        </ StackPanel >
//    </ Grid >
//</ Window >

//UpdatePass.xaml.cs :
//using System;
//using System.Data;
//using System.Windows;
//using Npgsql;

//namespace demo
//{
//    public partial class UpdatePass : Window
//    {
//        private int userId;

//        public UpdatePass(int userId)
//        {
//            InitializeComponent();
//            this.userId = userId;
//        }

//        private void Button_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                string oldPass = TextBox3.Text.Trim();
//                string newPass = TextBox4.Text.Trim();
//                string confirmPass = TextBox5.Text.Trim();

//                // Проверка обязательных полей
//                if (string.IsNullOrEmpty(oldPass) ||
//                    string.IsNullOrEmpty(newPass) ||
//                    string.IsNullOrEmpty(confirmPass))
//                {
//                    MessageBox.Show("Все поля обязательны для заполнения!");
//                    return;
//                }

//                // Проверка совпадения новых паролей
//                if (newPass != confirmPass)
//                {
//                    MessageBox.Show("Новые пароли не совпадают!");
//                    return;
//                }

//                // Подключение к БД
//                string connString = "Host=localhost;Username=postgres;Password=root;Database=postgres";

//                using (var conn = new NpgsqlConnection(connString))
//                {
//                    conn.Open();

//                    // Проверка старого пароля
//                    var checkCmd = new NpgsqlCommand(
//                        "SELECT password FROM \"Users\" WHERE id = @id", conn);
//                    checkCmd.Parameters.AddWithValue("id", userId);

//                    string dbPassword = checkCmd.ExecuteScalar()?.ToString();

//                    if (dbPassword != oldPass)
//                    {
//                        MessageBox.Show("Старый пароль неверен!");
//                        return;
//                    }

//                    // Обновление пароля
//                    var updateCmd = new NpgsqlCommand(
//                        "UPDATE \"Users\" SET password = @newPass WHERE id = @id", conn);
//                    updateCmd.Parameters.AddWithValue("newPass", newPass);
//                    updateCmd.Parameters.AddWithValue("id", userId);

//                    int rowsAffected = updateCmd.ExecuteNonQuery();

//                    if (rowsAffected > 0)
//                    {
//                        MessageBox.Show("Пароль успешно изменен!");

//                        // Возвращаемся на главный экран авторизации
//                        new MainWindow().Show();
//                        this.Close();
//                    }
//                    else
//                    {
//                        MessageBox.Show("Ошибка при изменении пароля");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Ошибка: {ex.Message}");
//            }
//        }
//    }
//}

//UpdatePass.xaml :
//< Window x: Class = "demo.UpdatePass"
//        xmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation"
//        xmlns: x = "http://schemas.microsoft.com/winfx/2006/xaml"
//        xmlns: d = "http://schemas.microsoft.com/expression/blend/2008"
//        xmlns: mc = "http://schemas.openxmlformats.org/markup-compatibility/2006"
//        xmlns: local = "clr-namespace:demo"
//        mc: Ignorable = "d"
//        Title = "UpdatePass" Height = "450" Width = "800" >
//    < Grid >

//        < Label  x: Name = "Label1" Content = "Вы успешно авторизировались как клиент. Пожалуйста, измените 
//пароль" HorizontalAlignment="Center" Height="40" Margin="100,16,0,0" 
//VerticalAlignment="Top"></Label>
//        <Label  x:Name = "Label2" Content = "Старый пароль" HorizontalAlignment = "Left" Height = "40"
//Margin = "148,46,0,0" VerticalAlignment = "Top" ></ Label >
//        < Label  x: Name = "Label3" Content = "Новый пароль" HorizontalAlignment = "Left" Height = "43"
//Margin = "148,150,0,0" VerticalAlignment = "Top" >
//        </ Label >
//        < Label  x: Name = "Label4" Content = "Подтвердите пароль" HorizontalAlignment = "Left"
//Height = "40" Margin = "148,250,0,0" VerticalAlignment = "Top" ></ Label >
//        < TextBox x: Name = "TextBox3" HorizontalAlignment = "Left" Height = "33" Width = "250"
//Margin = "148,86,0,0" TextWrapping = "Wrap" VerticalAlignment = "Top" ></ TextBox >
//        < TextBox x: Name = "TextBox4" HorizontalAlignment = "Left" Height = "33" Width = "250"
//Margin = "148,190,0,0" TextWrapping = "Wrap" VerticalAlignment = "Top" ></ TextBox >
//        < TextBox x: Name = "TextBox5" HorizontalAlignment = "Left" Height = "33" Width = "250"
//Margin = "148,290,0,0" TextWrapping = "Wrap" VerticalAlignment = "Top" ></ TextBox >
//        < Button x: Name = "button2" Content = "Изменить пароль" HorizontalAlignment = "Left"
//Height = "45" Width = "200"  Margin = "300,350,0,0" VerticalAlignment = "Top" Click = "Button_Click" >
//        </ Button >

//    </ Grid >
//</ Window >






//create table "Users" (
//Id serial primary key,
//Surname varchar(100) not null,
//Name varchar(100) not null,
//Othcestvo varchar(100) not null,
//Role varchar(100) not null,
//Login varchar(100) not null,
//password varchar(100) not null,
//Count int not null,
//Active bit not null,
//Date date not null
//);


//create table "Role" (
//RoleId serial primary key,
//Role varchar(100) not null
//);
