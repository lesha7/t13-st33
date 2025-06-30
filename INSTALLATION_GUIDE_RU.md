# Инструкция по установке

## Системные требования

### Операционная система
- Windows 10 (версия 1809 или новее)
- Windows 11 (любая версия)
- Windows Server 2016/2019/2022

### Программное обеспечение
- .NET Framework 4.7.2 или выше
- PostgreSQL 12.0 или выше
- Visual Studio 2019/2022 (для разработки, необязательно для запуска)

### Аппаратные требования
- **Процессор**: Intel Core i3 или AMD эквивалент
- **Оперативная память**: Минимум 4 GB, рекомендуется 8 GB
- **Свободное место на диске**: 500 MB для приложения + 1 GB для PostgreSQL
- **Экран**: Разрешение минимум 1024x768

## Пошаговая установка

### Шаг 1: Установка PostgreSQL

#### 1.1 Загрузка PostgreSQL
1. Перейдите на официальный сайт PostgreSQL: https://www.postgresql.org/download/windows/
2. Загрузите последнюю стабильную версию PostgreSQL для Windows
3. Запустите загруженный установочный файл

#### 1.2 Установка PostgreSQL
1. Запустите установщик от имени администратора
2. Выберите компоненты для установки:
   - ✅ PostgreSQL Server
   - ✅ pgAdmin 4
   - ✅ Stack Builder
   - ✅ Command Line Tools

3. Выберите директорию установки (по умолчанию `C:\Program Files\PostgreSQL\15\`)
4. Выберите директорию для данных (по умолчанию `C:\Program Files\PostgreSQL\15\data`)
5. **ВАЖНО**: Установите пароль для пользователя `postgres` (запомните его!)
6. Оставьте порт по умолчанию: `5432`
7. Выберите локаль: `Russian, Russia` или `Default locale`
8. Завершите установку

#### 1.3 Проверка установки PostgreSQL
1. Откройте командную строку (Win + R → cmd)
2. Выполните команду:
```bash
psql --version
```
3. Если команда не найдена, добавьте PostgreSQL в PATH:
   - Откройте "Система" → "Дополнительные параметры системы"
   - Нажмите "Переменные среды"
   - В системных переменных найдите PATH
   - Добавьте путь: `C:\Program Files\PostgreSQL\15\bin`

### Шаг 2: Настройка базы данных

#### 2.1 Подключение к PostgreSQL
1. Откройте pgAdmin 4 или командную строку
2. Для командной строки выполните:
```bash
psql -U postgres -h localhost
```
3. Введите пароль, установленный при установке PostgreSQL

#### 2.2 Создание базы данных
Выполните следующие команды в psql или pgAdmin:

```sql
-- Создание базы данных
CREATE DATABASE user_management 
    WITH ENCODING = 'UTF8'
    LC_COLLATE = 'Russian_Russia.1251'
    LC_CTYPE = 'Russian_Russia.1251';

-- Подключение к созданной базе данных
\c user_management;
```

#### 2.3 Выполнение скрипта настройки
1. Скопируйте содержимое файла `database_setup_RU.sql`
2. Выполните его в pgAdmin или через psql:
```bash
psql -U postgres -d user_management -f database_setup_RU.sql
```

### Шаг 3: Установка .NET Framework

#### 3.1 Проверка текущей версии
1. Откройте командную строку
2. Выполните команду:
```bash
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\" /v Release
```
3. Если значение Release >= 461808, то .NET Framework 4.7.2 уже установлен

#### 3.2 Установка .NET Framework (если требуется)
1. Перейдите на https://dotnet.microsoft.com/download/dotnet-framework
2. Скачайте .NET Framework 4.7.2 или новее
3. Запустите установщик от имени администратора
4. Следуйте инструкциям установщика
5. Перезагрузите компьютер после установки

### Шаг 4: Подготовка приложения

#### 4.1 Компиляция из исходного кода
Если у вас есть исходный код:

1. Откройте Visual Studio 2019/2022
2. Откройте файл `demo2.sln`
3. В Solution Explorer щелкните правой кнопкой на проекте
4. Выберите "Restore NuGet Packages"
5. Дождитесь загрузки пакета Npgsql
6. Постройте решение: Build → Build Solution (Ctrl+Shift+B)

#### 4.2 Установка готового приложения
Если у вас есть готовый exe файл:

1. Создайте папку для приложения (например, `C:\UserManagement\`)
2. Скопируйте все файлы приложения в эту папку:
   - `demo2.exe`
   - `Npgsql.dll`
   - `demo2.exe.config`
   - Другие зависимые библиотеки

### Шаг 5: Настройка подключения к базе данных

#### 5.1 Проверка строки подключения
В файле `MainWindow.xaml.cs` и других файлах найдите строку подключения:
```csharp
string connString = "Host=localhost;Username=postgres;Password=root;Database=user_management";
```

#### 5.2 Изменение параметров подключения
Измените параметры в соответствии с вашей конфигурацией:
- **Host**: `localhost` (если база данных на том же компьютере)
- **Username**: `postgres` (или созданный пользователь)
- **Password**: пароль, установленный при установке PostgreSQL
- **Database**: `user_management` (название созданной базы данных)

#### 5.3 Использование файла конфигурации (рекомендуется)
Создайте файл `app.config` в папке приложения:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <connectionStrings>
        <add name="DefaultConnection" 
             connectionString="Host=localhost;Username=postgres;Password=your_password;Database=user_management" 
             providerName="Npgsql" />
    </connectionStrings>
</configuration>
```

### Шаг 6: Тестирование установки

#### 6.1 Первый запуск
1. Запустите файл `demo2.exe`
2. Если приложение не запускается, проверьте:
   - Установлен ли .NET Framework 4.7.2+
   - Запущен ли PostgreSQL сервер
   - Правильность строки подключения

#### 6.2 Тестирование входа
Используйте тестовые учетные записи:

**Руководитель:**
- Логин: `director`
- Пароль: `password123`

**Администратор:**
- Логин: `admin`
- Пароль: `admin123`

**Клиент:**
- Логин: `client1`
- Пароль: `client123`

#### 6.3 Проверка функциональности
1. **Тест аутентификации**: войдите под разными учетными записями
2. **Тест ролей**: проверьте доступ к разным окнам
3. **Тест администратора**: добавьте нового пользователя
4. **Тест клиента**: смените пароль

## Устранение неполадок

### Ошибка подключения к базе данных

#### Проблема: "Npgsql.NpgsqlException: Failed to connect to..."

**Возможные причины и решения:**

1. **PostgreSQL не запущен**
   ```bash
   # Проверка статуса службы
   sc query postgresql-x64-15
   
   # Запуск службы
   net start postgresql-x64-15
   ```

2. **Неправильные параметры подключения**
   - Проверьте Host, Port, Database name
   - Убедитесь, что пароль правильный

3. **Брандмауэр блокирует соединение**
   - Добавьте исключение для порта 5432
   - Или временно отключите брандмауэр для теста

### Ошибка при запуске приложения

#### Проблема: "System.IO.FileNotFoundException: Could not load file or assembly"

**Решение:**
1. Установите Microsoft Visual C++ Redistributable
2. Убедитесь, что все DLL файлы находятся в папке приложения
3. Переустановите .NET Framework

#### Проблема: Приложение не запускается совсем

**Решение:**
1. Запустите командную строку от имени администратора
2. Перейдите в папку приложения
3. Запустите: `demo2.exe`
4. Посмотрите текст ошибки в консоли

### Проблемы с правами доступа

#### Проблема: "Permission denied" при создании пользователей

**Решение:**
1. Убедитесь, что пользователь `postgres` имеет права на создание пользователей
2. Проверьте права пользователя `app_user` в базе данных
3. При необходимости предоставьте дополнительные права:
```sql
GRANT ALL PRIVILEGES ON DATABASE user_management TO app_user;
```

## Настройка для продуктивного использования

### Безопасность

#### 1. Изменение паролей по умолчанию
```sql
-- Смена пароля пользователя postgres
ALTER USER postgres PASSWORD 'new_secure_password';

-- Смена пароля пользователя приложения
ALTER USER app_user PASSWORD 'new_app_password';
```

#### 2. Настройка шифрования паролей
Рекомендуется добавить хеширование паролей в приложении:
```csharp
using System.Security.Cryptography;
using System.Text;

public static string HashPassword(string password)
{
    using (SHA256 sha256Hash = SHA256.Create())
    {
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
```

#### 3. Настройка SSL соединения
В строке подключения добавьте SSL параметры:
```csharp
string connString = "Host=localhost;Username=app_user;Password=password;Database=user_management;SSL Mode=Require;";
```

### Мониторинг и обслуживание

#### 1. Настройка автоматических резервных копий
Создайте bat-файл для автоматического бэкапа:
```batch
@echo off
set PGPASSWORD=your_password
set BACKUP_DIR=C:\Backups
set DATE=%date:~-4,4%%date:~-10,2%%date:~-7,2%

pg_dump -h localhost -U postgres -d user_management > %BACKUP_DIR%\backup_%DATE%.sql

echo Backup completed: %BACKUP_DIR%\backup_%DATE%.sql
```

#### 2. Настройка логирования
Добавьте в `postgresql.conf`:
```
log_statement = 'all'
log_destination = 'stderr'
logging_collector = on
log_directory = 'pg_log'
log_filename = 'postgresql-%Y-%m-%d_%H%M%S.log'
```

#### 3. Мониторинг производительности
Регулярно выполняйте:
```sql
-- Проверка размера базы данных
SELECT pg_size_pretty(pg_database_size('user_management'));

-- Проверка активных соединений
SELECT COUNT(*) FROM pg_stat_activity WHERE state = 'active';

-- Статистика по таблице Users
SELECT * FROM pg_stat_user_tables WHERE relname = 'Users';
```

## Обновление системы

### Обновление приложения
1. Остановите работающее приложение
2. Создайте резервную копию базы данных
3. Замените исполняемые файлы новыми версиями
4. Выполните необходимые миграции базы данных
5. Протестируйте обновленное приложение

### Обновление PostgreSQL
1. Создайте полную резервную копию данных
2. Установите новую версию PostgreSQL
3. Мигрируйте данные из старой версии
4. Обновите строки подключения при необходимости
5. Протестируйте работу приложения

## Техническая поддержка

При возникновении проблем соберите следующую информацию:

### Информация о системе:
- Версия Windows
- Версия .NET Framework
- Версия PostgreSQL
- Версия приложения

### Логи и ошибки:
- Текст сообщений об ошибках
- Логи PostgreSQL (в папке `pg_log`)
- Логи Windows Event Viewer
- Последние действия перед возникновением ошибки

### Конфигурация:
- Строка подключения к БД
- Размер базы данных
- Количество пользователей в системе
- Настройки безопасности