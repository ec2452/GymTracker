
# GymTracker

GymTracker is a web application developed using ASP.NET Core 8 MVC for managing gym members, trainers, workout plans, exercises, body measurements, and personal fitness goals.

The application provides an MVC-based web interface and a REST API, supporting multiple user roles and secure access to application resources. It enables users to manage workout activities, monitor their progress, and track personal fitness goals.

The project demonstrates practical implementation of back-end web development concepts, including database management, authentication, authorization, REST API development, repository pattern, logging, and automated testing.

## Features

### User Management
- User registration and login
- Member and trainer management
- User account management
- Role-based access control

### Workout Management
- Create and manage workout plans
- Manage exercises
- Organize exercises into workout plans
- Track workout activities

### Progress Tracking
- Record and manage body measurements
- Set personal fitness goals
- Track workout progress
- Monitor fitness development over time

### Authentication and Authorization
- ASP.NET Identity for user authentication and account management
- JWT Bearer authentication for REST API access
- Google OAuth integration
- Role-based authorization
- Protected API endpoints

### Logging and Monitoring
- Serilog for structured application logging
- Console logging
- Rolling text log files
- Structured JSON log files
- HTTP request logging with response status codes and execution times

### AI Integration and MCP
- AI-assisted data entry service
- Anthropic HTTP client integration
- Model Context Protocol (MCP) server
- Custom MCP tools for programmatic interaction with application functionality

## User Roles

The application supports three user roles:

- **Admin**
- **Trainer**
- **User**

Role-based authorization is implemented using ASP.NET Core Identity.

## Tech Stack

| Technology | Purpose |
|---|---|
| C# | Programming language |
| .NET 8 | Development platform |
| ASP.NET Core MVC | Web application framework |
| Razor Pages | Web interface |
| Entity Framework Core | Object-relational mapping |
| SQL Server | Relational database |
| ASP.NET Identity | Authentication and user management |
| JWT Bearer | Token-based API authentication |
| Google OAuth | External authentication |
| Serilog | Structured logging |
| xUnit | Unit testing framework |
| ASP.NET Core MVC Testing | Integration testing support |
| EF Core InMemory | In-memory database testing |
| Coverlet | Code coverage collection |
| Playwright | End-to-end testing |

## Application Architecture

GymTracker follows the ASP.NET Core MVC architectural pattern and uses a repository-based data access approach.

The application is organized into several components:

- **Web Layer** – handles HTTP requests, controllers, views, filters, and application endpoints.
- **Data Access Layer (DAL)** – manages database access through Entity Framework Core.
- **Repositories** – provide data access operations for users, workouts, exercises, workout plans, goals, and measurements.
- **Services** – handle application services such as JWT token generation and AI-assisted data entry.
- **Authentication and Authorization** – manages user authentication and access control.

The application uses SQL Server for persistent data storage.

## Database and Data Access

The application uses Microsoft SQL Server as its relational database and Entity Framework Core for database operations.

The database context is implemented through `TeretanaDbContext`.

Dedicated repositories are registered for:

- Users
- Workouts
- Exercises
- Workout plans
- Goals
- Measurements
- Workout exercises

Entity Framework Core is used for object-relational mapping and database access.

## Authentication and Security

The application implements multiple authentication mechanisms:

- **ASP.NET Identity** – manages user accounts, registration, login, and roles.
- **JWT Bearer Authentication** – provides token-based authentication for API endpoints.
- **Google OAuth** – enables authentication through Google accounts.
- **Role-Based Authorization** – restricts access to application functionality according to assigned user roles.

The default authentication mechanism for the web application is Identity cookie authentication, while API controllers can explicitly require JWT authentication.

## REST API

GymTracker includes a REST API that provides programmatic access to application resources.

The API uses HTTP endpoints and supports JWT Bearer authentication for protected operations.

## AI Integration and MCP Server

The application includes an AI-assisted data entry service and an MCP (Model Context Protocol) server.

The MCP server uses HTTP transport and exposes custom tools through `TeretanaMcpTools`, allowing compatible MCP clients to interact with application functionality programmatically.

The MCP endpoint is mapped to `/mcp`.

## Logging and Monitoring

Serilog is integrated for structured application logging and HTTP request monitoring.

Logs are written to:
- Console output
- Rolling text log files
- Structured JSON log files

HTTP request logging captures:
- HTTP method
- Request path
- Response status code
- Request execution time

## Localization

The application supports Croatian and English cultures.

- Croatian (`hr`) is configured as the default culture.
- English (`en-US`) is also supported.
- Request localization uses the browser's `Accept-Language` header.

## Testing

The project includes an automated testing project targeting .NET 8.

### Testing Technologies

- **xUnit** – unit testing framework
- **Microsoft.AspNetCore.Mvc.Testing** – ASP.NET Core integration testing support
- **Entity Framework Core InMemory** – in-memory database provider for testing
- **Coverlet** – code coverage collection
- **Microsoft.NET.Test.Sdk** – test execution infrastructure
- **Playwright** – end-to-end testing

### Running Tests

To execute the test suite:

```bash
dotnet test
```

To collect code coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Microsoft SQL Server
- Visual Studio 2022 or another IDE with .NET support

### Installation

1. Clone the repository:

```bash
git clone https://github.com/ec2452/GymTracker.git
```

2. Navigate to the solution directory:

```bash
cd GymTracker
```

3. Configure the database connection string using `appsettings.json` or .NET User Secrets.

4. Configure the required authentication settings, including JWT settings and Google OAuth credentials if Google authentication is enabled.

5. Apply database migrations:

```bash
dotnet ef database update
```

6. Run the application:

```bash
dotnet run --project Sustavzapracenjenapretkauteretani/Sustavzapracenjenapretkauteretani.csproj
```

The application will be available at the local URL specified by the .NET development server.

## Author

**Ema Čurla**

Junior Back-end Developer

[GitHub Profile](https://github.com/ec2452)
