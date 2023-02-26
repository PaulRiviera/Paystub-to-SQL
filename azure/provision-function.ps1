Param(
    [Parameter(Mandatory = $true)]
    [String]
    $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]
    $StorageAccountName,
    [Parameter(Mandatory = $false)]
    [String]
    $StorageAccountType = "Standard_LRS",
    [Parameter(Mandatory = $true)]
    [String]
    $FunctionAppName,
    [Parameter(Mandatory = $true)]
    [String]
    $Location
)

$RESOURCE_GROUP_NAME = $ResourceGroupName
$LOCATION = $Location

$FUNC_APP_NAME = $FunctionAppName
$FUNC_STORAGE_ACCOUNT = $StorageAccountName
$FUNC_STORAGE_ACCOUNT_TYPE = $StorageAccountType

# ------------------------------------------------------------------------------
# Provision Resource Group
# ------------------------------------------------------------------------------
az group create `
    --name $RESOURCE_GROUP_NAME `
    --location $LOCATION

# ------------------------------------------------------------------------------
# Provision Storage Account
# ------------------------------------------------------------------------------
az storage account create `
    --name $FUNC_STORAGE_ACCOUNT `
    --resource-group $RESOURCE_GROUP_NAME `
    --location $LOCATION `
    --sku $FUNC_STORAGE_ACCOUNT_TYPE

# ------------------------------------------------------------------------------
# Provision Function App
# ------------------------------------------------------------------------------
az functionapp create `
    --resource-group $RESOURCE_GROUP_NAME `
    --name $FUNC_APP_NAME `
    --storage-account $FUNC_STORAGE_ACCOUNT `
    --consumption-plan-location $LOCATION `
    --runtime dotnet `
    --assign-identity [system] `
    --functions-version 4 `
    --os-type Windows `
    --https-only true
