Param(
    [Parameter(Mandatory = $true)]
    [String]
    $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]
    $SqlServerName
)

# ------------------------------------------------------------------------------
# Variables
# ------------------------------------------------------------------------------
$RESOURCE_GROUP_NAME = $ResourceGroupName
$SQL_SERVER_NAME = $SqlServerName

$START_IP = "0.0.0.0" # Allow all Azure IP addresses
$END_IP = "0.0.0.0" # Allow all Azure IP addresses

# ------------------------------------------------------------------------------
# Configure a server-based firewall rule
# ------------------------------------------------------------------------------
az sql server firewall-rule create `
    --resource-group $RESOURCE_GROUP_NAME `
    --server $SQL_SERVER_NAME `
    --name AzureServicesRule `
    --start-ip-address $START_IP `
    --end-ip-address $END_IP
