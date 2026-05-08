using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RevolutionaryStuff.Azure.Services.Messaging.Inbound;
using RevolutionaryStuff.Core.Caching;
using RevolutionaryStuff.Data.Cosmos.BackgroundServices;
using RevolutionaryStuff.Data.JsonStore.ChangeDataCapture;
using RevolutionaryStuff.Data.JsonStore.Entities;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.ChangeDataCapture;

public class CosmosChangeDataCaptureJsonEntityEventProcessor : RevolutionaryStuffService, IInboundMessageProcessor
{
    public record CosmosChangeDataCaptureJsonEntityEventProcessorConstructorArgs(
        IServiceProvider ServiceProvider,
        RevolutionaryStuffServiceConstrutorArgs BaseConstructorArgs)
    { }

    private readonly IServiceProvider ServiceProvider;

    public CosmosChangeDataCaptureJsonEntityEventProcessor(CosmosChangeDataCaptureJsonEntityEventProcessorConstructorArgs constructorArgs)
        : base(constructorArgs.BaseConstructorArgs)
    {
        ServiceProvider = constructorArgs.ServiceProvider;
    }

    private record ProcessorInfo(Type BaseObjectType, MethodInfo Method, Type ActorEntityType, Func<JsonElement, object> Parser)
    {
        public override string ToString() => $"{BaseObjectType.Name}.{Method.Name}";
    }

    private IList<ProcessorInfo> GetProcessorInfosByDataType(string dataType)
        => PermaCache.FindOrCreate(
            Cache.CreateKey(GetType(), nameof(GetProcessorInfosByDataType), dataType),
            () =>
            {
                var items = new List<ProcessorInfo>();
                var poolOfActors = ServiceProvider.GetServices<IChangeDataCaptureJsonEntityController>()
                    .OrderBy(z => z.Order).ThenBy(z => z.GetType().Name).OfType<object>();
                foreach (var p in poolOfActors)
                {
                    var pt = p.GetType();
                    foreach (var mi in pt.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                    {
                        var a = mi.GetCustomAttribute<ChangeDataCaptureJsonEntityActorAttribute>();
                        if (a == null) continue;
                        var ps = mi.GetParameters();
                        if (mi.ReturnType == typeof(Task) && ps.Length == 1 && ps[0].ParameterType.IsA<JsonEntity>())
                        {
                            var actorEntityType = ps[0].ParameterType;
                            var actorDataType = JsonEntity.GetDataType(actorEntityType);
                            if (actorDataType != dataType) continue;
                            var parseMethodInfo = typeof(JsonHelpers)
                                .GetMethod(nameof(JsonHelpers.FromJsonElement), BindingFlags.Public | BindingFlags.Static)
                                !.MakeGenericMethod(actorEntityType);
                            items.Add(new(pt, mi, actorEntityType,
                                (JsonElement jel) => parseMethodInfo.Invoke(null, [jel])!));
                        }
                        else
                        {
                            LogError($"Method {mi.Name} in {pt.Name} has [ChangeDataCaptureJsonEntityActor] but wrong signature. Expected: Task MethodName(TEntity) where TEntity : JsonEntity");
                        }
                    }
                }
                return items;
            });

    /// <summary>
    /// Override to perform pre-dispatch work such as setting a tenant context.
    /// Called once per CDC event, before any actors are invoked.
    /// </summary>
    protected virtual Task OnPreDispatchAsync(ICosmosInboundMessage cmsg)
        => Task.CompletedTask;

    protected virtual string GetId(ICosmosInboundMessage cmsg)
        => cmsg.GetPropertyVal<string>(JsonEntity.JsonEntityPropertyNames.Id);

    protected virtual string GetDataType(ICosmosInboundMessage cmsg)
        => cmsg.GetPropertyVal<string>(JsonEntity.JsonEntityPropertyNames.DataType);

    async Task IInboundMessageProcessor.ProcessInboundMessageAsync(IInboundMessage msg)
    {
        if (msg is not ICosmosInboundMessage cmsg) return;

        await OnPreDispatchAsync(cmsg);

        var id = GetId(cmsg);
        var dataType = GetDataType(cmsg);
        var infos = GetProcessorInfosByDataType(dataType);

        Dictionary<Type, object>? actorByType = null;
        object e = null;

        foreach (var info in infos)
        {
            try
            {
                e ??= info.Parser(cmsg.DocumentElement);
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
}
