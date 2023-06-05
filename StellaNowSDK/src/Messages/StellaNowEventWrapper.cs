using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StellaNowSDK.Messages;

public sealed class StellaNowEventWrapper
{
    public EventKey Key { get; set; }
    
    public StellaNowMessageWrapper Value { get; set; }

    public StellaNowEventWrapper(EventKey key, StellaNowMessageWrapper value)
    {
        Key = key;
        Value = value;
    }
    
    public override string ToString()
    {
        var settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        return JsonConvert.SerializeObject(this, settings);
    }

    public string GetForDispatch()
    {
        Value.SetDispatchTime();
        return ToString();
    }
}