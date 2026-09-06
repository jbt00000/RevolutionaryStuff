using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Data.JsonStore.Cosmos.Repos;
using RevolutionaryStuff.Data.JsonStore.Cosmos.Services.CosmosJsonEntityServer;
using RevolutionaryStuff.Data.JsonStore.Entities;
using RevolutionaryStuff.Data.JsonStore.Repos;
using RevolutionaryStuff.Data.JsonStore.Store;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Tests.Store;

[TestClass]
public class CosmosJsonEntityContainerTests
{
    private const string ContainerId = "test-container";

    [JsonEntityAbbreviation("createTest")]
    [JsonEntityContainerId(ContainerId)]
    public sealed class TestEntity : JsonEntity
    {
        public List<string> PreparationSteps { get; } = [];
        public Exception PreparationException { get; set; }

        protected override void OnPreSave()
        {
            base.OnPreSave();
            PreparationSteps.Add("pre-save");
            PartitionKey = "prepared-key";
            if (PreparationException != null)
            {
                throw PreparationException;
            }
        }

        protected override void OnPreSave(IJsonEntityContainer container)
        {
            base.OnPreSave(container);
            PreparationSteps.Add("container-pre-save");
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            PreparationSteps.Add("validate");
        }
    }

    private sealed class TestContainer(Container container)
        : CosmosJsonEntityContainer(container, Options.Create(new CosmosJsonEntityServerConfig()), NullLogger.Instance)
    {
        protected override PartitionKey CreatePartitionKey(string partitionKey)
            => new("custom-" + partitionKey);
    }

    private static Mock<Container> CreateContainerMock(string containerId = ContainerId)
    {
        var container = new Mock<Container>(MockBehavior.Strict);
        container.SetupGet(z => z.Id).Returns(containerId);
        return container;
    }

    [TestMethod]
    public async Task CreateItemIfNotExistsAsync_PreparesItemAndForwardsCreateOptions()
    {
        var cosmos = CreateContainerMock();
        var entity = new TestEntity();
        using var cancellation = new CancellationTokenSource();
        var response = new Mock<ItemResponse<TestEntity>>();
        cosmos.Setup(z => z.CreateItemAsync(entity, new PartitionKey("custom-prepared-key"),
                It.Is<ItemRequestOptions>(o => o.EnableContentResponseOnWrite == false), cancellation.Token))
            .Callback<TestEntity, PartitionKey?, ItemRequestOptions, CancellationToken>((item, key, options, token) =>
                CollectionAssert.AreEqual(new[] { "pre-save", "container-pre-save", "validate" }, item.PreparationSteps))
            .ReturnsAsync(response.Object);
        using var container = new TestContainer(cosmos.Object);

        var created = await ((IJsonEntityContainer)container).CreateItemIfNotExistsAsync(entity, cancellation.Token);

        Assert.IsTrue(created);
        cosmos.Verify(z => z.CreateItemAsync(entity, new PartitionKey("custom-prepared-key"),
            It.IsAny<ItemRequestOptions>(), cancellation.Token), Times.Once);
    }

    [TestMethod]
    public async Task CreateItemIfNotExistsAsync_ConflictReturnsFalse()
    {
        var cosmos = CreateContainerMock();
        var entity = new TestEntity();
        cosmos.Setup(z => z.CreateItemAsync(entity, It.IsAny<PartitionKey?>(), It.IsAny<ItemRequestOptions>(), default))
            .ThrowsAsync(new CosmosException("Already exists", HttpStatusCode.Conflict, 0, "test", 1));
        using var container = new TestContainer(cosmos.Object);

        Assert.IsFalse(await ((IJsonEntityContainer)container).CreateItemIfNotExistsAsync(entity));
        cosmos.Verify(z => z.CreateItemAsync(entity, It.IsAny<PartitionKey?>(), It.IsAny<ItemRequestOptions>(), default), Times.Once);
    }

    [TestMethod]
    [DataRow(HttpStatusCode.BadRequest)]
    [DataRow(HttpStatusCode.Forbidden)]
    [DataRow(HttpStatusCode.TooManyRequests)]
    [DataRow(HttpStatusCode.InternalServerError)]
    public async Task CreateItemIfNotExistsAsync_NonConflictErrorsPropagate(HttpStatusCode statusCode)
    {
        var cosmos = CreateContainerMock();
        var entity = new TestEntity();
        var error = new CosmosException("Failure", statusCode, 0, "test", 1);
        cosmos.Setup(z => z.CreateItemAsync(entity, It.IsAny<PartitionKey?>(), It.IsAny<ItemRequestOptions>(), default))
            .ThrowsAsync(error);
        using var container = new TestContainer(cosmos.Object);

        var actual = await Assert.ThrowsAsync<CosmosException>(() => ((IJsonEntityContainer)container).CreateItemIfNotExistsAsync(entity));

        Assert.AreSame(error, actual);
    }

    [TestMethod]
    public async Task CreateItemIfNotExistsAsync_CancellationPropagates()
    {
        var cosmos = CreateContainerMock();
        var entity = new TestEntity();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        cosmos.Setup(z => z.CreateItemAsync(entity, It.IsAny<PartitionKey?>(), It.IsAny<ItemRequestOptions>(), cancellation.Token))
            .Returns(Task.FromCanceled<ItemResponse<TestEntity>>(cancellation.Token));
        using var container = new TestContainer(cosmos.Object);

        await Assert.ThrowsAsync<OperationCanceledException>(() => ((IJsonEntityContainer)container).CreateItemIfNotExistsAsync(entity, cancellation.Token));
    }

    [TestMethod]
    public async Task CreateItemIfNotExistsAsync_PreparationConflictIsNotSuppressed()
    {
        var cosmos = CreateContainerMock();
        var error = new CosmosException("Preparation failed", HttpStatusCode.Conflict, 0, "test", 0);
        var entity = new TestEntity { PreparationException = error };
        using var container = new TestContainer(cosmos.Object);

        var actual = await Assert.ThrowsAsync<CosmosException>(() => ((IJsonEntityContainer)container).CreateItemIfNotExistsAsync(entity));

        Assert.AreSame(error, actual);
        cosmos.Verify(z => z.CreateItemAsync(It.IsAny<TestEntity>(), It.IsAny<PartitionKey?>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateItemIfNotExistsAsync_WrongContainerDoesNotWrite()
    {
        var cosmos = CreateContainerMock("wrong-container");
        using var container = new TestContainer(cosmos.Object);
        var entity = new TestEntity();

        await Assert.ThrowsAsync<Exception>(() => ((IJsonEntityContainer)container).CreateItemIfNotExistsAsync(entity));

        Assert.AreEqual(0, entity.PreparationSteps.Count);
        cosmos.Verify(z => z.CreateItemAsync(It.IsAny<TestEntity>(), It.IsAny<PartitionKey?>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task CreateItemIfNotExistsAsync_RepositoryInterfacesDelegateToContainer(bool created)
    {
        var entity = new TestEntity();
        var container = new Mock<IJsonEntityContainer>(MockBehavior.Strict);
        container.SetupGet(z => z.ContainerId).Returns(ContainerId);
        container.Setup(z => z.CreateItemIfNotExistsAsync(entity, default)).ReturnsAsync(created);
        var server = new Mock<IJsonEntityServer>(MockBehavior.Strict);
        server.Setup(z => z.GetContainer<TestEntity>()).Returns(container.Object);
        var args = new JsonEntityRepoConstructorArgs(server.Object, Mock.Of<ILocalCacher>(),
            Options.Create(new JsonEntityRepoBaseConfig()),
            new RevolutionaryStuffService.RevolutionaryStuffServiceConstrutorArgs(NullLoggerFactory.Instance));
        var repo = new Mock<CosmosJsonEntityRepo<JsonEntity>>(new List<string> { ContainerId }, new CosmosRepoConstructorArgs(args)) { CallBase = true };

        Assert.AreEqual(created, await ((IJsonEntityRepo<JsonEntity>)repo.Object).CreateItemIfNotExistsAsync(entity));
        Assert.AreEqual(created, await ((ICosmosJsonEntityRepo<JsonEntity>)repo.Object).CreateItemIfNotExistsAsync(entity));
        container.Verify(z => z.CreateItemIfNotExistsAsync(entity, default), Times.Exactly(2));
    }
}
