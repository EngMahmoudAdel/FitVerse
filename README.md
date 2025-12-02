# FitVerse - Fitness Management Platform

<div align="center">

![FitVerse](https://img.shields.io/badge/FitVerse-Fitness%20Platform-blue?style=for-the-badge)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core%209.0-512BD4?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoft-sql-server)
![SignalR](https://img.shields.io/badge/SignalR-Real--time-00ADD8?style=for-the-badge)

**A comprehensive fitness coaching and client management platform built with ASP.NET Core MVC**

[Features](#-features) • [Architecture](#-architecture) • [Installation](#-installation) • [Usage](#-usage) • [Technologies](#-technologies)

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Architecture](#-architecture)
- [Technologies](#-technologies)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Database Setup](#-database-setup)
- [Default Users](#-default-users)
- [Project Structure](#-project-structure)
- [Usage](#-usage)
- [API Endpoints](#-api-endpoints)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Overview

**FitVerse** is a modern, full-featured fitness management platform that connects fitness coaches with clients. It provides comprehensive tools for workout planning, diet management, progress tracking, real-time communication, and more. Built using ASP.NET Core 9.0 with a clean N-tier architecture, FitVerse emphasizes scalability, maintainability, and user experience.

### Key Highlights

- 🏋️ **Multi-Role System**: Supports Admin, Coach, and Client roles
- 💬 **Real-Time Chat**: SignalR-powered instant messaging and notifications
- 📊 **Progress Tracking**: Daily logs, exercise plans, and diet plans
- 💳 **Payment Management**: Package subscriptions and payment tracking
- 🎨 **Responsive UI**: Modern and intuitive user interface
- 🔐 **Secure Authentication**: ASP.NET Identity with role-based authorization

---

## ✨ Features

### For Administrators
- 👥 **User Management**: Manage coaches, clients, and their accounts
- 📦 **Package Management**: Create and manage subscription packages
- 🏋️ **Exercise Library**: Manage exercises, equipment, muscles, and anatomy
- 📊 **Analytics Dashboard**: View system-wide statistics and reports
- 🔧 **System Configuration**: Configure specialties and coach packages

### For Coaches
- 👨‍⚕️ **Client Management**: View and manage assigned clients
- 📋 **Exercise Plans**: Create customized workout plans for clients
- 🥗 **Diet Plans**: Design personalized nutrition plans with calorie calculations
- 💬 **Real-Time Chat**: Communicate with clients instantly
- 📝 **Daily Logs**: Review client progress and daily activities
- 💰 **Package Offerings**: Manage and offer training packages
- ⭐ **Feedback System**: Receive and view client feedback

### For Clients
- 🎯 **Goal Setting**: Define fitness goals (weight loss, muscle gain, etc.)
- 📅 **Workout Plans**: Access personalized exercise plans from coaches
- 🍽️ **Diet Plans**: Follow customized nutrition programs
- 📊 **Progress Dashboard**: Track weight, measurements, and achievements
- 📝 **Daily Logging**: Record daily workouts, meals, and progress
- 💬 **Coach Communication**: Chat with coaches in real-time
- 💳 **Subscription Management**: Purchase and manage training packages
- ⭐ **Coach Reviews**: Provide feedback on coaching services

---

## 🏗️ Architecture

FitVerse follows a clean **N-Tier Architecture** pattern with clear separation of concerns:

```
FitVerse/
├── FitVerse.Core/          # Business Logic Layer
│   ├── Models/             # Domain entities
│   ├── ViewModels/         # DTOs for data transfer
│   ├── IService/           # Service interfaces
│   ├── Interfaces/         # Repository interfaces
│   ├── Enums/             # Enumerations
│   ├── Helpers/           # Helper classes and constants
│   └── MapperConfigs/     # AutoMapper profiles
│
├── FitVerse.Data/          # Data Access Layer
│   ├── Context/           # EF Core DbContext
│   ├── Repositories/      # Repository implementations
│   ├── Configurations/    # Entity configurations
│   ├── Migrations/        # Database migrations
│   └── Seed/             # Database seeding
│
├── FitVerse.Service/       # Service Layer
│   ├── Service/           # Business logic implementations
│   └── UnitOfWorkServices/# Unit of Work pattern
│
└── FitVerse.Web/          # Presentation Layer
    ├── Controllers/       # MVC Controllers
    ├── Views/            # Razor views
    ├── Hubs/             # SignalR hubs
    ├── Filters/          # Action filters
    └── wwwroot/          # Static files
```

### Design Patterns Used

- **Repository Pattern**: For data access abstraction
- **Unit of Work Pattern**: For managing transactions
- **Dependency Injection**: For loose coupling
- **DTO Pattern**: For data transfer between layers
- **Builder Pattern**: For complex object creation

---

## 🛠️ Technologies

### Backend
- **ASP.NET Core 9.0** - Web framework
- **Entity Framework Core 9.0** - ORM
- **ASP.NET Identity** - Authentication & Authorization
- **SignalR** - Real-time communication
- **AutoMapper 15.0** - Object mapping
- **SQL Server** - Primary database

### Frontend
- **Razor Pages** - Server-side rendering
- **jQuery 3.7** - DOM manipulation
- **SignalR Client** - Real-time updates
- **SweetAlert2** - Beautiful alerts
- **Bootstrap** - Responsive design
- **CSS3 & JavaScript** - Custom styling and interactions

### Additional Libraries
- **Microsoft.EntityFrameworkCore.Proxies** - Lazy loading
- **BuilderGenerator** - Code generation
- **MySQL.Data** - MySQL support (optional)

---

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- ✅ [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (2019 or later)
- ✅ [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community or higher)
- ✅ [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/sql/ssms/) (optional)

---

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/EngMahmoudAdel/FitVerse.git
cd FitVerse
```

### 2. Configure Connection String

Create `appsettings.json` in the `FitVerse.WebUI` project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FitVerseDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 3. Restore NuGet Packages

```bash
dotnet restore
```

### 4. Build the Solution

```bash
dotnet build
```

---

## 💾 Database Setup

### Option 1: Using Package Manager Console (Recommended)

1. Open **Package Manager Console** in Visual Studio
2. Set `FitVerse.WebUI` as the startup project
3. Set `FitVerse.Data` as the default project in PMC
4. Run the following commands:

```powershell
# Create a new migration (if needed)
Add-Migration InitialCreate

# Update the database
Update-Database
```

### Option 2: Using .NET CLI

```bash
cd FitVerse.Data
dotnet ef database update --startup-project ../FitVerse.WebUI
```

### Database Seeding

The application automatically seeds the database with:
- **Roles**: Admin, Coach, Client
- **Default Users** (see below)
- **Sample Data** (in development mode only)

---

## 👤 Default Users

The system comes pre-configured with default accounts for testing:

| Role   | Email                  | Password    | Description                    |
|--------|------------------------|-------------|--------------------------------|
| Admin  | admin@fitverse.com     | Admin@123   | Full system administration     |
| Coach  | coach@fitverse.com     | Coach@123   | Coach functionalities          |
| Client | client@fitverse.com    | Client@123  | Client functionalities         |

> ⚠️ **Security Note**: Please change these passwords immediately after first login in production!

---

## 📁 Project Structure

### FitVerse.Core (Business Logic)

#### Models
- `ApplicationUser` - Extended Identity user
- `Client` - Client profile and data
- `Coach` - Coach profile and credentials
- `Exercise` - Exercise library
- `ExercisePlan` - Workout plans
- `DietPlan` - Nutrition plans
- `DailyLog` - Progress tracking
- `Chat` & `Message` - Messaging system
- `Notification` - Real-time notifications
- `Package` & `Payment` - Subscription management
- `Muscle`, `Equipment`, `Anatomy` - Exercise categorization

#### ViewModels
Organized by feature area (Admin, Coach, Client, etc.)

#### Services Interfaces
- `IAccountService` - Authentication
- `ICoachService` - Coach operations
- `IClientService` - Client operations
- `IExerciseService` - Exercise management
- `IExercisePlanService` - Workout planning
- `IDietPlanService` - Diet planning
- `IChatService` - Messaging
- `INotificationService` - Notifications
- And many more...

### FitVerse.Data (Data Access)

#### Repositories
- Generic repository pattern
- Specialized repositories for each entity
- Unit of Work implementation

#### Configurations
- Fluent API entity configurations
- Relationship definitions
- Index and constraint configurations

### FitVerse.Service (Service Layer)

Implementation of business logic services, bridging controllers and repositories.

### FitVerse.Web (Presentation)

#### Controllers
- `AccountController` - Authentication
- `AdminController` - Admin dashboard
- `CoachController` - Coach features
- `ClientController` - Client features
- `ChatController` - Messaging
- `ExercisePlanController` - Workout management
- `DietPlanController` - Diet management
- And more...

#### Views
Organized by controller with shared layouts for each role.

#### Hubs
- `ChatHub` - SignalR hub for real-time messaging and notifications

---

## 🎮 Usage

### Running the Application

1. **Start the application**:
   ```bash
   dotnet run --project FitVerse.WebUI
   ```

2. **Navigate to**: `https://localhost:5001`

3. **Login** with one of the default accounts

### Admin Workflow

1. Login as admin
2. Manage system data (exercises, equipment, muscles, etc.)
3. View and manage users (coaches and clients)
4. Create and manage packages
5. Monitor system analytics

### Coach Workflow

1. Login as coach
2. View assigned clients
3. Create exercise plans for clients
4. Design diet plans with automatic calorie calculations
5. Chat with clients in real-time
6. Review client daily logs and progress
7. Manage offered packages

### Client Workflow

1. Register/Login as client
2. Browse and subscribe to coach packages
3. View assigned exercise and diet plans
4. Log daily activities (workouts, meals, weight)
5. Track progress on dashboard
6. Chat with coach
7. Provide feedback

---

## 🔌 API Endpoints

### Key Controller Actions

#### Account
- `GET /Account/Login` - Login page
- `POST /Account/Login` - Authenticate user
- `GET /Account/Register` - Registration page
- `POST /Account/Register` - Create new user
- `GET /Account/Logout` - Sign out

#### Admin
- `GET /Admin/Index` - Admin dashboard
- `GET /Admin/Coaches` - Manage coaches
- `GET /Admin/Clients` - Manage clients
- `GET /Admin/CoachPackages` - Package management

#### Coach
- `GET /Coach/Dashboard` - Coach dashboard
- `GET /Coach/MyClients` - View clients
- `POST /ExercisePlan/Create` - Create workout plan
- `POST /DietPlan/Create` - Create diet plan

#### Client
- `GET /ClientDashboard/Dashboard` - Client dashboard
- `GET /Coach/ClientCoaches` - Browse coaches
- `POST /Client/Payment` - Subscribe to package
- `POST /DailyLog/Create` - Log daily activity

#### SignalR Hub Methods
- `SendMessage(chatId, receiverId, message)` - Send chat message
- `SendNotification(receiverId, content, refId, type)` - Send notification
- `MarkMessageAsRead(messageId)` - Mark message as read
- `UserTyping(receiverId)` - Typing indicator

---

## 🔐 Security Features

- ✅ **ASP.NET Identity** for authentication
- ✅ **Role-based authorization** (Admin, Coach, Client)
- ✅ **Cookie authentication** with secure policies
- ✅ **Anti-forgery tokens** on forms
- ✅ **Password policies** (configurable)
- ✅ **Session management** with timeout
- ✅ **User info action filter** for security context

---

## 📊 Database Schema

### Key Entities and Relationships

- `ApplicationUser` (1:1) → `Client` / `Coach`
- `Coach` (1:N) → `ExercisePlan`, `DietPlan`, `ClientSubscription`
- `Client` (1:N) → `ExercisePlan`, `DietPlan`, `DailyLog`, `Payment`
- `Exercise` (N:1) → `Muscle`, `Equipment`
- `ExercisePlan` (1:N) → `ExercisePlanDetail` (N:1) → `Exercise`
- `Chat` (1:N) → `Message`
- `Package` (1:N) → `Payment`, `ClientSubscription`

---

## 🧪 Testing

### Development Mode Features

- Automatic sample data seeding
- Detailed logging
- Developer exception pages

### Running Tests

```bash
# Build and run the application in development mode
dotnet run --project FitVerse.WebUI --environment Development
```

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML comments for public methods
- Write unit tests for new features
- Keep controllers thin, services thick

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Mahmoud Adel**

- GitHub: [@EngMahmoudAdel](https://github.com/EngMahmoudAdel)
- Project: [FitVerse](https://github.com/EngMahmoudAdel/FitVerse)

---

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Entity Framework Core for powerful ORM
- SignalR for real-time capabilities
- All open-source contributors

---

## 📞 Support

For support, email support@fitverse.com or create an issue in the GitHub repository.

---

## 🗺️ Roadmap

- [ ] Mobile application (iOS/Android)
- [ ] Advanced analytics and reporting
- [ ] Integration with fitness trackers
- [ ] AI-powered workout recommendations
- [ ] Video exercise demonstrations
- [ ] Social features and community
- [ ] Multi-language support
- [ ] Payment gateway integration

---

<div align="center">

**Made with ❤️ for the fitness community**

[⬆ Back to Top](#fitverse---fitness-management-platform)

</div>
