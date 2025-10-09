🏪 StockWise

StockWise is an advanced warehouse management system built with ASP.NET Core Web API.
It helps businesses efficiently track products, manage stock levels, handle transfers between warehouses, and monitor inventory updates in real-time.

🚀 Features

Manage products, warehouses, and stock quantities.

Handle product transfers between warehouses.

Apply validations and business rules for stock management.

Centralized error handling using custom middleware.

Layered architecture (API, Application, Domain, Infrastructure).

Integrated logging for better system monitoring.

🧩 Technologies Used

.NET 8 / ASP.NET Core Web API

Entity Framework Core

SQL Server

DTO

Dependency Injection

Repository & Unit of Work Pattern

Middleware for Error Handling

🏗️ Project Structure
StockWise/
│
├── StockWise.API/              → Presentation layer (Controllers, Middlewares)
├── StockWise.Application/      → Business logic, DTOs, Interfaces, Services
├── StockWise.Domain/           → Core entities and exceptions
├── StockWise.Infrastructure/   → EF Core configurations, Repositories, DbContext
└── README.md                   → Project documentation

⚙️ Getting Started

Clone the repository:

git clone https://github.com/the-khaled/StockWise


Update the connection string in appsettings.json.

Apply migrations:

dotnet ef database update


Run the API:

dotnet run


Test the endpoints using Swagger or Postman.
