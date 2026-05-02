using RevolutionaryStuff.Data.Cosmos.BackgroundServices;
using RevolutionaryStuff.Data.JsonStore.ChangeDataCapture;

namespace RevolutionaryStuff.Data.JsonStore.Cosmos.ChangeDataCapture;

public interface ICosmosChangeDataCaptureJsonEntityEvent : IChangeDataCaptureJsonEntityEvent, ICosmosInboundMessage
{
}
