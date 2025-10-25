# Use .NET SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the csproj file first and restore dependencies
COPY FlightProducerService/*.csproj ./FlightProducerService/
RUN dotnet restore ./FlightProducerService/FlightProducerService.csproj

COPY . .

WORKDIR /src/FlightProducerService
RUN dotnet publish -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "FlightProducerService.dll"]
