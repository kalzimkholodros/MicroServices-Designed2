FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5228

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RabbitMQCommunication/RabbitMQCommunication.csproj", "RabbitMQCommunication/"]
COPY ["MicroServices/BasketService/BasketService.csproj", "MicroServices/BasketService/"]
RUN dotnet restore "MicroServices/BasketService/BasketService.csproj"
COPY . .
WORKDIR "/src/MicroServices/BasketService"
RUN dotnet build "BasketService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BasketService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BasketService.dll"] 