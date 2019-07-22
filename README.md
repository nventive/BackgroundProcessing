# BackgroundProcessing

.NET Core server-side background processing.

Allows background processing of jobs/tasks in ASP.NET Core and Azure Functions.
Support for in-memory queue and Azure Storage Queue.

Integrated with .NET Core Dependency Injection.

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

## Getting Started

### Vocabulary

Here are the terms used throughout the components:

- `IBackgroundCommand`: The POCO command that allows transfer of execution; this is the unit that gets serialized; contains the parameters of the execution.
- `IBackgroundCommandHandler`: Handles the execution of `IBackgrounCommand`. This is where the business logic is implemented.
- `IBackgroundDispatcher`: The dispatcher is responsible for queueing commands for later execution.
- `IBackgroundProcessor`: The processor is responsible for selecting and invoking appropriate `IBackgroundCommandHandler`.

### Basic usage

Install the package:

```
Install-Package BackgroundProcessing.Core
```

Define the commands & handlers

```csharp
using BackgroundProcessing.Core;

///<summary>
/// Defines a command
///</summary>
public class MyCommand : BackgroundCommand
{
    ///<summary>
    /// Gets or sets custom parameters.
    ///</summary>
    public string MyParameter { get; set; }
}

///<summary>
/// Defines the corresponding command handler
///</summary>
public class MyCommandHandler : IBackgroundCommandHandler<MyCommand>
{
    public MyCommandHandler(IDependencyOne dependency)
    {
        // Handle dependencies
    }

    public async Task HandleAsync(MyCommand command, CancellationToken cancellationToken = default)
    {
        // Handles the command by executing business logic
    }
}

```

Registers the command handler

```csharp
using Microsoft.Extensions.DependencyInjection;

// in your Startup.cs, or wherever services are registered.
// This will register all the IBackgroundCommandHandler in the assembly.
services.AddBackgroundCommandHandlersFromAssemblyContaining<MyCommandHandler>();
```

When you want to dispatch a command for execution, use the registered `IBackgroundDispatcher`:

```csharp
using BackgroundProcessing.Core;

public class MyService
{
    private readonly IBackgroundDispatcher _dispatcher;

    public MyService(IBackgroundDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? new ArgumentNullException(nameof(dispatcher));
    }

    public async Task DispatchCommand()
    {
        var command = new MyCommand { MyParameter = "foo" };
        await _dispatcher.DispatchAsync(command);
    }
}

```

### Getting Started with .NET Core Host in-memory

This will registered a `IBackgroundDispatcher`, a `IBackgroundProcessor` and a `IHostingService` that communicates via an in-memory queue.
This is very easy to get started with, but should not be used in any production scenario, as you will loose pending commands in case of process stop/restart.

```csharp
using Microsoft.Extensions.DependencyInjection;

// in your Startup.cs, or wherever services are registered.
services.AddHostingServiceConcurrentQueueBackgroundProcessing();
```

### Getting Started with .NET Core `IHostingService` and an Azure Storage Queue

This is the easiest deployment that can be suitable for production; it only requires a .NET Core host (such as ASP.NET Core) and an [Azure Storage Queue](https://azure.microsoft.com/en-in/services/storage/queues/).

First, ensure that you created an Azure Storage Account.

Then, install the package:

```
Install-Package BackgroundProcessing.Azure.Storage.Queue
```

And register the services:

```csharp
using Microsoft.Extensions.DependencyInjection;

// in your Startup.cs, or wherever services are registered.
services.AddAzureStorageQueueBackgroundDispatcher();
services
  .AddAzureStorageQueueBackgroundProcessing()
  // Convenient method to register the CloudQueue client from a connection string.
  .ConfigureCloudQueueUsingConnectionStringName("ConnectionStringName", "queue name");

// Alternatively, you can build a CloudQueue service and register it yourself
services.AddSingleton<CloudQueue>(serviceProvider => { ... });
```

### Getting started using Azure Functions processing and Azure Storage Queue

This method allows dispatching commands to an Azure Storage Queue and the processing
inside an Azure Functions. This is probably the best production option.

First, ensure that you created an Azure Storage Account.

Then, install the package:

```
Install-Package BackgroundProcessing.Azure.Storage.Queue
```

Then, in the process responsible for dispatching commands, register the dispatcher:

```csharp
using Microsoft.Extensions.DependencyInjection;

// in your Startup.cs, or wherever services are registered.
services.AddAzureStorageQueueBackgroundDispatcher();
```

Last, create an Azure Functions project and:

1. Add the Azure Functions Queue Bindings
```
Install-Package Microsoft.Azure.WebJobs.Extensions.Storage
```

2. Register the services by creating a `FunctionsStartup` class
```csharp
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Create a FunctionStartup to configure dependency injection
/// </summary>
public class Startup : FunctionsStartup
{
    /// <inheritdoc />
    public override void Configure(IFunctionsHostBuilder builder)
    {
        // Register your services...
        builder
          .Services
          // Register the command handlers if it's not already done
          .AddBackgroundCommandHandlersFromAssemblyContaining<>();
          .AddAzureFunctionsQueueStorageProcessing();
    }
}
```

3. Create a function with a queue trigger

```csharp
using System;
using System.Threading.Tasks;
using BackgroundProcessing.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Queue;

public class BackgroundProcessingFunction
{
    private readonly AzureFunctionsQueueStorageHandler _handler;

    public BackgroundProcessingFunction(
        AzureFunctionsQueueStorageHandler handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    [FunctionName("BackgroundProcessingFunction")]
    public async Task Run(
      [QueueTrigger("queue name", Connection = "ConnectionString")] CloudQueueMessage message)
    {
        await _handler.HandleAsync(message);
    }
}
```

## Features

### Handle processor exceptions

It is possible to get notified and received `Exceptions` during the execution of handlers.

To do so, you can register a `IBackgroundProcessor` decorator.

```csharp
using BackgroundProcessing.Core;

public class ErrorHandlerBackgroundProcessorDecorator: IBackgroundProcessor
{
    private readonly IBackgroundProcessor _wrappedProcessor;

    public ErrorHandlerBackgroundProcessorDecorator(IBackgroundProcessor wrappedProcessor)
    {
        _wrappedProcessor = _wrappedProcessor ?? throw new ArgumentNullException(nameof(wrappedProcessor));
    }

    public async Task ProcessAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            await _wrappedProcessor.ProcessAsync(command, cancellationToken);
        } catch (Exception ex)
        {
            // Handle the exception.
        }
    }
}

// Then during the registration of the processor/services:
using Microsoft.Extensions.DependencyInjection;

services
    .Add...Processing()
    .DecorateProcessor<ErrorHandlerBackgroundProcessorDecorator>();
```

This technique can be extended to handle any cross-cutting concern when dispatching or processing commands.

### Integration with Application Insights

Add the package
```
Install-Package BackgroundProcessing.Azure.ApplicationInsights
```

Then register the decorators:

```csharp
// Then during the registration of the processor/services:
using Microsoft.Extensions.DependencyInjection;

services
    .Add...Processing() // or Add...Dispatcher()
    .AddApplicationInsightsDecorators();
```

### Further customizations

- Most of the dispatcher and services can be further customize by looking at the options; e.g. when applicable, the degree of execution parallelism, the queue polling frequency and the message batch size can be adjusted for your scenario
- Override `IBackgroundProcessor` to customize the way `IBackgroundCommandHandler` are resolved and executed, regardless of the queueing scenario
- Override `IBackgroundCommandSerializer` to adjust `IBackgroundCommand` serialization (defaults to JSON.NET)
- Commands can implement `IBackgroundCommand` (instead of deriving from `BackgroundCommand`) if needed

## Changelog

Please consult the [CHANGELOG](CHANGELOG.md) for more information about version
history.

## License

This project is licensed under the Apache 2.0 license - see the
[LICENSE](LICENSE) file for details.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on the process for
contributing to this project.

Be mindful of our [Code of Conduct](CODE_OF_CONDUCT.md).

## Acknowledgments

- [Dataflow](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) for hosting services execution management
- [Scrutor](https://github.com/khellang/Scrutor) for decorators service registration
