namespace StellaNowSDK.Config;

public record StellaNowConfig
{
    public string ApiKey { get; init; }
    public string ApiSecret { get; init; }
    public string OrganizationId { get; init; }
    public string ProjectId { get; init; }
    public string ClientId { get; init; }
}