namespace StellaNowSDK.Messages;

public class Metadata
{
    public string Source { get; } = "external";
    public string? MessageId { get; set;  }
    public DateTime MessageProcessingDateUtc { get; set; }
    public DateTime MessageOriginDateUtc { get; set; }
    public string? EventTypeDefinitionId { get; set; }
    public List<EntityType>? EntityTypeIds { get; set; }
}