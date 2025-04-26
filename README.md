# Users API

![.NET CI](https://github.com/yourusername/Users/actions/workflows/dotnet-ci.yml/badge.svg)

A .NET 9 API project for managing user information with EF Core and SQLite integration.

## Features

- User management with CRUD operations
- Repository pattern implementation
- Entity Framework Core with SQLite
- TDD approach with NUnit, Shouldly, and Moq

## Development

This project follows TDD (Test-Driven Development) practices and uses:

- NUnit for unit and integration tests
- Shouldly for fluent assertions
- Moq for mocking dependencies
- Entity Framework Core for data access

## Code Coverage

This project includes code coverage analysis using [Coverlet](https://github.com/coverlet-coverage/coverlet) and [ReportGenerator](https://github.com/danielpalme/ReportGenerator).

To generate a code coverage report:

1. Run the `code-coverage.sh` script: `./code-coverage.sh`
2. View the HTML report at `coverage-report/index.html`

The script will:
- Execute all tests with coverage collection
- Generate an HTML report with detailed metrics
- Display coverage results by class and method

## API Testing

The project includes a comprehensive HTTP request file (`Users.Api.http`) that demonstrates all API endpoints and validation scenarios. This file can be used with REST Client extensions in VS Code, JetBrains Rider, or other HTTP client tools.

### Available Test Scenarios

- **GET User by ID**: Retrieve user details by ID
- **POST - Create a new user**: Create a user with address and employment information
- **POST - Create a user (duplicate email validation)**: Test email uniqueness validation
- **POST - Create a user (employment date validation)**: Test validation of employment dates
- **PUT - Update user (valid data)**: Update all user information including address and employment
- **PUT - Update user (address changes only)**: Test partial updates with address changes
- **PUT - Update user (employment changes only)**: Test partial updates with employment changes
- **PUT - Update user (invalid employment dates)**: Test validation of employment dates during update
- **PUT - Update user (duplicate email)**: Test email uniqueness validation during update

### How to Use

1. Make sure your API is running at the URL specified in the `.http` file (default: `https://localhost:3000`)
2. Open `Users.Api.http` in an editor with REST Client support
3. Click on the "Send Request" link above each request or use your editor's command to send HTTP requests
4. Review the responses to verify correct behavior

These sample requests help verify that the API correctly implements all validation rules:
- Email uniqueness across all users
- Employment end date must be after start date
- Required fields validation

## Deployment

The API is deployed on Render.com and is accessible at [https://users-api-jhnp.onrender.com/](https://users-api-jhnp.onrender.com/).

### Render.com Deployment Instructions

To deploy this application on Render:

1. **Prepare your repository**:
   - Ensure your repository has a `Dockerfile` (included in this repo)
   - Push changes to GitHub

2. **Create a Web Service on Render**:
   - Sign up/Sign in to [Render.com](https://render.com)
   - Create a "New Web Service"
   - Connect your GitHub repository
   - Choose "Docker" as the environment
   - Configure the service:
     - Set a name for your service
     - Select the appropriate region
     - Choose the branch to deploy
     - Set the Docker port to `10000` (matches the Dockerfile)

3. **Configure Environment Variables**:
   - Add the following environment variables:
     - `ConnectionStrings__DefaultConnection`: For the database connection
     - `ASPNETCORE_ENVIRONMENT`: Set to `Production`
     - Any other application-specific variables

4. **Deploy**:
   - Click "Create Web Service"
   - Wait for the build and deployment to complete

5. **Accessing the API**:
   - Once deployed, your API will be available at `https://[service-name].onrender.com/`
   - Use the provided HTTP request file to test the endpoints against the deployed API

### Updating Environment Variables Format

In Render.com, use double underscores (`__`) for nested configuration values from `appsettings.json`. For example:

```
ConnectionStrings__DefaultConnection=Data Source=users.db
Logging__LogLevel__Default=Information
```

## CI/CD

The project includes GitHub Actions workflows for:
- Continuous Integration (build and test)
- Code Quality Analysis

## Getting Started

1. Clone the repository
2. Restore dependencies: `dotnet restore`
3. Run the tests: `dotnet test`
4. Run the API: `dotnet run --project Users.Api` 