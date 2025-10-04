namespace ImplementInteractiveAuthentication;

using Azure.Core;
using dotenv.net;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

public class CustomTokenCredential : TokenCredential
{
    private readonly string _accessToken;
    private readonly DateTimeOffset? _expiresOn; // Optional: if you have expiration info

    public CustomTokenCredential(string accessToken, DateTimeOffset? expiresOn = null)
    {
        _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        _expiresOn = expiresOn;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        // For synchronous calls, return the existing token
        return new AccessToken(_accessToken, _expiresOn ?? DateTimeOffset.MaxValue);
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        // For asynchronous calls, return the existing token
        return new ValueTask<AccessToken>(new AccessToken(_accessToken, _expiresOn ?? DateTimeOffset.MaxValue));
    }
}

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

            var credential = new CustomTokenCredential(result.AccessToken);

            var graphClient = new GraphServiceClient(credential);

            // Example: get the current user
            var me = await graphClient.Me.GetAsync();

            Console.WriteLine($"UserId: {me.Id}");
            Console.WriteLine($"Name : {me.DisplayName}");
            Console.WriteLine($"Job Title : {me.JobTitle}");
            Console.WriteLine($"Mail: {me.Mail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong.");
        }


    }
}
