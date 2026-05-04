FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PaymentService/PaymentService.csproj", "PaymentService/"]
COPY ["Ecommerce.Messaging/ECommerce.Messaging.csproj", "Ecommerce.Messaging/"]
RUN dotnet restore "PaymentService/PaymentService.csproj"

COPY . .
RUN dotnet publish "PaymentService/PaymentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "PaymentService.dll"]
