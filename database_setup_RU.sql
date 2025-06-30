-- Скрипт настройки базы данных для системы управления пользователями
-- Версия: 1.0
-- База данных: PostgreSQL

-- ============================================================================
-- СОЗДАНИЕ БАЗЫ ДАННЫХ
-- ============================================================================

-- Создание базы данных (выполняется от имени superuser)
-- CREATE DATABASE user_management 
--     WITH ENCODING = 'UTF8'
--     LC_COLLATE = 'ru_RU.UTF-8'
--     LC_CTYPE = 'ru_RU.UTF-8';

-- Подключение к созданной базе данных
-- \c user_management;

-- ============================================================================
-- СОЗДАНИЕ ТАБЛИЦ
-- ============================================================================

-- Удаление таблицы если существует (для пересоздания)
DROP TABLE IF EXISTS "Users" CASCADE;

-- Создание основной таблицы пользователей
CREATE TABLE "Users" (
    -- Уникальный идентификатор пользователя (автоинкремент)
    id SERIAL PRIMARY KEY,
    
    -- Личные данные пользователя
    surname VARCHAR(100) NOT NULL COMMENT 'Фамилия пользователя',
    name VARCHAR(100) NOT NULL COMMENT 'Имя пользователя', 
    othcestvo VARCHAR(100) COMMENT 'Отчество пользователя (необязательно)',
    
    -- Данные для авторизации
    login VARCHAR(50) UNIQUE NOT NULL COMMENT 'Уникальный логин пользователя',
    password VARCHAR(100) NOT NULL COMMENT 'Пароль пользователя (в открытом виде)',
    
    -- Роль и права доступа
    role INTEGER NOT NULL CHECK (role IN (1,2,3)) COMMENT 'Роль: 1-руководитель, 2-администратор, 3-клиент',
    
    -- Данные безопасности
    count INTEGER DEFAULT 0 CHECK (count >= 0 AND count <= 3) COMMENT 'Количество неудачных попыток входа',
    active BIT(1) DEFAULT B'0' COMMENT 'Статус активности: 0-активен, 1-заблокирован',
    
    -- Временные метки
    date TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT 'Дата последнего входа в систему',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT 'Дата создания записи',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT 'Дата последнего обновления записи'
);

-- ============================================================================
-- СОЗДАНИЕ ИНДЕКСОВ
-- ============================================================================

-- Уникальный индекс для логина (обеспечивает быстрый поиск при авторизации)
CREATE UNIQUE INDEX idx_users_login ON "Users"(login);

-- Индекс для роли (для быстрой фильтрации по ролям)
CREATE INDEX idx_users_role ON "Users"(role);

-- Индекс для статуса активности (для поиска заблокированных пользователей)
CREATE INDEX idx_users_active ON "Users"(active);

-- Индекс для даты последнего входа (для поиска неактивных аккаунтов)
CREATE INDEX idx_users_date ON "Users"(date);

-- Составной индекс для часто используемых запросов
CREATE INDEX idx_users_login_active ON "Users"(login, active);

-- ============================================================================
-- СОЗДАНИЕ ФУНКЦИЙ И ПРОЦЕДУР
-- ============================================================================

-- Функция для блокировки пользователя
CREATE OR REPLACE FUNCTION block_user(user_id INTEGER)
RETURNS VOID AS $$
BEGIN
    UPDATE "Users" 
    SET active = B'1', 
        count = 3, 
        updated_at = CURRENT_TIMESTAMP
    WHERE id = user_id;
    
    -- Логирование действия
    RAISE NOTICE 'Пользователь с ID % заблокирован', user_id;
END;
$$ LANGUAGE plpgsql;

-- Функция для сброса счетчика неудачных попыток
CREATE OR REPLACE FUNCTION reset_counter(user_id INTEGER)
RETURNS VOID AS $$
BEGIN
    UPDATE "Users" 
    SET count = 0, 
        date = CURRENT_TIMESTAMP,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = user_id;
    
    -- Логирование действия
    RAISE NOTICE 'Счетчик попыток сброшен для пользователя с ID %', user_id;
END;
$$ LANGUAGE plpgsql;

-- Функция для разблокировки пользователя (для администраторов)
CREATE OR REPLACE FUNCTION unblock_user(user_id INTEGER)
RETURNS VOID AS $$
BEGIN
    UPDATE "Users" 
    SET active = B'0', 
        count = 0,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = user_id;
    
    -- Логирование действия
    RAISE NOTICE 'Пользователь с ID % разблокирован', user_id;
END;
$$ LANGUAGE plpgsql;

-- Функция для автоматической блокировки неактивных пользователей
CREATE OR REPLACE FUNCTION block_inactive_users()
RETURNS INTEGER AS $$
DECLARE
    blocked_count INTEGER := 0;
BEGIN
    UPDATE "Users" 
    SET active = B'1',
        updated_at = CURRENT_TIMESTAMP
    WHERE date < (CURRENT_TIMESTAMP - INTERVAL '1 month') 
      AND active = B'0';
    
    GET DIAGNOSTICS blocked_count = ROW_COUNT;
    
    RAISE NOTICE 'Заблокировано % неактивных пользователей', blocked_count;
    RETURN blocked_count;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- СОЗДАНИЕ ТРИГГЕРОВ
-- ============================================================================

-- Функция триггера для обновления поля updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Триггер для автоматического обновления времени изменения
CREATE TRIGGER tr_users_updated_at
    BEFORE UPDATE ON "Users"
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- ============================================================================
-- СОЗДАНИЕ ПРЕДСТАВЛЕНИЙ (VIEWS)
-- ============================================================================

-- Представление для безопасного просмотра пользователей (без паролей)
CREATE OR REPLACE VIEW v_users_safe AS
SELECT 
    id,
    surname,
    name,
    othcestvo,
    CASE role
        WHEN 1 THEN 'Руководитель'
        WHEN 2 THEN 'Администратор'
        WHEN 3 THEN 'Клиент'
        ELSE 'Неизвестная роль'
    END as role_name,
    role,
    login,
    count,
    CASE 
        WHEN active::integer = 0 THEN 'Активен'
        ELSE 'Заблокирован'
    END as status,
    date as last_login,
    created_at,
    updated_at
FROM "Users";

-- Представление активных пользователей
CREATE OR REPLACE VIEW v_active_users AS
SELECT * FROM v_users_safe 
WHERE status = 'Активен';

-- Представление заблокированных пользователей
CREATE OR REPLACE VIEW v_blocked_users AS
SELECT * FROM v_users_safe 
WHERE status = 'Заблокирован';

-- ============================================================================
-- ВСТАВКА ТЕСТОВЫХ ДАННЫХ
-- ============================================================================

-- Очистка таблицы перед вставкой тестовых данных
TRUNCATE TABLE "Users" RESTART IDENTITY CASCADE;

-- Вставка тестовых пользователей
INSERT INTO "Users" (surname, name, othcestvo, role, login, password, count, active, date) VALUES 
    -- Руководитель
    ('Иванов', 'Иван', 'Иванович', 1, 'director', 'password123', 0, B'0', CURRENT_TIMESTAMP),
    
    -- Администратор
    ('Петров', 'Петр', 'Петрович', 2, 'admin', 'admin123', 0, B'0', CURRENT_TIMESTAMP),
    
    -- Клиенты
    ('Сидоров', 'Сидор', 'Сидорович', 3, 'client1', 'client123', 0, B'0', CURRENT_TIMESTAMP),
    ('Козлова', 'Анна', 'Михайловна', 3, 'client2', 'password456', 0, B'0', CURRENT_TIMESTAMP),
    
    -- Заблокированный пользователь (для тестирования)
    ('Федоров', 'Федор', 'Федорович', 3, 'blocked_user', 'password789', 3, B'1', CURRENT_TIMESTAMP - INTERVAL '2 months');

-- ============================================================================
-- СОЗДАНИЕ ПОЛЬЗОВАТЕЛЕЙ БАЗЫ ДАННЫХ
-- ============================================================================

-- Создание пользователя приложения (с ограниченными правами)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'app_user') THEN
        CREATE USER app_user WITH PASSWORD 'secure_app_password_2024';
    END IF;
END
$$;

-- Предоставление необходимых прав пользователю приложения
GRANT CONNECT ON DATABASE postgres TO app_user;
GRANT USAGE ON SCHEMA public TO app_user;
GRANT SELECT, INSERT, UPDATE ON TABLE "Users" TO app_user;
GRANT USAGE, SELECT ON SEQUENCE "Users_id_seq" TO app_user;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO app_user;

-- Создание пользователя только для чтения (для отчетов)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'readonly_user') THEN
        CREATE USER readonly_user WITH PASSWORD 'readonly_password_2024';
    END IF;
END
$$;

-- Предоставление прав только на чтение
GRANT CONNECT ON DATABASE postgres TO readonly_user;
GRANT USAGE ON SCHEMA public TO readonly_user;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO readonly_user;
GRANT SELECT ON ALL SEQUENCES IN SCHEMA public TO readonly_user;

-- ============================================================================
-- НАСТРОЙКА АВТОМАТИЧЕСКИХ ЗАДАЧ
-- ============================================================================

-- Создание функции для ежедневной очистки (можно настроить через cron)
CREATE OR REPLACE FUNCTION daily_maintenance()
RETURNS VOID AS $$
BEGIN
    -- Блокировка неактивных пользователей
    PERFORM block_inactive_users();
    
    -- Обновление статистики таблиц
    ANALYZE "Users";
    
    -- Логирование выполнения
    RAISE NOTICE 'Ежедневное обслуживание выполнено в %', CURRENT_TIMESTAMP;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- СОЗДАНИЕ РЕЗЕРВНЫХ КОПИЙ (ПРИМЕР КОМАНД)
-- ============================================================================

-- Команды для создания резервных копий (выполняются через командную строку):
--
-- Полная резервная копия:
-- pg_dump -h localhost -U postgres -d user_management > backup_full_$(date +%Y%m%d).sql
--
-- Резервная копия только данных:
-- pg_dump -h localhost -U postgres -d user_management --data-only > backup_data_$(date +%Y%m%d).sql
--
-- Резервная копия только структуры:
-- pg_dump -h localhost -U postgres -d user_management --schema-only > backup_schema_$(date +%Y%m%d).sql

-- ============================================================================
-- ЗАПРОСЫ ДЛЯ МОНИТОРИНГА
-- ============================================================================

-- Проверка количества пользователей по ролям
-- SELECT 
--     CASE role
--         WHEN 1 THEN 'Руководители'
--         WHEN 2 THEN 'Администраторы' 
--         WHEN 3 THEN 'Клиенты'
--     END as role_name,
--     COUNT(*) as user_count
-- FROM "Users" 
-- GROUP BY role 
-- ORDER BY role;

-- Проверка заблокированных пользователей
-- SELECT login, surname, name, count, date 
-- FROM "Users" 
-- WHERE active = B'1'
-- ORDER BY date DESC;

-- Проверка неактивных пользователей (не входили более месяца)
-- SELECT login, surname, name, date,
--        EXTRACT(DAY FROM (CURRENT_TIMESTAMP - date)) as days_inactive
-- FROM "Users" 
-- WHERE date < (CURRENT_TIMESTAMP - INTERVAL '1 month')
--   AND active = B'0'
-- ORDER BY date;

-- ============================================================================
-- ЗАВЕРШЕНИЕ УСТАНОВКИ
-- ============================================================================

-- Вывод информации об успешной установке
DO $$
BEGIN
    RAISE NOTICE '============================================';
    RAISE NOTICE 'База данных успешно настроена!';
    RAISE NOTICE 'Создано пользователей: %', (SELECT COUNT(*) FROM "Users");
    RAISE NOTICE 'Системные пользователи БД: app_user, readonly_user';
    RAISE NOTICE 'Представления: v_users_safe, v_active_users, v_blocked_users';
    RAISE NOTICE 'Функции: block_user(), reset_counter(), unblock_user()';
    RAISE NOTICE '============================================';
END
$$;