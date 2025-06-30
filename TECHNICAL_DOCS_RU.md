# Техническая документация

## Архитектура приложения

### Общая структура
Приложение построено по архитектуре Model-View-ViewModel (MVVM) с использованием WPF и PostgreSQL.

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│     View        │    │   ViewModel     │    │     Model       │
│   (XAML)        │◄──►│  (Code-behind)  │◄──►│  (Database)     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Компоненты системы

#### 1. Слой представления (View)
- **MainWindow.xaml** - Форма аутентификации
- **Window1.xaml** - Интерфейс руководителя  
- **Window2.xaml** - Панель администратора
- **Window3.xaml** - Дополнительное окно
- **UpdatePass.xaml** - Форма смены пароля

#### 2. Слой логики (ViewModel)
- **MainWindow.xaml.cs** - Обработка аутентификации
- **Window2.xaml.cs** - Управление пользователями
- **UpdatePass.xaml.cs** - Логика смены паролей

#### 3. Слой данных (Model)
- **PostgreSQL база данных** - Хранение пользователей
- **User класс** - Модель пользователя

## Детальное описание компонентов

### MainWindow (Главное окно)

#### Функциональность:
```csharp
private void Button1_Click(object sender, RoutedEventArgs e)
```

**Алгоритм аутентификации:**
1. Валидация входных данных
2. Подключение к базе данных
3. Проверка существования пользователя
4. Проверка статуса блокировки
5. Валидация пароля
6. Обновление счетчика попыток
7. Проверка активности аккаунта
8. Сброс счетчика и авторизация
9. Перенаправление по ролям

**Параметры безопасности:**
- Максимум 3 попытки входа
- Блокировка на 1 месяц неактивности
- Автоматический сброс счетчика при успешном входе

### Window2 (Панель администратора)

#### Основные методы:

##### InitializeRoles()
```csharp
private void InitializeRoles()
{
    var roles = new Dictionary<string, string>
    {
        { "1", "Руководитель (1)" },
        { "2", "Администратор (2)" },
        { "3", "Клиент (3)" }
    };
}
```

##### LoadUsers()
```csharp
private void LoadUsers()
```
- Загружает всех пользователей из БД
- Обновляет ObservableCollection
- Привязывает данные к DataGrid

##### button3_Click() - Добавление пользователя
```csharp
private void button3_Click(object sender, RoutedEventArgs e)
```
- Валидация обязательных полей
- Вставка нового пользователя в БД
- Обновление списка пользователей

##### button4_Click() - Сохранение изменений
```csharp
private void button4_Click(object sender, RoutedEventArgs e)
```
- Пакетное обновление всех пользователей
- Транзакционная обработка

### UpdatePass (Смена пароля)

#### Алгоритм смены пароля:
1. Валидация всех полей
2. Проверка совпадения новых паролей
3. Верификация старого пароля
4. Обновление пароля в БД
5. Возврат к главному окну

## База данных

### Схема таблицы Users

```sql
CREATE TABLE "Users" (
    id SERIAL PRIMARY KEY,
    surname VARCHAR(100) NOT NULL,
    name VARCHAR(100) NOT NULL,
    othcestvo VARCHAR(100),
    role INTEGER NOT NULL CHECK (role IN (1,2,3)),
    login VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(100) NOT NULL,
    count INTEGER DEFAULT 0 CHECK (count >= 0 AND count <= 3),
    active BIT(1) DEFAULT B'0',
    date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Индексы для оптимизации:
```sql
CREATE UNIQUE INDEX idx_users_login ON "Users"(login);
CREATE INDEX idx_users_role ON "Users"(role);
CREATE INDEX idx_users_active ON "Users"(active);
CREATE INDEX idx_users_date ON "Users"(date);
```

### Хранимые процедуры

#### Процедура блокировки пользователя:
```sql
CREATE OR REPLACE FUNCTION block_user(user_id INTEGER)
RETURNS VOID AS $$
BEGIN
    UPDATE "Users" 
    SET active = B'1', count = 3 
    WHERE id = user_id;
END;
$$ LANGUAGE plpgsql;
```

#### Процедура сброса счетчика:
```sql
CREATE OR REPLACE FUNCTION reset_counter(user_id INTEGER)
RETURNS VOID AS $$
BEGIN
    UPDATE "Users" 
    SET count = 0, date = CURRENT_TIMESTAMP 
    WHERE id = user_id;
END;
$$ LANGUAGE plpgsql;
```

## Обработка ошибок

### Типы исключений:

#### NpgsqlException
```csharp
catch (NpgsqlException ex)
{
    MessageBox.Show($"Ошибка БД: {ex.Message}",
                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
}
```

#### Общие исключения:
```csharp
catch (Exception ex)
{
    MessageBox.Show($"Ошибка: {ex.Message}",
                   "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
}
```

### Логирование ошибок
Рекомендуется добавить систему логирования:

```csharp
public static class Logger
{
    public static void LogError(string message, Exception ex)
    {
        string logMessage = $"[{DateTime.Now}] ERROR: {message}\n{ex}\n";
        File.AppendAllText("error.log", logMessage);
    }
}
```

## Конфигурация

### Строка подключения
```csharp
private readonly string _connString = 
    "Host=localhost;Username=postgres;Password=root;Database=postgres";
```

### Параметры безопасности
```csharp
// Максимальное количество попыток входа
private const int MAX_LOGIN_ATTEMPTS = 3;

// Период блокировки аккаунта (месяцы)
private const int ACCOUNT_LOCK_MONTHS = 1;
```

## Развертывание

### Требования к серверу:
- Windows Server 2016+ или Windows 10+
- .NET Framework 4.7.2+
- PostgreSQL 12+
- Минимум 2GB RAM
- 100MB свободного места на диске

### Настройка PostgreSQL:
1. Создание базы данных:
```sql
CREATE DATABASE user_management;
```

2. Создание пользователя:
```sql
CREATE USER app_user WITH PASSWORD 'secure_password';
GRANT ALL PRIVILEGES ON DATABASE user_management TO app_user;
```

3. Выполнение миграций:
```sql
\i create_tables.sql
\i insert_initial_data.sql
```

## Мониторинг и обслуживание

### Метрики для мониторинга:
- Количество попыток входа в систему
- Количество заблокированных аккаунтов
- Время отклика базы данных
- Использование памяти приложением

### Регулярное обслуживание:
- Очистка логов старше 30 дней
- Резервное копирование базы данных
- Обновление статистики PostgreSQL
- Проверка индексов на фрагментацию

## API для расширения

### Интерфейс для работы с пользователями:
```csharp
public interface IUserService
{
    Task<User> AuthenticateAsync(string login, string password);
    Task<bool> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<List<User>> GetAllUsersAsync();
    Task<bool> ChangePasswordAsync(int userId, string newPassword);
}
```

### Пример реализации:
```csharp
public class UserService : IUserService
{
    private readonly string _connectionString;
    
    public UserService(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    // Реализация методов...
}
```