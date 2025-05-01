FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5043

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RabbitMQCommunication/RabbitMQCommunication.csproj", "RabbitMQCommunication/"]
COPY ["MicroServices/ProductService/ProductService.csproj", "MicroServices/ProductService/"]
RUN dotnet restore "MicroServices/ProductService/ProductService.csproj"
COPY . .
WORKDIR "/src/MicroServices/ProductService"
RUN dotnet build "ProductService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.dll"] 