# Modern POS System

A comprehensive, cloud-native Point of Sale (POS) system built with .NET 8 and modern architecture patterns. Designed for restaurants, retail stores, and multi-branch businesses requiring scalable, reliable, and feature-rich POS capabilities.

## 🚀 Overview

The Modern POS System is a multi-tenant, modular monolith that provides complete point-of-sale functionality with advanced features like offline operation, real-time synchronization, comprehensive reporting, and multi-branch management. Built with enterprise-grade architecture patterns, it can scale from single-store operations to large multi-branch enterprises.

## ✨ Key Features

### Core POS Functionality
- **Sales Processing**: Fast, intuitive checkout with barcode scanning and multiple payment methods
- **Inventory Management**: Real-time stock tracking, automated reordering, and supplier management
- **Customer Management**: Customer profiles, loyalty programs, and purchase history
- **Multi-Branch Support**: Centralized management with branch-specific operations
- **Offline Operation**: Critical functions work without internet connectivity

### Advanced Features
- **Recipe Management**: Bill of Materials (BOM) for restaurants and manufacturing
- **Promotion Engine**: Flexible discount rules, coupons, and loyalty rewards
- **Comprehensive Reporting**: Sales analytics, inventory reports, and business intelligence
- **Multi-Tenant Architecture**: Complete data isolation between organizations
- **Real-Time Synchronization**: Instant updates across all connected devices
- **Progressive Web App**: Works on desktop, tablet, and mobile devices

### Technical Excellence
- **Modular Monolith**: Clean architecture with module boundaries for future microservices migration
- **Event-Driven Design**: Loose coupling through domain events and CQRS patterns
- **Multi-Tenancy**: Secure, isolated data for multiple organizations
- **Scalable Infrastructure**: Built for high availability and horizontal scaling
- **Modern UI/UX**: Responsive Blazor interface with real-time updates

## Project Structure

```
src/
├── POS.Web/                          # Presentation Layer (Blazor + Web API)
├── POS.Shared/                       # Shared Kernel
│   ├── Domain/                       # Common domain objects
│   ├── Infrastructure/               # Common infrastructure interfaces
│   └── Events/                       # Domain events
├── POS.Infrastructure/               # Infrastructure Layer
│   ├── Data/                         # EF Core context and repositories
│   └── Events/                       # Event bus implementation
└── POS.Modules/                      # Application Modules
    ├── Authentication/               # User authentication and authorization
    ├── Sales/                        # Sales and checkout functionality
    ├── Inventory/                    # Product and inventory management
    ├── Customers/                    # Customer relationship management
    ├── Branches/                     # Multi-branch management
    ├── Reporting/                    # Analytics and reporting
    ├── Payments/                     # Payment processing
    ├── Recipes/                      # Recipe/BOM management
    └── Promotions/                   # Discount and promotion engine
```

## 🛠️ Technology Stack

### Backend
- **.NET 8** - Latest LTS framework with performance improvements
- **ASP.NET Core** - Web API and Blazor Server hosting
- **Entity Framework Core 8** - ORM with PostgreSQL provider
- **MediatR** - CQRS pattern and internal messaging
- **FluentValidation** - Input validation and business rules
- **AutoMapper** - Object-to-object mapping
- **Hangfire** - Background job processing

### Frontend
- **Blazor Server** - Interactive web UI with real-time updates
- **SignalR** - Real-time communication
- **Bootstrap 5** - Responsive UI framework
- **Progressive Web App** - Offline capabilities and mobile support

### Data & Infrastructure
- **PostgreSQL** - Primary relational database
- **Redis** - Caching and session management
- **Entity Framework Migrations** - Database schema management
- **Multi-tenancy** - Row-level security and data isolation

### Development & Testing
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework for tests
- **FluentAssertions** - Readable test assertions
- **Docker** - Containerization support (planned)

### Architecture Patterns
- **Modular Monolith** - Domain-driven module boundaries
- **CQRS** - Command Query Responsibility Segregation
- **Domain Events** - Event-driven architecture
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management

## 🚀 Quick Start

### Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PostgreSQL 13+** - [Download here](https://www.postgresql.org/download/)
- **Redis** (optional) - For caching and session management
- **Visual Studio 2022** or **VS Code** with C# extension

### 1. Clone and Build

```bash
git clone <repository-url>
cd modern-pos-system
dotnet restore
dotnet build
```

### 2. Database Setup

1. Create a PostgreSQL database:
```sql
CREATE DATABASE modernpos_dev;
```

2. Update connection string in `src/POS.Web/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=modernpos_dev;Username=postgres;Password=your_password",
    "Redis": "localhost:6379"
  }
}
```

3. Run database migrations:
```bash
dotnet ef database update --project src/POS.Infrastructure --startup-project src/POS.Web
```

### 3. Run the Application

```bash
cd src/POS.Web
dotnet run
```

The application will be available at:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`
- **Hangfire Dashboard**: `https://localhost:5001/hangfire` (Development only)

### 4. Health Check

Verify the system is running correctly:
```bash
curl https://localhost:5001/api/health
```

Expected response:
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-01T00:00:00Z",
  "database": "Connected",
  "version": "1.0.0"
}
```

## Module System

Each module is self-contained and implements the `IModule` interface for dependency injection configuration. Modules communicate through domain events via the internal event bus.

### Adding a New Module

1. Create a new class library project in `src/POS.Modules.{ModuleName}/`
2. Implement the `IModule` interface
3. Add the module to the solution and reference it in `POS.Web`
4. Register the module in `Program.cs`

## 📋 Development Status

### ✅ Completed (Phase 1)
- [x] **Core Infrastructure**: .NET 8 solution with modular architecture
- [x] **Database Layer**: Entity Framework Core with PostgreSQL
- [x] **Shared Kernel**: Common domain objects, value objects, and interfaces
- [x] **Multi-Tenancy**: Tenant context and data isolation
- [x] **Event System**: Domain events with MediatR integration
- [x] **Background Jobs**: Hangfire integration for async processing
- [x] **Caching**: Redis-based caching infrastructure
- [x] **Testing**: Unit test foundation with xUnit

### 🚧 In Progress (Phase 2)
- [ ] **Authentication Module**: User management and JWT authentication
- [ ] **Sales Module**: Core POS functionality and checkout
- [ ] **Inventory Module**: Product and stock management
- [ ] **Customer Module**: Customer profiles and management

### 📅 Planned (Phase 3+)
- [ ] **Branch Management**: Multi-branch operations
- [ ] **Reporting Module**: Analytics and business intelligence
- [ ] **Payment Processing**: Multiple payment method support
- [ ] **Recipe Management**: BOM and ingredient tracking
- [ ] **Promotion Engine**: Discounts and loyalty programs

## 📚 Documentation

### 📋 Project Documentation
- **[Project Overview](docs/PROJECT-OVERVIEW.md)** - Executive summary and business context
- **[Feature Overview](docs/FEATURES.md)** - Comprehensive feature list and capabilities
- **[Development Plan](docs/DEVELOPMENT-PLAN.md)** - Detailed development roadmap and phases

### 🏗️ Technical Documentation
- **[Architecture Guide](docs/ARCHITECTURE.md)** - System architecture and design patterns
- **[Multi-Tenancy Guide](docs/MULTI-TENANCY.md)** - Database-per-tenant multi-tenancy implementation
- **[Technology Stack](docs/TECH-STACK.md)** - Detailed technology choices and rationale
- **[API Documentation](docs/api.md)** - REST API reference (coming soon)
- **[Deployment Guide](docs/deployment.md)** - Production deployment instructions (coming soon)

### 📝 Specification Documents
- **[Requirements](.kiro/specs/modern-pos-system/requirements.md)** - Detailed functional requirements
- **[System Design](.kiro/specs/modern-pos-system/design.md)** - Technical design and architecture
- **[Implementation Tasks](.kiro/specs/modern-pos-system/tasks.md)** - Development task list and progress

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Development Guidelines
- Follow **Clean Architecture** principles
- Write **unit tests** for business logic
- Use **domain events** for cross-module communication
- Maintain **module boundaries** and avoid direct dependencies
- Follow **C# coding standards** and use EditorConfig

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Issues**: [GitHub Issues](https://github.com/your-repo/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-repo/discussions)
- **Documentation**: [Wiki](https://github.com/your-repo/wiki)

---

**Built with ❤️ using .NET 8 and modern architecture patterns**