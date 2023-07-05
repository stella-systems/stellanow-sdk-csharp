# StellaNowSDK
## Introduction
Welcome to the StellaNow C# SDK. This SDK is designed to provide an easy-to-use interface for developers integrating their C# applications with the StellaNow Platform. The SDK communicates with the StellaNow Platform using the MQTT protocol over secure WebSockets.

## Key Features
* Automated connection handling (connection, disconnection and reconnection)
* Message queuing to handle any network instability
* Authentication management (login and automatic token refreshing)
* Easy interface to send different types of messages
* Extensibility options for more specific needs

## Getting Started
Before you start integrating the SDK, ensure you have a Stella Now account and valid API credentials which include **OrganizationId**, **ApiKey**, and **ApiSecret**.

## Installation
To install the SDK in your project:
* Clone the StellaNowSDK GitHub repository.
* Build the SDK.
* Add the built .dll file as a reference to your project.

## Configuration
You will need to provide necessary credentials to interact with the Stella Now platform:

    services.AddStellaNowSdk(
        StellaNowEnvironment.Integrations,
        new StellaNowConfig
        {
            ApiKey = "<Your-API-Key>",
            ApiSecret = "<Your-API-Secret>",
            ClientId = "<Your-Client-ID>",
            OrganizationId = "<Your-Organization-ID>",
            ProjectId = "<Your-Project-ID>"
        }
    );

Replace **<Your-API-Key>**, **<Your-API-Secret>**, **<Your-Client-ID>**, **<Your-Organization-ID>**, and **<Your-Project-ID>** with your respective Stella Now credentials.

## Sample Application
Here is a simple application that uses StellaNowSDK to send user login messages to the Stella Now platform:

    internal class Program
    {
    private static IServiceProvider _serviceProvider;
    
        static async Task Main(string[] args)
        {
            RegisterServices();
    
            var stellaSdk = _serviceProvider.GetRequiredService<IStellaNowSdk>();
            var logger = _serviceProvider.GetRequiredService<ILogger<Program>>();
    
            // Establish connection
            await stellaSdk.StartAsync();
    
            // Ensure connection is established
            await Task.Delay(TimeSpan.FromSeconds(5));
    
            // Send 5 messages
            for (int i = 0; i < 5; i++)
            {
                var uuid = Guid.NewGuid().ToString();
    
                var message = new UserLoginMessage(
                    uuid, uuid,
                    DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                );
    
                // Send the message
                stellaSdk.SendMessage(message);
                logger.LogInformation("New Message Queued");
    
                // Delay between messages
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
    
            // Disconnect and terminate
            await stellaSdk.StopAsync();
            DisposeServices();
        }
    
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
    
            // Register StellaNowSdk with necessary configurations and environment.
            services.AddStellaNowSdk(
                StellaNowEnvironment.Integrations,
                new StellaNowConfig
                {
                    ApiKey = "username10@some.domain",
                    ApiSecret = "1234567890",
                    ClientId = "StellaNowCsharpSDK",
                    OrganizationId = "62dbd729-54c0-43cd-9282-1828424f0873",
                    ProjectId = "9569c762-a633-4606-b2a0-c05b4fbac542"
                }
            );
    
            // Build the service provider from the service collection.
            _serviceProvider = services.BuildServiceProvider();
        }
    
        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

## Message Formatting
Messages in StellaNowSDK are wrapped in a **StellaNowMessageWrapper** and each specific message type extends this class to define its own properties. Each message needs to follow a certain format, including a type, list of entities, and optional fields. Here is an example:

    public class UserLoginMessage : StellaNowMessageWrapper
    {
        public UserLoginMessage(string patronId, string userId, string timestamp)
            : base(
                "user_login",
                new List<EntityType>{ new EntityType("patron", patronId) })
        {
            AddField("user_id", userId);
            AddField("timestamp", timestamp);
        }
    }

Creating these classes by hand can be prone to errors. Therefore, we provide a command line interface (CLI) tool, **StellaNow CLI**, to automate this task. This tool generates the code for the message classes automatically based on the configuration defined in the Operators Console.

You can install **StellaNow CLI** tool using pip, which is a package installer for Python. It is hosted on Python Package Index (PyPI), a repository of software for the Python programming language. To install it, open your terminal and run the following command:

    pip install stellanow-cli

Once you have installed the **StellaNow CLI** tool, you can use it to generate message classes. Detailed instructions on how to use the **StellaNow CLI** tool can be found in the tool's documentation.

Please note that it is discouraged to write these classes yourself. Using the CLI tool ensures that the message format aligns with the configuration defined in the Operators Console and reduces the potential for errors.

## Customization
StellaNowSDK provides the flexibility to adapt to your needs. Developers can extend provided interfaces and add custom implementations if necessary. This includes changing the message queue strategy to fit your application's requirements.

By default, the SDK uses in-memory queues to temporarily hold messages before sending them. These queues are not persistent and data will be lost if the application terminates unexpectedly.

If your application requires persistent queues that survive application restarts or crashes, you can implement your own queue strategy by extending the **IMessageQueueStrategy** interface and integrating it with a persistent storage solution (like a database or a disk-based queue).

Here is an example of how to change the queue strategy:
    
    services.AddSingleton<IMessageQueueStrategy, YourCustomQueueStrategy>();

Remember to replace **YourCustomQueueStrategy** with your custom strategy class. Be aware that adding a persistent queue may impact the performance of your application depending on the solution used.

For advanced customization options and details on how to extend StellaNowSDK, refer to the detailed SDK documentation on our website.


## Support
For any issues or feature requests, feel free to create a new issue on our GitHub repository. If you need further assistance, contact our support team at help@stella.systems.

## Documentation
Detailed documentation will be available soon.

## License
This project is licensed under the terms of the MIT license.