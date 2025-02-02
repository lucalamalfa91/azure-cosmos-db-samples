using Humanizer;
using Microsoft.Azure.Cosmos;

namespace azure_cosmos_db_samples;

public static class CosmosHandler
{
    
    private static readonly CosmosClient Client;

    static CosmosHandler()
    {
        Client = new CosmosClient(
            accountEndpoint: "https://msdocs-140042544.documents.azure.com:443/", 
            authKeyOrResourceToken: "QWkSeYOL2xrstEpGRu4kP6tqI0uXNIPOAkUQMKUctCuiKae4IprfksfUSMRABtJYSDY23bkg0Se2ACDbucMIEg=="
        );
    }
    
    public static async Task InitializeDatabaseAsync()
    {
        var database = await Client.CreateDatabaseIfNotExistsAsync("cosmicworks");
        Console.WriteLine($"Database 'cosmicworks' created with status code {database.StatusCode}");
    }
    
    private static async Task<Container> GetOrCreateContainerAsync()
    {
        var database = Client.GetDatabase("cosmicworks");
        ContainerProperties properties = new(
            id: "customers",
            partitionKeyPaths: new List<string> { "/address/country", "/address/state" }
        );    
        var containerResponse = await database.CreateContainerIfNotExistsAsync(properties);
        Console.WriteLine($"Container 'customers' ready with status code {containerResponse.StatusCode}");
        return containerResponse.Container;
    }

    public static async Task AddCustomerAsync(string name, string email, string state, string country)
    {
        var container = await GetOrCreateContainerAsync();
        var id = name.Kebaberize();
        var partitionKey = new PartitionKeyBuilder()
            .Add(country)
            .Add(state)
            .Build();

        var customer = new {
            id = id,
            name,
            email,
            address = new { state, country }
        };
        var shippingInfo = new {
            id = $"{id}-shipping",
            customerId = id,
            location = $"{state}, {country}",
            address = new { state, country }
        };
        var cart = new {
            id = $"{id}-cart",
            customerId = id,
            address = new { state, country },
            items = new { }
        };

        var batch = container.CreateTransactionalBatch(partitionKey)
            .CreateItem(customer)
            .CreateItem(shippingInfo)
            .CreateItem(cart);

        using var response = await batch.ExecuteAsync();
        Console.WriteLine($"Customer '{name}' shipping and cart address added with status code {response.StatusCode}");
    }
    
    public static async Task UpdateCustomerEmailAsync(string id, string email, string state, string country)
    {
        var container = await GetOrCreateContainerAsync();
        var partitionKey = new PartitionKeyBuilder()
            .Add(country)
            .Add(state)
            .Build();
        
        var customerResponse = await container.ReadItemAsync<dynamic>(id, partitionKey);
        var customer = customerResponse.Resource;
        customer.email = email;
        var response = await container.ReplaceItemAsync(customer, id);
        Console.WriteLine($"Updated customer '{id}' email with status code {response.StatusCode}");
    }
    
    public static async Task DeleteCustomerAsync(string id, string state, string country)
    {
        var container = await GetOrCreateContainerAsync();
        var partitionKey = new PartitionKeyBuilder()
            .Add(country)
            .Add(state)
            .Build();

        try
        {
            // Creiamo una transaction batch per eliminare tutti i documenti correlati
            var batch = container.CreateTransactionalBatch(partitionKey)
                .DeleteItem(id)                     // Elimina il cliente
                .DeleteItem($"{id}-shipping")       // Elimina l'indirizzo di spedizione
                .DeleteItem($"{id}-cart");          // Elimina il carrello

            using var response = await batch.ExecuteAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Successfully deleted customer '{id}' and all dependencies.");
            }
            else
            {
                Console.WriteLine($"Failed to delete customer '{id}'. Status Code: {response.StatusCode}");
            }
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"Error deleting customer '{id}': {ex.Message}");
        }
    }
    
    public static async Task UpdateProductToCartAsync(string name, string state, string country, string product)
    {
        var container = await GetOrCreateContainerAsync();
        var id = name.Kebaberize();
        var cartId = $"{id}-cart";

        var cart = new {
            id = $"{id}-cart",
            customerId = id,
            address = new { state, country },
            items = new { product }
        };

        await container.UpsertItemAsync(cart);
        Console.WriteLine($"Product '{product}' added to cart for customer '{name}'");
    }   
}