[![Build and Publish to NuGet](https://github.com/stella-systems/stellanow-sdk-csharp/actions/workflows/publish.yml/badge.svg)](https://github.com/stella-systems/stellanow-sdk-csharp/actions/workflows/publish.yml)
![NuGet](https://img.shields.io/nuget/v/StellaNowSDK.svg?style=flat-square)
![NuGet Downloads](https://img.shields.io/nuget/dt/StellaNowSDK.svg?style=flat-square)

# StellaNowSDK
## Introduction
Welcome to the StellaNow C# SDK. This SDK is designed to provide an easy-to-use interface for developers integrating their C# applications with the StellaNow Platform. The SDK communicates with the StellaNow Platform using the MQTT protocol over secure WebSockets.

## Key Features
* Automated connection handling (connection, disconnection and reconnection)
* Message queuing to handle any network instability
* Authentication management (login and automatic token refreshing)
* Easy interface to send different types of messages
* Per-message callbacks for notification of successful message sending
* Extensibility options for more specific needs

## Getting Started
Before you start integrating the SDK, ensure you have a Stella Now account.

## Installation
To integrate the StellaNowSDK into your project, follow these steps:

### Via NuGet Package Manager
The easiest way to add the StellaNowSDK to your project is through NuGet. You can do so directly within your project's development environment (like Visual Studio) using the NuGet Package Manager.

Search for `StellaNowSDK` and install it by following your environment's standard installation process.

### Via .NET CLI
Alternatively, you can use the .NET Core command-line interface (CLI) to add the StellaNowSDK package to your project. Run the following command in your terminal:

```bash
dotnet add package StellaNowSDK
```

## Configuration
The SDK supports multiple authentication strategies when connecting to the Stella Now MQTT Sink. You can configure the SDK using OIDC authentication or No authentication, depending on your environment.

### Setting Up StellaNow SDK
To use the SDK, first, ensure you have set the necessary environment variables:
* USERNAME
* PASSWORD
* ORGANIZATION_ID
* PROJECT_ID

* Then, register the SDK with the appropriate authentication strategy.

### Using OIDC Authentication

To authenticate with `StellaNow's` OIDC (OpenID Connect), use `AddStellaNowSdkWithMqttAndOidcAuth`:

```csharp
services.AddStellaNowSdkWithMqttAndOidcAuth(
    new StellaNowDevEnvironmentConfig(),
    new StellaNowConfig(
        OrganizationId = Environment.GetEnvironmentVariable("ORGANIZATION_ID")!,
        ProjectId = Environment.GetEnvironmentVariable("PROJECT_ID")!
    ),
    new OidcAuthCredentials(
        Environment.GetEnvironmentVariable("API_KEY")!,
        Environment.GetEnvironmentVariable("API_SECRET")!
    )
);
```

This will:
* Authenticate with OIDC using the provided username and password, using a specific OIDC Client designed for data ingestion.
* Resulting token will be used in MQTT broker authentication with specific claim.
* Connect to the MQTT sink securely.

### Using No Authentication
For local development or scenarios where authentication is not required, use `AddStellaNowSdkWithMqttAndNoAuth`:

```csharp
services.AddStellaNowSdkWithMqttAndNoAuth(
    new StellaNowDevEnvironmentConfig(),
    new StellaNowConfig(
        OrganizationId = Environment.GetEnvironmentVariable("ORGANIZATION_ID")!,
        ProjectId = Environment.GetEnvironmentVariable("PROJECT_ID")!
    )
);
```

This will:
* Connect to the MQTT sink without authentication.
* Be useful for testing against local MQTT brokers like NanoMQ.

Ensure you have set the appropriate environment variables.

> **Note:**  The `ClientId` used in the `StellaNowConfig` must be unique per connection. If two connections use the same `ClientId`, the first connection will be dropped. To prevent conflicts, the SDK now automatically generates a unique `ClientId` at startup. You can find the generated value in the logs:

```csharp
[2025-02-21 17:39:07] info: StellaNowSDK.Sinks.Mqtt.ConnectionStrategy.OidcMqttConnectionStrategy[0]
SDK Client ID is "StellaNowSDK_zhwxTo2KQX"
```
> If needed, you can override this behavior by manually specifying a `ClientId` as part of `StellaNowConfig`.

## Sample Application
Here is a simple application that uses StellaNowSDK to send user login messages to the Stella Now platform.

This function is the main entry point for our demonstration.
The `RunAsync` function does a few things:

* It establishes a connection to StellaNow.
* It creates a series of `UserLoginMessage` messages and sends them using the StellaNow SDK.
* It checks and logs whether there are any messages left in the queue.
* Finally, it disconnects from StellaNow and disposes of the services.

```csharp
private async Task RunAsync(int? messageCount)
{
    // Establish connection
    await _stellaSdk.StartAsync();

    _logger.LogInformation("Press 'ENTER' to stop the application!");
    
    // Ensure connection is established
    await Task.Delay(TimeSpan.FromSeconds(5));

    var uuid = Guid.NewGuid().ToString();

    int i = 0;

    // Either send messages based on count or indefinitely
    while (!messageCount.HasValue || i < messageCount.Value)
    {
        if (_cts.IsCancellationRequested)
        {
            break; // Exit the loop if cancellation is requested
        }
        
        var message = new UserDetailsMessage(
            uuid,
            uuid,
            new PhoneNumberModel(
                44,
                753594
                )
        );

        // Send the message
        _stellaSdk.SendMessage(message, OnMessageSentAction);
        _logger.LogInformation("New Message Queued");

        _logger.LogInformation(
            "Queue has messages {HasMessages} and count is {Count}",
            _stellaSdk.HasMessagesPendingForDispatch(),
            _stellaSdk.MessagesPendingForDispatchCount()
        );
        
        // Delay between messages
        await Task.Delay(TimeSpan.FromSeconds(1));
        
        i++;
    }

    // Disconnect and terminate
    await _stellaSdk.StopAsync();
    DisposeServices();
}
```

Below is the function to register services for the application.

`RegisterServices` function registers services needed for the application, such as logging and StellaNow SDK. It then builds the service provider from the service collection.

```csharp
private static void RegisterServices()
{
    // Create a new instance of ServiceCollection to register application services.
    var services = new ServiceCollection();

    // Configure logging to use a simple console logger and set the minimum log level.
    services.AddLogging(builder =>
    {
        builder.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
        });
        builder.SetMinimumLevel(LogLevel.Debug);
    });
    
    services.AddTransient<Program>();

    // Register StellaNowSdk with necessary configurations and environment.
    services.AddStellaNowSdkWithMqttAndOidcAuth(
        new StellaNowProdEnvironmentConfig(),
        new StellaNowConfig(
            Environment.GetEnvironmentVariable("ORGANIZATION_ID")!,
            Environment.GetEnvironmentVariable("PROJECT_ID")!
        ),
        new OidcAuthCredentials(
            Environment.GetEnvironmentVariable("OIDC_USERNAME"),
            Environment.GetEnvironmentVariable("OIDC_PASSWORD")!
        )
    );

    // Build the service provider from the service collection.
    _serviceProvider = services.BuildServiceProvider();
}
```

This is the callback function that gets called when a message is successfully sent.
The `OnMessageSentAction` function logs the information that a message was sent successfully, along with the message's ID.

```csharp
private void OnMessageSentAction(StellaNowEventWrapper message)
{
    _logger.LogInformation(
        "Send Confirmation: {MessagesId}",
        message.Value.Metadata.MessageId
    );
}
```

The full code for sample applications can be found here: 

* [Basic Application](StellaNowSDKDemo/src/Program.cs)

### IMPORTANT

> **`StopAsync` METHOD USAGE**
>
> The `StopAsync` method in the StellaNowSdk can be used with or without parameters. When it is used with the default parameters (i.e., `StopAsync()`), it immediately stops the processing of the message queue and triggers the `OnDisconnected` event.
>
> This behavior can have important consequences when you use non-persistent queue implementations. Any messages that have been added to the queue but not yet sent will remain in the queue, unsent, until `StartAsync` is called again.
>
> However, if your application is shut down before `StartAsync` is called again, those unsent messages will be lost, because non-persistent queues do not store their contents when the application is terminated.
>
> If it is important for your application to ensure that all queued messages are sent before the `OnDisconnected` event is triggered, you should call `StopAsync` with the `waitForEmptyQueue` parameter set to `true`. This will cause `StopAsync` to delay the triggering of the `OnDisconnected` event until the queue is empty, or until a specified timeout period has elapsed.
>
> Be aware that this does not delay the shutdown of your application. The application can be shut down independently by the developer or system, regardless of the state of the `StopAsync` method or message queue.
>
> Here is an example of how to call `StopAsync` in this way:
>
> ```csharp
> await sdk.StopAsync(waitForEmptyQueue: true);
> ```
>
> In this example, `StopAsync` will wait indefinitely until the queue is empty before triggering `OnDisconnected`. If you want to specify a maximum waiting time, you can use the `timeout` parameter:
>
> ```csharp
> await sdk.StopAsync(waitForEmptyQueue: true, timeout: TimeSpan.FromSeconds(30));
> ```
>
> In this example, `StopAsync` will wait until the queue is empty or until 30 seconds have elapsed, whichever happens first, before triggering `OnDisconnected`.

## Message Formatting
Messages in StellaNowSDK are wrapped in a `StellaNowMessageWrapper` and each specific message type extends this class to define its own properties. Each message needs to follow a certain format, including a type, list of entities, and optional fields. Here is an example:

```csharp
public record UserLoginStellaNowMessage(
    [property: Newtonsoft.Json.JsonIgnore] string EntityId, 
    [property: Newtonsoft.Json.JsonProperty("patron_id")] string PatronId, 
    [property: Newtonsoft.Json.JsonProperty("timestamp")] DateTime Timestamp, 
    [property: Newtonsoft.Json.JsonProperty("user_group_id")] int UserGroupId
    ) : StellaNowMessageBase("user_login", new List<EntityType>{ new EntityType("patron", EntityId) });
```

Creating these classes by hand can be prone to errors. Therefore, we provide a command line interface (CLI) tool, **StellaNow CLI**, to automate this task. This tool generates the code for the message classes automatically based on the configuration defined in the Operators Console.

You can install **StellaNow CLI** tool using pip, which is a package installer for Python. It is hosted on Python Package Index (PyPI), a repository of software for the Python programming language. To install it, open your terminal and run the following command:

```bash
pip install stellanow-cli
```

Once you have installed the **StellaNow CLI** tool, you can use it to generate message classes. Detailed instructions on how to use the **StellaNow CLI** tool can be found in the tool's documentation.

Please note that it is discouraged to write these classes yourself. Using the CLI tool ensures that the message format aligns with the configuration defined in the Operators Console and reduces the potential for errors.

## Customization

StellaNowSDK provides extensive flexibility for developers to adapt the SDK to their specific needs. You can extend key components, including message queuing strategies, sinks (where messages are sent), connection strategies, and authentication mechanisms.

### Customizing the Message Queue Strategy
By default, `StellaNowSDK` uses an in-memory queue to temporarily hold messages before sending them to a sink. These non-persistent queues will lose all messages if the application terminates unexpectedly.

If your application requires a persistent queue that survives restarts or crashes, you can implement a custom queue strategy by extending `IMessageQueueStrategy` and integrating it with a database, file system, or distributed queue.

#### Using a Custom Queue Strategy

```csharp
services.AddSingleton<IMessageQueueStrategy, YourCustomQueueStrategy>();
```

Replace YourCustomQueueStrategy with your custom queue implementation.

>⚠️ **Performance Considerations:** Persistent queues introduce additional latency and require careful design to balance reliability and performance.

#### Adding a Custom Sink & Connection Strategy
A sink is where messages are ultimately delivered. StellaNowSDK supports MQTT-based sinks, but you can extend this to support Kafka, Webhooks, Databases, or any custom integration.

Each sink is paired with a connection strategy, which determines how it establishes a connection. You can implement a custom connection strategy for different protocols or authentication mechanisms.

##### Example: Adding a Custom Sink
To add a new sink, implement `IStellaNowSink` and register it:

```csharp
services.AddSingleton<IStellaNowSink, YourCustomSink>();
```

##### Example: Custom Connection Strategy for a Sink 
Each sink needs a connection strategy, implemented using `IMqttConnectionStrategy` (for MQTT) or a custom interface:

```csharp
services.AddSingleton<IMqttConnectionStrategy, YourCustomConnectionStrategy>();
```

#### Extending the Authentication Service
StellaNowSDK supports multiple authentication mechanisms, including:

* OIDC-based authentication
* No authentication

If your project requires a new authentication mechanism for sink of your choice, implement `IStellaNowAuthenticationService` and register it.

##### Example: Adding a Custom Authentication Service

```csharp
services.AddSingleton<IStellaNowAuthenticationService, YourCustomAuthService>();
```

> ⚠️ **Security Considerations:** Ensure your authentication mechanism is properly secured and does not expose credentials in logs or configuration files.

## Support
For any issues or feature requests, feel free to create a new issue on our GitHub repository. If you need further assistance, contact our support team at help@stella.systems.

## Documentation
Detailed documentation will be available soon.

## License
This project is licensed under the terms of the MIT license.