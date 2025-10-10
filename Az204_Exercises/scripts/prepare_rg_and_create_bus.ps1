param (
    [string]$AzureResourceGroupName = 'vladv-bus-rg',
    [string]$AzureLocation = 'eastus2'
)

$ErrorActionPreference = "Stop"

$ServiceBusName = "vladvservicebus"
$QueueName     = "myqueuevladv"
$ServiceBusRoleName = "Azure Service Bus Data Owner"

# Ensure az CLI is available
if (-not (Get-Command "az" -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI (az) is not installed or not available in PATH."
    exit 1
}

$rgStatus = $(az group create --name $AzureResourceGroupName --location $AzureLocation --output json)

Write-Host "Resource Group creation status:" -ForegroundColor Green

$rgStatus | jq .

$busStatus = $(az servicebus namespace create --name $ServiceBusName --resource-group $AzureResourceGroupName --location $AzureLocation)

Write-Host "Service Bus creation status:" -ForegroundColor Green
$busStatus | jq .

$queueStatus = $(az servicebus queue create --name $QueueName --namespace-name $ServiceBusName --resource-group $AzureResourceGroupName)

Write-Host "Queue creation status:" -ForegroundColor Green
$queueStatus | jq .

# Get current user's object ID
$userPrincipal=$(az rest --method GET --url https://graph.microsoft.com/v1.0/me --headers 'Content-Type=application/json' --query userPrincipalName --output tsv)

Write-Host "User Principal Name:" -ForegroundColor Green
Write-Host $userPrincipal

# Get current resource's object ID
$resourceId=$(az servicebus namespace show --resource-group $AzureResourceGroupName --name $ServiceBusName --query id --output tsv)

Write-Host "Service Bus Resource ID:" -ForegroundColor Green
Write-Host $resourceId

# Assign the "Azure Service Bus Data Owner" role to the user for the Service Bus
Write-Host "Assigning 'Azure Service Bus Data Owner' role to user..." -ForegroundColor Green
az role assignment create --assignee $userPrincipal --role $ServiceBusRoleName --scope $resourceId

