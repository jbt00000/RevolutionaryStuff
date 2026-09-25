using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RevolutionaryStuff.Data.Cosmos.Tests;

internal enum CosmosEnumSerializationTestStatus
{
    [JsonStringEnumMemberName("ready-for-review")]
    ReadyForReview
}

internal class CosmosEnumSerializationTestEntity
{
    public CosmosEnumSerializationTestStatus Status { get; set; }
}

[TestClass]
public class CosmosEnumSerializationTests
{
    [TestMethod]
    public void DefaultCosmosSerializer_PreservesCustomEnumString()
    {
        var serializer = new DefaultCosmosEntitySerializer();
        var entity = new CosmosEnumSerializationTestEntity { Status = CosmosEnumSerializationTestStatus.ReadyForReview };

        using var stream = serializer.ToStream(entity);
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        using var jsonDocument = JsonDocument.Parse(json);

        Assert.AreEqual("ready-for-review", jsonDocument.RootElement.GetProperty(nameof(entity.Status)).GetString());

        using var readStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var deserialized = serializer.FromStream<CosmosEnumSerializationTestEntity>(readStream);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(entity.Status, deserialized.Status);
    }
}
