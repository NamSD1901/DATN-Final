FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy the entire backend directory into the container
COPY . ./

# Restore dependencies
RUN dotnet restore "src/WebApi/WebApi.csproj"

# Build and publish the application
RUN dotnet publish "src/WebApi/WebApi.csproj" -c Release -o /app/out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

# Set environment variables for Render (listen on port 80 or 8080)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApi.dll"]
