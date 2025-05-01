#!/bin/bash

# Wait for PostgreSQL to be ready
echo "Waiting for PostgreSQL to be ready..."
while ! pg_isready -U postgres; do
  sleep 1
done

echo "PostgreSQL is ready!"

# Create databases if they don't exist
echo "Creating databases..."
psql -U postgres -c "CREATE DATABASE ProductDb;" || true
psql -U postgres -c "CREATE DATABASE OrderDb;" || true
psql -U postgres -c "CREATE DATABASE PaymentDb;" || true

echo "Databases created successfully!"

# Run migrations (if you have any)
# echo "Running migrations..."
# dotnet ef database update --project microservices/ProductService/ProductService.csproj
# dotnet ef database update --project microservices/OrderService/OrderService.csproj
# dotnet ef database update --project microservices/PaymentService/PaymentService.csproj

echo "Database initialization completed!" 