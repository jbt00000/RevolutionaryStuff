# CDC Entity Actor Framework — Implementation Plan

## Overview

Introduce a CDC (Change Data Capture) Entity Actor dispatch framework into the
`RevolutionaryStuff` library stack. The framework lets any application register
"actor" classes whose methods are automatically invoked when a matching
`JsonEntity` document arrives from a CDC-capable data store (initially Cosmos DB
Change Feed).

---

## Dependency Graph (existing, unchanged)

```
RevolutionaryStuff.Core
  └─ (no project references)

RevolutionaryStuff.Azure
  └─ Core

RevolutionaryStuff.Data.JsonStore
  └─ Core

RevolutionaryStuff.Data.Cosmos
  ├─ Azure
  └─ Core

RevolutionaryStuff.Data.JsonStore.Cosmos      ← all new Cosmos CDC code goes here
  ├─ Core
  ├─ Data.Cosmos
  └─ Data.JsonStore
```

**No project references are added or changed.** `RevolutionaryStuff.Data.JsonStore.Cosmos`
already has everything it needs. `RevolutionaryStuff.Data.Cosmos` is **not modified**.

---

## Project Placement

| Concern | Project |
|---|---|
| Storage-agnostic CDC abstractions (`IChangeDataCaptureEvent`, `IEntityActor`, `EntityActorAttribute`, DI extension) | `RevolutionaryStuff.Data.JsonStore` |
| Cosmos CDC contract (`ICosmosChangeDataCaptureEvent`), dispatcher (`CosmosChangeFeedEntityActorProcessor`) | `RevolutionaryStuff.Data.JsonStore.Cosmos` |
| Existing `ICosmosReceivedMessage` / `CosmosReceivedMessage` | **unchanged** in `RevolutionaryStuff.Data.Cosmos` |

---

## Files to Create

### 1. `src/RevolutionaryStuff.Data.JsonStore/Cdc/IChangeDataCaptureEvent.cs`

Storage-agnostic CDC event. `DocumentElement` uses `System.Text.Json.JsonElement`
which is a BCL type and introduces no project dependency.

```csharp
using System.Text.Json;

namespace RevolutionaryStuff.Data.JsonStore.Cdc;

/// <summary>
/// Represents a single document-level change event from any CDC-capable data store.
/// </summary>
public interface IChangeDataCaptureEvent
{
    /// <summary>The data type discriminator for the changed document (the "_jet" property value).</summary>
    string DataType { get; }

    /// <summary>The primary key of the changed document.</summary>
    string DocumentId { get; }

    /// <summary>The full changed document as a JsonElement.</summary>
    JsonElement DocumentElement { get; }

    /// <summary>Arbitrary properties extracted from the document or transport envelope.</summary>
    IDictionary<string, object> Properties { get; }
}
```

---

### 2. `src/RevolutionaryStuff.Data.JsonStore/Cdc/IEntityActor.cs`

```csharp
namespace RevolutionaryStuff.Data.JsonStore.Cdc;

/// <summary>
/// Marker interface for classes that contain methods which react to
/// JsonEntity CDC events. Methods are discovered via <see cref="EntityActorAttribute"/>.
/// </summary>
public interface IEntityActor
{
    /// <summary>
    /// Determines dispatch order when multiple actors respond to the same entity type.
    /// Lower values execute first. Default is 0.
    /// </summary>
    int Order => 0;
}
```

---

### 3. `src/RevolutionaryStuff.Data.JsonStore/Cdc/EntityActorAttribute.cs`

```csharp
namespace RevolutionaryStuff.Data.JsonStore.Cdc;

/// <summary>
/// Marks an instance method on an <see cref="IEntityActor"/> as a CDC handler.
/// The method must have the signature:
///   Task MethodName(TEntity entity)
/// where TEntity is a concrete subclass of <see cref="JsonEntity"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class EntityActorAttribute : Attribute { }
```

---

### 4. `src/RevolutionaryStuff.Data.JsonStore/Cdc/EntityActorServiceCollectionExtensions.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace RevolutionaryStuff.Data.JsonStore.Cdc;

public static class EntityActorServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="T"/> as both its concrete type and as
    /// <see cref="IEntityActor"/> for CDC dispatch.
    /// </summary>
    public static IServiceCollection AddEntityActor<T>(this IServiceCollection services)
        where T : class, IEntityActor
    {
        services.AddScoped<T>();
        services.AddScoped<IEntityActor, T>();
        return services;
    }
}
```

---

### 5. `src/RevolutionaryStuff.Data.JsonStore.Cosmos/Workers/ICosmosChangeDataCaptureEvent.cs`

Placed in `JsonStore.Cosmos` (not in `Data.Cosmos`) so it can reference both
`IChangeDataCaptureEvent` (from `JsonStore`) and `ICosmosReceivedMessage` (from `Data.Cosmos`)
without introducing any new project references.

```csharp
using RevolutionaryStuff.Data.Cosmos.Workers;
using RevolutionaryStuff.Data.JsonStore.Cdc;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Workers;

/// <summary>
/// A CDC event sourced from the Cosmos DB Change Feed.
/// Combines <see cref="IChangeDataCaptureEvent"/> with the existing
/// <see cref="ICosmosReceivedMessage"/> contract.
/// </summary>
public interface ICosmosChangeDataCaptureEvent : IChangeDataCaptureEvent, ICosmosReceivedMessage
{
}
```

---

### 6. `src/RevolutionaryStuff.Data.JsonStore.Cosmos/Workers/CosmosChangeFeedEntityActorProcessor.cs`

The dispatcher. Checks for `ICosmosReceivedMessage`, then reads `DataType` and
`DocumentId` directly from `DocumentElement` using `JsonEntity.JsonEntityPropertyNames`
constants (accessible because `JsonStore.Cosmos` already references `JsonStore`).
No modifications to `Data.Cosmos` are needed.

Key design points:
- Inherits `LoggingDisposableBase` (existing base class in `RevolutionaryStuff.Core`).
- Implements `IInboundMessageProcessor`.
- `GetProcessorInfosByDataType` caches reflection results per data-type string via `PermaCache`.
- Validates actor method signatures: `Task MethodName(TEntity)` where `TEntity : JsonEntity`.
- Uses `JsonEntity.GetDataType(Type)` for entity-type → data-type mapping.
- Uses `JsonHelpers.FromJsonElement<T>` for deserialization.
- Exposes `protected virtual Task OnPreDispatchAsync(ICosmosChangeDataCaptureEvent)` hook
  for subclasses to inject cross-cutting logic (e.g., tenant resolution).

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;
using RevolutionaryStuff.Core;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Core.Services.DependencyInjection;
using RevolutionaryStuff.Data.Cosmos.Workers;
using RevolutionaryStuff.Data.JsonStore.Cdc;
using RevolutionaryStuff.Data.JsonStore.Entities;
using System.Reflection;
using System.Text.Json;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.Workers;

[NamedService("cosmosChangeFeedEntityActorProcessor")]
public class CosmosChangeFeedEntityActorProcessor : LoggingDisposableBase, IInboundMessageProcessor
{
    private readonly IServiceProvider ServiceProvider;

    public CosmosChangeFeedEntityActorProcessor(
        IServiceProvider serviceProvider,
        ILogger<CosmosChangeFeedEntityActorProcessor> logger)
        : base(logger)
    {
        ServiceProvider = serviceProvider;
    }

    private record ProcessorInfo(
        Type BaseObjectType,
        MethodInfo Method,
        Type ActorEntityType,
        Func<JsonElement, object> Parser)
    {
        public override string ToString() => $"{BaseObjectType.Name}.{Method.Name}";
    }

    private IList<ProcessorInfo> GetProcessorInfosByDataType(string dataType)
        => PermaCache.FindOrCreate(
            Cache.CreateKey(GetType(), nameof(GetProcessorInfosByDataType), dataType),
            () =>
            {
                var items = new List<ProcessorInfo>();
                var poolOfActors = ServiceProvider.GetServices<IEntityActor>()
                    .OrderBy(z => z.Order).ThenBy(z => z.GetType().Name).OfType<object>();
                foreach (var p in poolOfActors)
                {
                    var pt = p.GetType();
                    foreach (var mi in pt.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                    {
                        var a = mi.GetCustomAttribute<EntityActorAttribute>();
                        if (a == null) continue;
                        var ps = mi.GetParameters();
                        if (mi.ReturnType == typeof(Task) && ps.Length == 1 && ps[0].ParameterType.IsA<JsonEntity>())
                        {
                            var actorEntityType = ps[0].ParameterType;
                            var actorDataType = JsonEntity.GetDataType(actorEntityType);
                            if (actorDataType != dataType) continue;
                            var parseMethodInfo = typeof(JsonHelpers)
                                .GetMethod(nameof(JsonHelpers.FromJsonElement), BindingFlags.Public | BindingFlags.Static)!
                                .MakeGenericMethod(actorEntityType);
                            items.Add(new(pt, mi, actorEntityType,
                                jel => parseMethodInfo.Invoke(null, [jel])!));
                        }
                        else
                        {
                            LogError("Method {method} in {type} has [EntityActor] but wrong signature. Expected: Task MethodName(TEntity) where TEntity : JsonEntity",
                                mi.Name, pt.Name);
                        }
                    }
                }
                return items;
            });

    /// <summary>
    /// Override to perform pre-dispatch work such as setting a tenant context.
    /// Called once per CDC event before any actors are invoked.
    /// </summary>
    protected virtual Task OnPreDispatchAsync(ICosmosChangeDataCaptureEvent cdcEvent)
        => Task.CompletedTask;

    async Task IInboundMessageProcessor.ProcessInboundMessageAsync(IInboundMessage msg)
    {
        if (msg is not ICosmosReceivedMessage cosmosMsg) return;

        var dataType = cosmosMsg.DocumentElement
            .TryGetProperty(JsonEntity.JsonEntityPropertyNames.DataType, out var dtEl)
            ? dtEl.GetString() ?? string.Empty
            : string.Empty;

        var id = cosmosMsg.DocumentElement
            .TryGetProperty(JsonEntity.JsonEntityPropertyNames.Id, out var idEl)
            ? idEl.GetString() ?? string.Empty
            : string.Empty;

        // Wrap in ICosmosChangeDataCaptureEvent for the pre-dispatch hook
        var cdcEvent = new CosmosChangeDataCaptureEventAdapter(dataType, id, cosmosMsg);
        await OnPreDispatchAsync(cdcEvent);

        var infos = GetProcessorInfosByDataType(dataType);
        Dictionary<Type, object>? actorByType = null;
        object? e = null;

        foreach (var info in infos)
        {
            try
            {
                e ??= info.Parser(cosmosMsg.DocumentElement);
                if (e == null) continue;

                actorByType ??= [];
                var actor = actorByType.FindOrCreate(info.BaseObjectType,
                    () => ServiceProvider.GetRequiredService(info.BaseObjectType));
                await (Task)info.Method.Invoke(actor, [e])!;
            }
            catch (Exception ex)
            {
                LogError("{processor} failed on document {documentId}: {ex}", info, id, ex);
            }
        }
#if DEBUG
        LogDebug("Document {id} dataType={dataType} matchedActors={count}", id, dataType, infos.Count);
#endif
    }

    // Private adapter so OnPreDispatchAsync receives a fully-typed event
    // without requiring CosmosReceivedMessage to implement ICosmosChangeDataCaptureEvent.
    private sealed class CosmosChangeDataCaptureEventAdapter(
        string dataType, string documentId, ICosmosReceivedMessage inner)
        : ICosmosChangeDataCaptureEvent
    {
        string IChangeDataCaptureEvent.DataType => dataType;
        string IChangeDataCaptureEvent.DocumentId => documentId;
        JsonElement IChangeDataCaptureEvent.DocumentElement => inner.DocumentElement;
        IDictionary<string, object> IChangeDataCaptureEvent.Properties => inner.Properties;
        string ICosmosReceivedMessage.DatabaseName => inner.DatabaseName;
        string ICosmosReceivedMessage.ContainerName => inner.ContainerName;
        JsonElement ICosmosReceivedMessage.DocumentElement => inner.DocumentElement;
    }
}
```

---

## Files NOT Modified

- `src/RevolutionaryStuff.Data.Cosmos/Workers/ICosmosReceivedMessage.cs` — **unchanged**
- `src/RevolutionaryStuff.Data.Cosmos/Workers/CosmosReceivedMessage.cs` — **unchanged**
- `src/RevolutionaryStuff.Data.Cosmos/RevolutionaryStuff.Data.Cosmos.csproj` — **unchanged**
- `src/RevolutionaryStuff.Data.JsonStore.Cosmos/RevolutionaryStuff.Data.JsonStore.Cosmos.csproj` — **unchanged**

---

## Version Bumps

All RSLLC assembly `.csproj` files bump from `4.148.100.0` → `4.149.100.0`
(`FileVersion`, `AssemblyVersion`, `Version`). Projects affected include (but are
not limited to):

- `RevolutionaryStuff.Data.JsonStore.csproj`
- `RevolutionaryStuff.Data.JsonStore.Cosmos.csproj`
- `RevolutionaryStuff.Data.Cosmos.csproj`
- All other RSLLC `.csproj` files at `4.148.x`

---

## Unit Tests

A new test class `CosmosChangeFeedEntityActorProcessorTests` will be created
in the appropriate existing test project. It will cover:

- Actor with matching entity type is dispatched.
- Actor with non-matching entity type is not dispatched.
- Multiple actors on the same entity type are dispatched in `Order` sequence.
- A malformed actor method signature logs an error and is skipped.
- `OnPreDispatchAsync` is called once per message with correct DataType and DocumentId.
- Non-`ICosmosReceivedMessage` messages are silently ignored.

---

## Checklist

- [ ] Create `IChangeDataCaptureEvent.cs` in `RevolutionaryStuff.Data.JsonStore/Cdc/`
- [ ] Create `IEntityActor.cs` in `RevolutionaryStuff.Data.JsonStore/Cdc/`
- [ ] Create `EntityActorAttribute.cs` in `RevolutionaryStuff.Data.JsonStore/Cdc/`
- [ ] Create `EntityActorServiceCollectionExtensions.cs` in `RevolutionaryStuff.Data.JsonStore/Cdc/`
- [ ] Create `ICosmosChangeDataCaptureEvent.cs` in `RevolutionaryStuff.Data.JsonStore.Cosmos/Workers/`
- [ ] Create `CosmosChangeFeedEntityActorProcessor.cs` in `RevolutionaryStuff.Data.JsonStore.Cosmos/Workers/`
- [ ] Create unit tests for `CosmosChangeFeedEntityActorProcessor`
- [ ] Bump versions in all RSLLC `.csproj` files to `4.149.100.0`
- [ ] Verify solution builds with zero errors
