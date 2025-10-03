StockWise - Inventory Management System
## 📖 Overview
StockWise is a robust inventory management system built with ASP.NET Core 8, designed to streamline stock tracking, transfers, invoicing, and expense management for businesses. It provides a RESTful API with JWT-based authentication to ensure secure access to resources. The system uses Entity Framework Core with SQL Server for data persistence and follows a clean architecture with separation of concerns.
Key features include:

Inventory Management: Track products, warehouses, stock levels, and locations.
Transfers: Manage stock movements between warehouses.
Invoicing and Payments: Handle customer invoices and payments.
Returns and Expenses: Process returns and track expenses.
User Authentication: Secure endpoints with JWT-based authentication and role-based authorization.

🛠️ Technologies Used

Backend: ASP.NET Core 8
Database: Entity Framework Core with SQL Server
Authentication: JWT (JSON Web Tokens)
Logging: Microsoft.Extensions.Logging
Serialization: Newtonsoft.Json
API Documentation: Swagger/OpenAPI
Validation: Data Annotations
Dependency Injection: Built-in ASP.NET Core DI
Repository Pattern: With Unit of Work for data access

📋 Project Structure
The project follows a Clean Architecture approach, divided into the following layers:

StockWise.Domain: Contains entity models (Product, Warehouse, Transfer, User, etc.).
StockWise.Infrastructure: Handles data access with EF Core (StockWiseDbContext, Repositories, Unit of Work).
StockWise.Application: Implements business logic and services (ITransferService, IUserService, etc.) with DTOs for data transfer.
StockWise.API: Exposes RESTful endpoints with controllers and middleware for error handling.

StockWise/
├── StockWise.Domain/
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Product.cs
│   │   ├── Warehouse.cs
│   │   └── ...
├── StockWise.Infrastructure/
│   ├── DataAccess/
│   │   ├── StockWiseDbContext.cs
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   ├── TransferRepository.cs
│   │   └── ...
├── StockWise.Application/
│   ├── Dtos/
│   │   ├── RegisterDto.cs
│   │   ├── TransferDto.cs
│   │   └── ...
│   ├── Services/
│   │   ├── UserService.cs
│   │   ├── TransferService.cs
│   │   └── ...
├── StockWise.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── TransfersController.cs
│   │   └── ...
│   ├── Middleware/
│   │   ├── ErrorHandlerMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json

🚀 Getting Started
Prerequisites

.NET 8 SDK
SQL Server (or SQL Server Express)
Git
Visual Studio 2022 
Postman or Swagger UI for testing APIs

Installation

Clone the repository:
git clone https://github.com/your-username/StockWise.git
cd StockWise


Restore dependencies:
dotnet restore


Configure the database:

Update the connection string in StockWise.API/appsettings.json:
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=StockWise;Trusted_Connection=True;TrustServerCertificate=True;"
}


Apply migrations to create the database:
dotnet ef database update --project StockWise.Infrastructure --startup-project StockWise.API




Run the application:
dotnet run --project StockWise.API

The API will be available at https://localhost:5001 (or your configured port).

Access Swagger:

Open your browser and navigate to https://localhost:5001/swagger to explore and test the API endpoints.



Configuration

JWT Settings: Update the Jwt section in appsettings.json with a secure key:
"Jwt": {
  "Key": "YourSuperSecretKey1234567890AtLeast32Chars",
  "Issuer": "StockWiseAPI",
  "Audience": "StockWiseAPI"
}

Ensure the Key is at least 32 characters long and stored securely (e.g., in environment variables for production).

Logging: Configured to output to the console and debug window. Modify appsettings.json for additional logging providers if needed.


🔐 Authentication
StockWise uses JWT-based authentication to secure API endpoints.

Register a new user:

Endpoint: POST /api/auth/register

Body:
{
  "username": "admin",
  "password": "P@ssw0rd",
  "role": "Admin"
}


Response: {"message": "User registered successfully."}



Login to obtain a JWT token:

Endpoint: POST /api/auth/login

Body:
{
  "username": "admin",
  "password": "P@ssw0rd"
}


Response: {"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."}



Use the token:

Add the token to the Authorization header for protected endpoints:
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...





Protected Endpoints
Some endpoints require authentication and specific roles (e.g., Admin, Manager). Example:

POST /api/transfers: Requires Admin or Manager role.
Use [Authorize] or [Authorize(Roles = "Admin,Manager")] in controllers to restrict access.

🛠️ API Endpoints
Below are the main API endpoints. All endpoints are prefixed with /api/.



Endpoint
Method
Description
Authentication



/auth/register
POST
Register a new user
None


/auth/login
POST
Login and get JWT token
None


/transfers
GET
Get all transfers
Required


/transfers/{id}
GET
Get a transfer by ID
Required


/transfers
POST
Create a new transfer
Required (Admin, Manager)


/products
GET
Get all products
Required


/warehouses
GET
Get all warehouses
Required


/customers
GET
Get all customers
Required


...
...
...
...


For a complete list, explore the Swagger UI at /swagger.
🧪 Testing
The project includes a test project (StockWise.Tests) using xUnit and Moq for unit testing.

Run tests:
dotnet test StockWise.Tests


Add new tests:

Tests are located in StockWise.Tests/Services/.
Example: TransferServiceTests.cs tests the TransferService methods.



🚀 Deployment
To deploy StockWise to a production environment:

Database: Deploy the SQL Server database and update the connection string in appsettings.json.

Environment Variables: Store sensitive data (e.g., Jwt:Key, connection string) in environment variables or a secret manager.

Hosting: Use a hosting provider like Azure, AWS, or Docker.

For Docker, create a Dockerfile:
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk: 8 AS build
WORKDIR /src
COPY ["StockWise.API/StockWise.API.csproj", "StockWise.API/"]
COPY ["StockWise.Application/StockWise.Application.csproj", "StockWise.Application/"]
COPY ["StockWise.Infrastructure/StockWise.Infrastructure.csproj", "StockWise.Infrastructure/"]
COPY ["StockWise.Domain/StockWise.Domain.csproj", "StockWise.Domain/"]
RUN dotnet restore "StockWise.API/StockWise.API.csproj"
COPY . .
WORKDIR "/src/StockWise.API"
RUN dotnet build "StockWise.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StockWise.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockWise.API.dll"]




CI/CD: Set up a CI/CD pipeline (e.g., GitHub Actions) for automated builds and deployments.


🤝 Contributing
Contributions are welcome! To contribute:

Fork the repository.
Create a new branch (git checkout -b feature/your-feature).
Commit your changes (git commit -m 'Add your feature').
Push to the branch (git push origin feature/your-feature).
Open a Pull Request.

Please ensure your code follows the project's coding standards and includes unit tests.
📜 License
This project is licensed under the MIT License - see the LICENSE file for details.
📞 Contact
For questions or feedback, reach out to khaledgamal6389@gmail.com
 or open an issue on GitHub.

Happy Inventory Management with StockWise! 🚀
