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

## CI/CD

The project includes GitHub Actions workflows for:
- Continuous Integration (build and test)
- Code Quality Analysis

## Getting Started

1. Clone the repository
2. Restore dependencies: `dotnet restore`
3. Run the tests: `dotnet test`
4. Run the API: `dotnet run --project Users.Api` 