# User Management System - API Documentation

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Database Schema](#database-schema)
4. [Application Components](#application-components)
5. [Public APIs and Functions](#public-apis-and-functions)
6. [User Interfaces](#user-interfaces)
7. [Configuration](#configuration)
8. [Usage Examples](#usage-examples)
9. [Error Handling](#error-handling)
10. [Security Features](#security-features)

## Overview

The User Management System is a WPF (.NET 8.0) desktop application that provides secure user authentication and management capabilities. The system supports role-based access control with three user types: Manager (Руководитель), Administrator (Администратор), and Client (Клиент).

### Key Features
- Secure user authentication with login attempts tracking
- Role-based access control
- User account management (add, edit, view users)
- Password change functionality for clients
- Account lockout after failed login attempts
- Automatic account deactivation for inactive users
- PostgreSQL database integration

### Technology Stack
- **Framework**: .NET 8.0 WPF
- **Database**: PostgreSQL
- **ORM**: Npgsql (PostgreSQL .NET connector)
- **UI Framework**: WPF with XAML

## System Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Presentation  │    │   Business      │    │   Data Access   │
│     Layer       │    │     Logic       │    │     Layer       │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ MainWindow      │    │ Authentication  │    │ PostgreSQL      │
│ Window1         │    │ User Management │    │ Database        │
│ Window2         │    │ Password Mgmt   │    │                 │
│ Window3         │    │ Role Management │    │                 │
│ UpdatePass      │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## Database Schema

### Users Table
```sql
CREATE TABLE "Users" (
    Id SERIAL PRIMARY KEY,
    Surname VARCHAR(100) NOT NULL,
    Name VARCHAR(100) NOT NULL,
    Othcestvo VARCHAR(100) NOT NULL,
    Role VARCHAR(100) NOT NULL,
    Login VARCHAR(100) NOT NULL,
    Password VARCHAR(100) NOT NULL,
    Count INT NOT NULL,
    Active BIT NOT NULL,
    Date DATE NOT NULL
);
```

#### Field Descriptions
- **Id**: Unique user identifier (auto-increment)
- **Surname**: User's last name
- **Name**: User's first name  
- **Othcestvo**: User's middle name/patronymic
- **Role**: User role (1=Manager, 2=Administrator, 3=Client)
- **Login**: Unique login username
- **Password**: User password (stored as plain text)
- **Count**: Failed login attempts counter
- **Active**: Account status (0=Active, 1=Blocked)
- **Date**: Last login date

### Database Connection
```
Host: localhost
Username: postgres
Password: root
Database: postgres
```

## Application Components

### Namespace Structure
- **Main Namespace**: `demo`
- **Secondary Namespace**: `demo2` (for Window3)

### Core Classes

#### 1. App Class
```csharp
namespace demo
{
    public partial class App : Application
    {
        // Application entry point
    }
}
```

**Purpose**: Main application class that inherits from WPF Application.

## Public APIs and Functions

### MainWindow Class

#### Constructor
```csharp
public MainWindow()
```
**Description**: Initializes the main login window.
**Usage**: Entry point for user authentication.

#### Button1_Click Method
```csharp
private void Button1_Click(object sender, RoutedEventArgs e)
```
**Description**: Handles user login authentication with comprehensive security checks.

**Authentication Flow**:
1. Validates input fields
2. Checks user existence in database
3. Verifies account is not blocked
4. Validates password
5. Checks account activity (blocks if inactive > 1 month)
6. Updates login attempt counter
7. Redirects to appropriate interface based on role

**Security Features**:
- Account lockout after 3 failed attempts
- Automatic blocking for accounts inactive > 1 month
- Failed login attempt tracking
- Role-based redirection

**Example Usage**:
```csharp
// User enters credentials in TextBox1 (login) and TextBox2 (password)
// Clicks the login button to trigger authentication
```

### Window1 Class (Manager Interface)

#### Constructor
```csharp
public Window1()
```
**Description**: Initializes the manager interface window.
**Access Level**: Role 1 (Manager/Руководитель)
**Features**: Basic welcome interface for managers.

### Window2 Class (Administrator Interface)

#### Constructor
```csharp
public Window2()
```
**Description**: Initializes the administrator interface with user management capabilities.
**Access Level**: Role 2 (Administrator/Администратор)

#### InitializeRoles Method
```csharp
private void InitializeRoles()
```
**Description**: Populates the role dropdown with available user roles.
**Roles Available**:
- Role 1: Руководитель (Manager)
- Role 2: Администратор (Administrator)  
- Role 3: Клиент (Client)

#### LoadUsers Method
```csharp
private void LoadUsers()
```
**Description**: Retrieves and displays all users from the database in a DataGrid.
**Returns**: Populates `_userList` ObservableCollection with User objects.

**User Properties**:
```csharp
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
```

#### button3_Click Method (Add User)
```csharp
private void button3_Click(object sender, RoutedEventArgs e)
```
**Description**: Adds a new user to the system.
**Required Fields**:
- Login (TextBox1)
- Password (TextBox2)
- Surname (TextBox3)
- Name (TextBox4)
- Othcestvo (TextBox5) - Optional
- Role (typeRole ComboBox)

**Default Values**:
- Count: 0
- Active: 0 (Active)
- Date: Current DateTime

**Example Usage**:
```csharp
// Fill required fields:
TextBox1.Text = "newuser";      // Login
TextBox2.Text = "password123";  // Password
TextBox3.Text = "Иванов";       // Surname
TextBox4.Text = "Иван";         // Name
TextBox5.Text = "Иванович";     // Othcestvo
typeRole.SelectedValue = "3";   // Client role
// Click Add button
```

#### button4_Click Method (Save Changes)
```csharp
private void button4_Click(object sender, RoutedEventArgs e)
```
**Description**: Saves all modifications made to users in the DataGrid to the database.
**Functionality**: Bulk update operation for all users in the collection.

### Window3 Class
```csharp
namespace demo2
{
    public partial class Window3 : Window
    {
        public Window3()
    }
}
```
**Description**: Empty window class with basic initialization.
**Note**: This window currently contains only commented code and no active functionality.

### UpdatePass Class (Password Update)

#### Constructor
```csharp
public UpdatePass(int userId)
```
**Description**: Initializes password update window for a specific user.
**Parameters**:
- `userId`: The ID of the user changing their password
**Access Level**: Role 3 (Client/Клиент)

#### Button_Click Method (Change Password)
```csharp
private void Button_Click(object sender, RoutedEventArgs e)
```
**Description**: Handles password change functionality with validation.

**Validation Rules**:
- All fields are required
- New password must match confirmation
- Old password must be correct

**Process Flow**:
1. Validate input fields
2. Verify old password against database
3. Update password in database
4. Return to main login window

**Example Usage**:
```csharp
// User fills in:
TextBox3.Text = "oldpassword";    // Current password
TextBox4.Text = "newpassword123"; // New password
TextBox5.Text = "newpassword123"; // Confirm new password
// Click change password button
```

## User Interfaces

### MainWindow.xaml
**Purpose**: Login interface
**Components**:
- TextBox1: Login input field
- TextBox2: Password input field (should be PasswordBox for security)
- button1: Login button

### Window1.xaml
**Purpose**: Manager welcome screen
**Components**:
- Label2: Welcome message for managers

### Window2.xaml
**Purpose**: Administrator user management interface
**Components**:
- **User Input Fields**:
  - TextBox1: Login
  - TextBox2: Password
  - TextBox3: Surname
  - TextBox4: Name
  - TextBox5: Othcestvo
  - typeRole: Role selection ComboBox
- **Action Buttons**:
  - button3: Add user
  - button4: Save changes
- **Data Display**:
  - GridUser: DataGrid showing all users with columns for all user properties

### UpdatePass.xaml
**Purpose**: Password change interface for clients
**Components**:
- TextBox3: Old password input
- TextBox4: New password input
- TextBox5: Confirm new password input
- button2: Change password button

## Configuration

### Database Configuration
The application uses hardcoded database connection strings:
```csharp
string connString = "Host=localhost;Username=postgres;Password=root;Database=postgres";
```

### Application Configuration
- **Target Framework**: .NET 8.0 Windows
- **UI Framework**: WPF enabled
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled

### Dependencies
```xml
<PackageReference Include="Npgsql" Version="9.0.3" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
```

## Usage Examples

### 1. User Login Process
```csharp
// 1. User enters credentials
MainWindow mainWindow = new MainWindow();
mainWindow.TextBox1.Text = "admin";      // Login
mainWindow.TextBox2.Text = "password";    // Password

// 2. Click login button - triggers authentication
// 3. System redirects based on role:
//    - Role 1: Opens Window1 (Manager)
//    - Role 2: Opens Window2 (Administrator)
//    - Role 3: Opens UpdatePass (Client)
```

### 2. Administrator Adding New User
```csharp
// 1. Access administrator interface (Role 2)
Window2 adminWindow = new Window2();

// 2. Fill user details
adminWindow.TextBox1.Text = "johndoe";        // Login
adminWindow.TextBox2.Text = "securepass";     // Password
adminWindow.TextBox3.Text = "Doe";            // Surname
adminWindow.TextBox4.Text = "John";           // Name
adminWindow.TextBox5.Text = "William";        // Othcestvo
adminWindow.typeRole.SelectedValue = "3";     // Client role

// 3. Click Add button to create user
```

### 3. Client Changing Password
```csharp
// 1. Client logs in and is redirected to UpdatePass
UpdatePass updateWindow = new UpdatePass(userId);

// 2. Fill password fields
updateWindow.TextBox3.Text = "currentpass";   // Old password
updateWindow.TextBox4.Text = "newpass123";    // New password
updateWindow.TextBox5.Text = "newpass123";    // Confirm password

// 3. Click change password button
```

### 4. Database Query Examples
```csharp
// Retrieve user by login
var checkUserCmd = new NpgsqlCommand(
    "SELECT id, login, password, role, surname, count, date, active " +
    "FROM \"Users\" WHERE login = @login", conn);
checkUserCmd.Parameters.AddWithValue("login", login);

// Update user login attempts
var updateCmd = new NpgsqlCommand(
    "UPDATE \"Users\" SET " +
    "count = CASE WHEN count < 3 THEN count + 1 ELSE 3 END, " +
    "active = CASE WHEN count >= 3 THEN B'1' ELSE active END " +
    "WHERE id = @id", conn);

// Insert new user
var insertCmd = new NpgsqlCommand(
    "INSERT INTO \"Users\" " +
    "(surname, name, othcestvo, role, login, password, count, active, date) " +
    "VALUES (@surname, @name, @othcestvo, @role, @login, @password, @count, @active::bit(1), @date)",
    conn);
```

## Error Handling

### Exception Types Handled
1. **NpgsqlException**: Database connection and query errors
2. **General Exception**: All other unexpected errors

### Error Messages
- **Invalid Credentials**: "Неверный логин или пароль"
- **Account Blocked**: "Вы заблокированы. Обратитесь к администратору"
- **Inactive Account**: "Аккаунт заблокирован из-за долгого отсутствия"
- **Failed Login**: "Неверный пароль. Осталось попыток: {attempts}"
- **Missing Fields**: "Поля обязательны для заполнения"
- **Database Error**: "Ошибка БД: {error message}"

### Best Practices for Error Handling
```csharp
try
{
    // Database operations
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
```

## Security Features

### Authentication Security
1. **Login Attempt Tracking**: Maximum 3 failed attempts before account lockout
2. **Account Lockout**: Automatic blocking after failed attempts
3. **Session Timeout**: Accounts blocked after 1 month of inactivity
4. **Password Validation**: Required for all operations

### Security Limitations
⚠️ **Important Security Notes**:
- Passwords are stored in plain text (should use hashing)
- No password complexity requirements
- No session management
- Hardcoded database credentials
- TextBox used for password input (should use PasswordBox)

### Recommended Security Improvements
1. Implement password hashing (BCrypt/Argon2)
2. Add password complexity requirements
3. Use PasswordBox for password inputs
4. Implement proper session management
5. Use configuration files for database connections
6. Add input validation and sanitization
7. Implement audit logging

### Role-Based Access Control
```csharp
switch (role)
{
    case 1: // Manager
        new Window1().Show();
        break;
    case 2: // Administrator
        new Window2().Show();
        break;
    case 3: // Client
        new UpdatePass(userId).Show();
        break;
}
```

## Additional Database Tables (Planned)

The system includes SQL schemas for additional hotel management functionality:

### Core Tables
- **Role**: User role definitions
- **Number**: Hotel room information
- **NumberStatus**: Room status tracking
- **Category**: Room categories
- **Guest**: Guest information
- **Client**: Client details
- **Score**: Billing and payments
- **PaymentType**: Payment method types
- **RoomServices**: Available room services
- **Personnel**: Staff management
- **Tasks**: Operational tasks
- **Statistics**: System analytics
- **Report**: Reporting functionality
- **Tariffs**: Pricing information
- **Hotel**: Main hotel entity

### Future API Extensions
These tables suggest planned functionality for:
- Hotel room management
- Guest registration and check-in/out
- Billing and payment processing
- Staff task management
- Reporting and analytics
- Service management

---

*This documentation covers the current implementation of the User Management System. For questions or contributions, please refer to the source code and database schema.*