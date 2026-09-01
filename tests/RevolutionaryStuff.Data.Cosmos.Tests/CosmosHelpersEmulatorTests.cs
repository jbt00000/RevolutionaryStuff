using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Data.Cosmos.Tests;

[TestClass]
public sealed class CosmosHelpersEmulatorTests
{
    private const string ConnectionStringEnvironmentVariableName = "COSMOS_EMULATOR_CONNECTION_STRING";
    private static CosmosClient Client = null!;
    private static Database Database = null!;
    private static Container Container = null!;

    private sealed class AggregateItem
    {
        public string Id { get; init; }
        public string PartitionKey { get; init; }
        public int IntValue { get; init; }
        public int? NullableIntValue { get; init; }
    }

    [ClassInitialize]
    public static async Task ClassInitializeAsync(TestContext _)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariableName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Assert.Inconclusive($"Set {ConnectionStringEnvironmentVariableName} to run Cosmos emulator integration tests.");
        }

        try
        {
            Client = new CosmosClient(connectionString, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway,
                HttpClientFactory = () => new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                }),
                LimitToEndpoint = true,
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                },
            });
            Database = await Client.CreateDatabaseAsync($"cosmos-helpers-tests-{Guid.NewGuid():N}");
            Container = await Database.CreateContainerAsync("items", "/partitionKey");
            await Container.CreateItemAsync(new AggregateItem { Id = "1", PartitionKey = "aggregate", IntValue = 1, NullableIntValue = 1 });
            await Container.CreateItemAsync(new AggregateItem { Id = "2", PartitionKey = "aggregate", IntValue = 3, NullableIntValue = null });
            await Container.CreateItemAsync(new AggregateItem { Id = "3", PartitionKey = "other", IntValue = 8, NullableIntValue = 8 });
        }
        catch (Exception ex)
        {
            Client?.Dispose();
            Assert.Inconclusive($"Cosmos emulator is unavailable: {ex.Message}");
        }
    }

    [ClassCleanup]
    public static async Task ClassCleanupAsync()
    {
        if (Database != null)
        {
            await Database.DeleteAsync();
        }
        Client?.Dispose();
    }

    [TestMethod]
    [TestCategory("CosmosEmulator")]
    public async Task AggregateHelpers_WithCosmosQueryable_ReturnExpectedValues()
    {
        var query = Container
            .GetItemLinqQueryable<AggregateItem>()
            .Where(x => x.PartitionKey == "aggregate");

        Assert.AreEqual(2, await query.GetCountAsync());
        Assert.AreEqual(4, await query.GetSumAsync(x => x.IntValue));
        Assert.AreEqual(1, await query.GetSumAsync(x => x.NullableIntValue));
        Assert.AreEqual(2D, await query.GetAverageAsync(x => x.IntValue));
        Assert.AreEqual(1D, await query.GetAverageAsync(x => x.NullableIntValue));
        Assert.AreEqual(1, await query.GetMinAsync(x => x.IntValue));
        Assert.AreEqual(3, await query.GetMaxAsync(x => x.IntValue));
    }

    [TestMethod]
    [TestCategory("CosmosEmulator")]
    public async Task GetAllItemsAsync_WithCosmosQueryable_ReturnsFilteredEmptyAndMultipleResults()
    {
        var query = Container.GetItemLinqQueryable<AggregateItem>();

        var multiple = await query.Where(x => x.PartitionKey == "aggregate").GetAllItemsAsync();
        var filtered = await query.Where(x => x.Id == "3").GetAllItemsAsync();
        var empty = await query.Where(x => x.Id == "missing").GetAllItemsAsync();

        Assert.HasCount(2, multiple);
        Assert.HasCount(1, filtered);
        Assert.AreEqual("3", filtered.Single().Id);
        Assert.IsEmpty(empty);
    }
}
