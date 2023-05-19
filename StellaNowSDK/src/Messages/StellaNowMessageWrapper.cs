using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StellaNowSDK.Messages;

public abstract class StellaNowMessageWrapper
{
    public Metadata Metadata { get; }
    public List<Field> Fields { get; }
    
    protected StellaNowMessageWrapper(string eventTypeDefinitionId, List<EntityType> entityTypeIds)
    {
        Metadata = new Metadata
        {
            MessageId = Guid.NewGuid().ToString(),
            MessageOriginDateUtc = DateTime.UtcNow,
            EventTypeDefinitionId = eventTypeDefinitionId,
            EntityTypeIds = entityTypeIds
        };
        
        Fields = new List<Field>();
    }
    
    protected void AddField(string name, string value)
    {
        Fields.Add(new Field(name, value));
    }
    
    public override string ToString()
    {
        Metadata.MessageProcessingDateUtc = DateTime.UtcNow;
        
        var settings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        
        return Newtonsoft.Json.JsonConvert.SerializeObject(this, settings);
    }
}