using Newtonsoft.Json;

namespace RevolutionaryStuff.Data.JsonStore.Serialization.Json;

public class BlankTypeRemovingJsonWriter : JsonTextWriter
{
    public BlankTypeRemovingJsonWriter(StreamWriter sw)
        : base(sw)
    { }

    private const string TypePropertyName = "$type";
    public override void WritePropertyName(string name)
    {
        LastPropertyNameWasType = false;
        base.WritePropertyName(name);
    }

    public override void WritePropertyName(string name, bool escape)
    {
        if (name == "$type")
        {
            LastPropertyNameWasType = true;
        }
        else
        {
            LastPropertyNameWasType = false;
            base.WritePropertyName(name, escape);
        }
    }

    private bool LastPropertyNameWasType;
    public override void WriteValue(string value)
    {
        if (LastPropertyNameWasType)
        {
            LastPropertyNameWasType = false;
            var vt = value.Trim();
            if (vt is "" or ",")
            {
                return;
            }
            base.WritePropertyName(TypePropertyName, false);
        }
        base.WriteValue(value);
    }
}
