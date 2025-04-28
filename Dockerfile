FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.sln .
COPY Users.Api/*.csproj ./Users.Api/
COPY Users.Tests/*.csproj ./Users.Tests/
RUN dotnet restore

# Copy the rest of the code
COPY . .

# Build and publish
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_ENVIRONMENT=Production

# Note: Set JWT_SECRET_KEY at runtime using:
# docker run -e JWT_SECRET_KEY="your_secure_key_here" ...

EXPOSE 10000
ENTRYPOINT ["dotnet", "Users.Api.dll"] 