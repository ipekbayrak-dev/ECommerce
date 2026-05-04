FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OrderService/OrderService.csproj", "OrderService/"]
COPY ["Ecommerce.Messaging/ECommerce.Messaging.csproj", "Ecommerce.Messaging/"]
RUN dotnet restore "OrderService/OrderService.csproj"

COPY . .
RUN dotnet publish "OrderService/OrderService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "OrderService.dll"]
