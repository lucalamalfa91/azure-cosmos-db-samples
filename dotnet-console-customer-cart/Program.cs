using System.CommandLine;
using azure_cosmos_db_samples;

var rootCommand = new RootCommand("Cosmos DB Customer Management CLI");

// Initialize Database
var initializeCommand = new Command("initialize", "Initialize the Cosmos DB database");
initializeCommand.SetHandler(CosmosHandler.InitializeDatabaseAsync);
rootCommand.Add(initializeCommand);

// Add Customer
var addCustomerCommand = new Command("addCustomer", "Add a customer to the database");

var nameOption = new Option<string>("--name") { IsRequired = true };
var emailOption = new Option<string>("--email");
var stateOption = new Option<string>("--state") { IsRequired = true };
var countryOption = new Option<string>("--country") { IsRequired = true };

addCustomerCommand.AddOption(nameOption);
addCustomerCommand.AddOption(emailOption);
addCustomerCommand.AddOption(stateOption);
addCustomerCommand.AddOption(countryOption);

addCustomerCommand.SetHandler(
    CosmosHandler.AddCustomerAsync, 
    nameOption, 
    emailOption,
    stateOption,
    countryOption
);

rootCommand.Add(addCustomerCommand);

// Add Product to Cart
var addProductCommand = new Command("addProduct", "Add a product to the customer's cart");

var productOption = new Option<string>("--product") { IsRequired = true };

addProductCommand.AddOption(nameOption);
addProductCommand.AddOption(stateOption);
addProductCommand.AddOption(countryOption);
addProductCommand.AddOption(productOption);

addProductCommand.SetHandler(
    CosmosHandler.UpdateProductToCartAsync,
    nameOption,
    stateOption,
    countryOption,
    productOption
);

rootCommand.Add(addProductCommand);

// Update Customer Email
var updateCustomerCommand = new Command("updateCustomer", "Update customer email");

var idOption = new Option<string>("--id") { IsRequired = true };
var newEmailOption = new Option<string>("--email") { IsRequired = true };

updateCustomerCommand.AddOption(idOption);
updateCustomerCommand.AddOption(newEmailOption);
updateCustomerCommand.AddOption(stateOption);
updateCustomerCommand.AddOption(countryOption);

updateCustomerCommand.SetHandler(
    CosmosHandler.UpdateCustomerEmailAsync,
    idOption,
    newEmailOption,
    stateOption,
    countryOption
);

rootCommand.Add(updateCustomerCommand);

// Delete Customer
var deleteCustomerCommand = new Command("deleteCustomer", "Delete a customer");

deleteCustomerCommand.AddOption(idOption);
deleteCustomerCommand.AddOption(stateOption);
deleteCustomerCommand.AddOption(countryOption);

deleteCustomerCommand.SetHandler(
    CosmosHandler.DeleteCustomerAsync,
    idOption,
    stateOption,
    countryOption
);

rootCommand.Add(deleteCustomerCommand);

// Execute the command line
await rootCommand.InvokeAsync(args);