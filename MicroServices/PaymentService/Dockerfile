FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5173

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RabbitMQCommunication/RabbitMQCommunication.csproj", "RabbitMQCommunication/"]
COPY ["MicroServices/PaymentService/PaymentService.csproj", "MicroServices/PaymentService/"]
RUN dotnet restore "MicroServices/PaymentService/PaymentService.csproj"
COPY . .
WORKDIR "/src/MicroServices/PaymentService"
RUN dotnet build "PaymentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"] 