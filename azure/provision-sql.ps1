Param(
    [Parameter(Mandatory = $true)]
    [String]
    $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]
    $SqlServerName,
    [Parameter(Mandatory = $true)]
    [String]
    $SqlDatabaseName,
    [Parameter(Mandatory = $true)]
    [String]
    $ClientIPStart,
    [Parameter(Mandatory = $true)]
    [String]
    $ClientIPEnd,
    [Parameter(Mandatory = $true)]
    [String]
    $Location
)

# ------------------------------------------------------------------------------
# Variables
# ------------------------------------------------------------------------------

$RESOURCE_GROUP_NAME = $ResourceGroupName
$LOCATION = $Location

$SQL_SERVER_NAME = $SqlServerName
$SQL_DATABASE_NAME = $SqlDatabaseName

$START_IP = $ClientIPStart
$END_IP = $ClientIPEnd

# ------------------------------------------------------------------------------
# Provision Resource Group
# ------------------------------------------------------------------------------
az group create `
    --name $RESOURCE_GROUP_NAME `
    --location $LOCATION

# ------------------------------------------------------------------------------
# Provision Server (for current signed-in user)
# ------------------------------------------------------------------------------
$SQL_ADMIN_NAME = az ad signed-in-user show `
    --query displayName `
    --output tsv

$SQL_ADMIN_USER_OBJECT_ID = az ad signed-in-user show `
    --query id `
    --output tsv

az sql server create `
    --name $SQL_SERVER_NAME `
    --resource-group $RESOURCE_GROUP_NAME `
    --location $LOCATION `
    --enable-ad-only-auth `
    --external-admin-principal-type User `
    --external-admin-name $SQL_ADMIN_NAME `
    --external-admin-sid $SQL_ADMIN_USER_OBJECT_ID

# ------------------------------------------------------------------------------
# Configure a server-based firewall rule
# ------------------------------------------------------------------------------
az sql server firewall-rule create `
    --resource-group $RESOURCE_GROUP_NAME `
    --server $SQL_SERVER_NAME `
    --name AllowMyIp `
    --start-ip-address $START_IP `
    --end-ip-address $END_IP

# ------------------------------------------------------------------------------
# Create a database
# ------------------------------------------------------------------------------
az sql db create `
    --resource-group $RESOURCE_GROUP_NAME `
    --server $SQL_SERVER_NAME `
    --name $SQL_DATABASE_NAME `
    --edition GeneralPurpose `
    --compute-model Serverless `
    --family Gen5 `
    --capacity 2