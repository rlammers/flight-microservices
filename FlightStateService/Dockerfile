FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FlightStateService.csproj .
RUN dotnet restore FlightStateService.csproj

COPY . .
RUN dotnet publish FlightStateService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "FlightStateService.dll"]