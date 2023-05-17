namespace StellaNowSDK.Models;

public abstract class StellaNowMessageWrapper
{
    public string Topic { get; }
    public string Message { get; }

    protected StellaNowMessageWrapper(string topic, string message)
    {
        Topic = topic;
        Message = message;
    }
}