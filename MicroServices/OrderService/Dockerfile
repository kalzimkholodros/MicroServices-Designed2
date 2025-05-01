FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5193

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RabbitMQCommunication/RabbitMQCommunication.csproj", "RabbitMQCommunication/"]
COPY ["MicroServices/OrderService/OrderService.csproj", "MicroServices/OrderService/"]
RUN dotnet restore "MicroServices/OrderService/OrderService.csproj"
COPY . .
WORKDIR "/src/MicroServices/OrderService"
RUN dotnet build "OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.dll"] 