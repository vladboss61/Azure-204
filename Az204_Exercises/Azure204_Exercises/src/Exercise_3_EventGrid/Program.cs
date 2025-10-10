using Azure;
using Azure.Messaging.EventGrid;
using System;
using System.Threading.Tasks;

namespace Exercise_3_EventGrid;

public record class OrderCreatedEventData
{
    public string OrderGuid { get; init; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        // Execute from console to get the values below:
        // az eventgrid topic show --name $TopicName - g $AzureResourceGroupName--query "endpoint"--output tsv
        // az eventgrid topic key list--name $TopicName - g $AzureResourceGroupName--query "key1"--output tsv

        string topicEndpoint = "<<place-holder>>";
        string topicKey = "<<place-holder>>";

        // Create an EventGridPublisherClient to send events to the specified topic
        EventGridPublisherClient client = new EventGridPublisherClient
            (new Uri(topicEndpoint),
            new AzureKeyCredential(topicKey));

        // Create a new EventGridEvent with sample data
        var eventGridEvent = new EventGridEvent(
            subject: "Example_Subject_VladV",
            eventType: "ExampleEventType_VladV",
            dataVersion: "1.0",
            data: new OrderCreatedEventData
            {
                OrderGuid = Guid.NewGuid().ToString()
            }
        );

        // Send the event to Azure Event Grid
        await client.SendEventAsync(eventGridEvent);
        Console.WriteLine("Event sent successfully.");
    }
}
