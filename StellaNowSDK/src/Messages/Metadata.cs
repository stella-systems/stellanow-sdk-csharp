namespace StellaNowSDK.Messages;

public class Metadata
{
    public string Source { get; } = "external";
    public string MessageId { get; set;  }
    public DateTime MessageProcessingDateUTC { get; set; }
    public DateTime MessageOriginDateUTC { get; set; }
    public string EventTypeDefinitionId { get; set; }
    public List<EntityType> EntityTypeIds { get; set; }
}