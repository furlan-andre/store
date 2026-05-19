FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Store.sln", "./"]
COPY ["src/Store.Domain/Store.Domain.csproj", "src/Store.Domain/"]
COPY ["src/Store.Application/Store.Application.csproj", "src/Store.Application/"]
COPY ["src/Store.Infrastructure/Store.Infrastructure.csproj", "src/Store.Infrastructure/"]
COPY ["src/Store.API/Store.API.csproj", "src/Store.API/"]

RUN dotnet restore "src/Store.API/Store.API.csproj"

COPY . .
RUN dotnet publish "src/Store.API/Store.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Store.API.dll"]
