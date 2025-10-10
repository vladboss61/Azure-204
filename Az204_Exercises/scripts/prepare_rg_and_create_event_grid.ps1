param (
    [string]$AzureResourceGroupName = 'vladv-bus-rg',
    [string]$AzureLocation = 'eastus2'
)

$ErrorActionPreference = "Stop"

# Ensure az CLI is available
if (-not (Get-Command "az" -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI (az) is not installed or not available in PATH."
    exit 1
}

# Ensure jq is available
if (-not (Get-Command "jq" -ErrorAction SilentlyContinue)) {
    Write-Error "jq is not installed or not available in PATH."
    exit 1
}

$RandomNumber = Get-Random -Minimum 10 -Maximum 999
$rgStatus = $(az group create --name $AzureResourceGroupName --location $AzureLocation --output json)

Write-Host "Resource Group creation status:" -ForegroundColor Green
$rgStatus | jq .

$TopicName="mytopic-event-grid-topic-${RandomNumber}"
$SiteName="event-grid-site-${RandomNumber}"
$SiteURL="https://${SiteName}.azurewebsites.net"

# Additional registration for Event Grid, executed once per subscription
# az provider register --namespace Microsoft.EventGrid
# az provider show --namespace Microsoft.EventGrid --query "registrationState"

#Create Event Grid Topic

$evStatus = $(az eventgrid topic create --name $TopicName --resource-group $AzureResourceGroupName --location $AzureLocation --output json)

Write-Host "Event Grid Topic creation status:" -ForegroundColor Green
$evStatus | jq .

az deployment group create `
    --resource-group $AzureResourceGroupName `
    --template-uri "https://raw.githubusercontent.com/Azure-Samples/azure-event-grid-viewer/main/azuredeploy.json" `
    --parameters siteName=$SiteName hostingPlanName=viewerhost `

Write-Host "Your web app URL: ${SiteURL}" -ForegroundColor Green

$Endpoint = "${SiteURL}/api/updates"

$TopicId = $(az eventgrid topic show --resource-group $AzureResourceGroupName `
    --name $TopicName --query "id" --output tsv) `

az eventgrid event-subscription create `
    --source-resource-id $TopicId `
    --name TopicSubscription `
    --endpoint $Endpoint `

Write-Host "Event Grid Topic Endpoint and Key:" -ForegroundColor Green

Write-Host $(az eventgrid topic show --name $TopicName -g $AzureResourceGroupName --query "endpoint" --output tsv) -ForegroundColor Green
Write-Host $(az eventgrid topic key list --name $TopicName -g $AzureResourceGroupName --query "key1" --output tsv) -ForegroundColor Green