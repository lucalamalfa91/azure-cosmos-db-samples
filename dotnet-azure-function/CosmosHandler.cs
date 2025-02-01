using Humanizer;
using Microsoft.Azure.Cosmos;

namespace azure_cosmos_db_samples;

public static class CosmosHandler
{
    
    private static readonly CosmosClient Client;

    static CosmosHandler()
    {
        Client = new CosmosClient(
            accountEndpoint: "<CosmosDBUrl>", 
            authKeyOrResourceToken: "CosmosDBPrimaryKey"
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
        var partitionKey = new PartitionKey(country);

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

        var batch = container.CreateTransactionalBatch(partitionKey)
            .CreateItem(customer)
            .CreateItem(shippingInfo);

        var response = await batch.ExecuteAsync();
        Console.WriteLine($"Customer '{name}' and shipping address added with status code {response.StatusCode}");
    }
    
    public static async Task UpdateCustomerEmailAsync(string id, string email)
    {
        var container = await GetOrCreateContainerAsync();
        var partitionKey = new PartitionKey(id);
        var customerResponse = await container.ReadItemAsync<dynamic>(id, partitionKey);
        var customer = customerResponse.Resource;
        customer.email = email;
        var response = await container.ReplaceItemAsync(customer, id, partitionKey);
        Console.WriteLine($"Updated customer '{id}' email with status code {response.StatusCode}");
    }
    
    public static async Task DeleteCustomerAsync(string id, string country)
    {
        var container = await GetOrCreateContainerAsync();
        var partitionKey = new PartitionKey(country);
        var response = await container.DeleteItemAsync<dynamic>(id, partitionKey);
        Console.WriteLine($"Deleted customer '{id}' with status code {response.StatusCode}");
    }
    
    public static async Task AddProductToCartAsync(string name, string state, string country, string product)
    {
        var container = await GetOrCreateContainerAsync();
        var id = name.Kebaberize();
        var cartId = $"{id}-cart";
        var partitionKey = new PartitionKey(country);

        var cart = new {
            id = cartId,
            customerId = id,
            items = new[] { product },
            address = new { state, country }
        };

        await container.UpsertItemAsync(cart);
        Console.WriteLine($"Product '{product}' added to cart for customer '{name}'");
    }   
    
    public static async Task RemoveProductFromCartAsync(string customerId, string productId, string country)
    {
        var container = await GetOrCreateContainerAsync();
        var cartId = $"{customerId}-cart";
        var partitionKey = new PartitionKey(country);

        // Fetch the current cart
        var cartResponse = await container.ReadItemAsync<dynamic>(cartId, partitionKey);
        var cart = cartResponse.Resource;

        // Remove the product from the items list if it exists
        var items = new List<string>(cart.items.ToObject<IEnumerable<string>>());
        if (items.Contains(productId))
        {
            items.Remove(productId);
            cart.items = items.ToArray();

            // If there are no items left in the cart, remove the cart entirely
            if (items.Count == 0)
            {
                await container.DeleteItemAsync<dynamic>(cartId, partitionKey);
                Console.WriteLine($"Cart for customer '{customerId}' is empty and has been removed.");
            }
            else
            {
                // Update the cart
                await container.ReplaceItemAsync(cart, cartId, partitionKey);
                Console.WriteLine($"Product '{productId}' removed from cart for customer '{customerId}'");
            }
        }
        else
        {
            Console.WriteLine($"Product '{productId}' not found in cart for customer '{customerId}'");
        }
    }
}