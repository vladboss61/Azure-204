param (
    [string]$AzureResourceGroupName = 'vladv-kv-rg',

    [string]$AzureLocation = 'eastus2'
)

$ErrorActionPreference = "Stop"

$KeyVaultName       = "vladv-kv100"
$KeyVaultRoleName   = "Key Vault Secrets Officer"
$KeyVaultSecretName = "ConnectionString"

$KeyVaultSecrets = @{
    $KeyVaultSecretName = "Server=tcp:vladv-sqlsvr.database.windows.net"
}

# Ensure az CLI is available
if (-not (Get-Command "az" -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI (az) is not installed or not available in PATH."
    exit 1
}

$rgStatus = $(az group create --name $AzureResourceGroupName --location $AzureLocation --output json)

Write-Host "Resource Group creation status:" -ForegroundColor Green

$rgStatus | jq .

az keyvault create --name $KeyVaultName --resource-group $AzureResourceGroupName --location $AzureLocation

Write-Host "Key Vault URL:" -ForegroundColor Green
az keyvault show --name $KeyVaultName --query "properties.vaultUri" -o tsv

# Get current user's object ID
$userPrincipal=$(az rest --method GET --url https://graph.microsoft.com/v1.0/me --headers 'Content-Type=application/json' --query userPrincipalName --output tsv)

Write-Host "User Principal Name:" -ForegroundColor Green
Write-Host $userPrincipal

# Get current resource's object ID
$resourceId=$(az keyvault show --resource-group $AzureResourceGroupName --name $KeyVaultName --query id --output tsv)

Write-Host "Key Vault Resource ID:" -ForegroundColor Green
Write-Host $resourceId

# Assign the "Key Vault Secrets Officer" role to the user for the Key Vault
Write-Host "Assigning 'Key Vault Secrets Officer' role to user..." -ForegroundColor Green
az role assignment create --assignee $userPrincipal --role $KeyVaultRoleName --scope $resourceId

# Wait for a few seconds to ensure the role assignment is propagated in Azure
Start-Sleep -Seconds 15

az keyvault secret set --vault-name $KeyVaultName --name $KeyVaultSecretName --value $KeyVaultSecrets[$KeyVaultSecretName]

Write-Host "Show ConnectionString secret." -ForegroundColor Green
az keyvault secret show --name $KeyVaultSecretName --vault-name $KeyVaultName --query value --output tsv

