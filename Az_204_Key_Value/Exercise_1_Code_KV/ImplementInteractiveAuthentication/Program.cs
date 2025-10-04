namespace ImplementInteractiveAuthentication;

using dotenv.net;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

internal class Program
{
    public static async Task  Main(string[] args)
    {
        DotEnv.Load();
        IDictionary<string, string> envVars = DotEnv.Read();

        string _clientId = envVars["CLIENT_ID"];
        string _tenantId = envVars["TENANT_ID"];

        // Define the scopes required for authentication
        string[] _scopes = { "User.Read" };

        // Build the MSAL public client application with authority and redirect URI
        var app = PublicClientApplicationBuilder.Create(_clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
            .WithDefaultRedirectUri()
            .Build();

        // Attempt to acquire an access token silently or interactively
        AuthenticationResult result;
        try
        {
            // Try to acquire token silently from cache for the first available account
            result = await app.AcquireTokenInteractive(_scopes)
                        .ExecuteAsync();
            Console.WriteLine($"Access Token:\n{result.AccessToken}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong.");
        }
    }
}
