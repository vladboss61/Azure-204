namespace Exercise_2_Bus;

using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;
using System.Timers;

internal sealed class Program
{
    // handle received messages
    private static async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        Console.WriteLine($"Received: {body}");

        // complete the message. message is deleted from the queue. 
        await args.CompleteMessageAsync(args.Message);
    }

    // handle any errors when receiving messages
    private static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    public static async Task Main(string[] args)
    {
        string serviceBusName = "vladvservicebus";
        string queueName = "myqueuevladv";

        string serviceBusNamespace = $"{serviceBusName}.servicebus.windows.net";

        string queueUrl = $"https://{serviceBusName}.servicebus.windows.net/{queueName}";

        DefaultAzureCredentialOptions options = new()
        {
            ExcludeEnvironmentCredential = true,
            ExcludeManagedIdentityCredential = true
        };

        ServiceBusClient client = new ServiceBusClient(serviceBusNamespace, new DefaultAzureCredential(options));
        ServiceBusSender sender = client.CreateSender(queueName);
        ServiceBusProcessor processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

        var timer = new Timer(2000); // 2 seconds

        timer.Elapsed += async (sr, e) =>
        {
            await SendMessage(sender);
            timer.Dispose(); // stop after first execution
        };
        timer.AutoReset = false; // fire only once
        timer.Start();


        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine("Exiting...");
            eventArgs.Cancel = true;
            processor.StopProcessingAsync().GetAwaiter().GetResult();
        };

        try
        {
            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();

            // Wait for the processor to stop
            while (processor.IsProcessing)
            {
                await Task.Delay(500);
            }
            Console.WriteLine("Stopped receiving messages");
        }
        finally
        {
            // Dispose processor after use
            await processor.DisposeAsync();
        }

    }

    private static async Task SendMessage(ServiceBusSender sender)
    {
        using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

        // number of messages to be sent to the queue
        const int numOfMessages = 3;

        for (int i = 1; i <= numOfMessages; i++)
        {
            messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}"));
        }

        try
        {
            // Use the producer client to send the batch of messages to the Service Bus queue
            await sender.SendMessagesAsync(messageBatch);
            Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
        }
        finally
        {
            // Calling DisposeAsync on client types is required to ensure that network
            // resources and other unmanaged objects are properly cleaned up.
            await sender.DisposeAsync();
        }

        Console.WriteLine("Press any key to continue");
    }
}
