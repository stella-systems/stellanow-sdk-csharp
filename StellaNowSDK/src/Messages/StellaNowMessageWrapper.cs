namespace StellaNowSDK.Messages;

public abstract class StellaNowMessageWrapper
{
    public Metadata Metadata { get; set; }
    public List<Field> Fields { get; set; }
    
    protected StellaNowMessageWrapper()
    {
        Metadata = new Metadata
        {
            MessageId = Guid.NewGuid().ToString(),
            MessageOriginDateUTC = DateTime.UtcNow
        };
    }
    
    public override string ToString()
    {
        Metadata.MessageProcessingDateUTC = DateTime.UtcNow;
        return Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }
}