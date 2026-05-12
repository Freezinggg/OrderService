# Start from an existing machine image that already has .NET 9 SDK installed.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# From now on, execute commands inside /src
WORKDIR /src

# Copy everything from my current project folder into /src inside container
COPY . .

# RUN means execute this command while building the image
RUN dotnet restore

RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "OrderService.API.dll"]