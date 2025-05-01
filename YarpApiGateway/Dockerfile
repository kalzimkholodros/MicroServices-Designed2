FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5223

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["YarpApiGateway/YarpApiGateway.csproj", "YarpApiGateway/"]
COPY ["RabbitMQCommunication/RabbitMQCommunication.csproj", "RabbitMQCommunication/"]
RUN dotnet restore "YarpApiGateway/YarpApiGateway.csproj"
COPY . .
WORKDIR "/src/YarpApiGateway"
RUN dotnet build "YarpApiGateway.csproj" -c Release -o /app/build /p:UseAppHost=false

FROM build AS publish
RUN dotnet publish "YarpApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YarpApiGateway.dll"] 