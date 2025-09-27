namespace AzurStorageAccExercise1;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Identity;
using Azure;

public class StorageInfo
{
    public string AccoutnName { get; set; }
    public string ContainerName { get; set; }
    public string BlobFileName { get; set; }
}

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Azure Blob Storage exercise");

        DefaultAzureCredentialOptions options = new()
        {
            ExcludeEnvironmentCredential = true,
            ExcludeManagedIdentityCredential = true
        };

        DefaultAzureCredential credential = new DefaultAzureCredential(options);
        var storageInfo = new StorageInfo { AccoutnName = "azaccountstg", ContainerName = "data-container", BlobFileName = "blob-file.txt" };
        await ExerciseWithStorageAsync(credential, storageInfo);
    }

    public static async Task ExerciseWithStorageAsync(DefaultAzureCredential defaultAzure, StorageInfo storageInfo)
    {
        string blobServiceEndpoint = $"https://{storageInfo.AccoutnName}.blob.core.windows.net";
        var blobServiceClient = new BlobServiceClient(new Uri(blobServiceEndpoint), defaultAzure);

        BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(storageInfo.ContainerName);

        await blobContainerClient.CreateIfNotExistsAsync();

        if (blobContainerClient is null)
        {
            Console.WriteLine("Container creation failed.");
            return;
        }

        BlobClient blobClient = blobContainerClient.GetBlobClient(storageInfo.BlobFileName);

        if (await blobClient.ExistsAsync())
        {
            Console.WriteLine("File already exits.");
        }

        File.WriteAllText(storageInfo.BlobFileName, "Hello, World Az container blob!\n test \n test");

        using FileStream fileStream = File.OpenRead(storageInfo.BlobFileName);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = "text/plain" });

        if (await blobClient.ExistsAsync())
        {
            Console.WriteLine("File Create successfully.");
        }

        await GetListOfBlobsAsync(blobContainerClient);

        Response<BlobDownloadStreamingResult> streamResponse = await blobClient.DownloadStreamingAsync();

        var reader = new StreamReader(streamResponse.Value.Content);

        while(!reader.EndOfStream)
        {
            Console.WriteLine(reader.ReadLine()); 
        }
    }

    public static async Task GetListOfBlobsAsync(BlobContainerClient blobContainerClient)
    {
        await foreach (BlobItem blob in blobContainerClient.GetBlobsAsync())
        {
            Console.WriteLine(blob.VersionId);
            Console.WriteLine(blob.Name);
        }
    }
}
