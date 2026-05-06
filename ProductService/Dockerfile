FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ProductService/ProductService.csproj", "ProductService/"]
COPY ["Ecommerce.Messaging/ECommerce.Messaging.csproj", "Ecommerce.Messaging/"]
RUN dotnet restore "ProductService/ProductService.csproj"

COPY . .
RUN dotnet publish "ProductService/ProductService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductService.dll"]
