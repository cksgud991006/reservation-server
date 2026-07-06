# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["ReservationServer.csproj", "./"]
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish to /app/out
RUN dotnet publish -c Release -o /app/out

# --- Stage 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out .

# Run the app
ENTRYPOINT ["dotnet", "ReservationServer.dll"]