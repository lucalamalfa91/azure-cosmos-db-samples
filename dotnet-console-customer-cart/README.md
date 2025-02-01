# Cosmos DB Customer Management CLI

## Prerequisites

- **.NET SDK** installed
- **Azure Cosmos DB account**
- Configure Cosmos DB connection details in `CosmosHandler` class

## Setup

### Restore dependencies
```sh
dotnet restore
```

### Build the project
```sh
dotnet build
```

## Commands

### Initialize Database
```sh
dotnet run -- initialize
```

### Add a Customer
```sh
dotnet run -- addCustomer --name "John Doe" --email "john@example.com" --state "CA" --country "USA"
```

### Add a Product to Cart
```sh
dotnet run -- addProduct --name "John Doe" --state "CA" --country "USA" --product "Laptop"
```

### Update Customer Email
```sh
dotnet run -- updateCustomer --id "john-doe" --email "new-email@example.com"
```

### Remove a Product from Cart
```sh
dotnet run -- removeProduct --name "John Doe" --product "Laptop" --country "USA"
```

### Delete Customer
```sh
dotnet run -- deleteCustomer --id "john-doe" --country "USA"
```

## Features

- **Customer management** (Add, Update, Delete)
- **Shopping cart and shipping address management**
- **Cosmos DB integration with partitioning support**