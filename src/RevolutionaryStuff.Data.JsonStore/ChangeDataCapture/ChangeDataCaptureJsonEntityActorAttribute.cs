namespace RevolutionaryStuff.Data.JsonStore.ChangeDataCapture;

[AttributeUsage(AttributeTargets.Method)]
public class ChangeDataCaptureJsonEntityActorAttribute : Attribute
{
    public int Order { get; set; }
    public ChangeDataCaptureJsonEntityActorAttribute(int order = 0)
    {
        Order = order;
    }
}
