Param(
    [Parameter(Mandatory = $true)]
    [String]
    $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]
    $SqlServerName,
    [Parameter(Mandatory = $true)]
    [String]
    $FunctionAppName
)

# ---------------------------------------------------------------------------------
# Define Variables
# ---------------------------------------------------------------------------------
$SQL_GROUP_NAME = "${SqlServerName}-admins"

# ---------------------------------------------------------------------------------
# Create AD Group
# ---------------------------------------------------------------------------------
$GROUP_OBJECT_ID = az ad group create `
    --display-name $SQL_GROUP_NAME `
    --mail-nickname $SQL_GROUP_NAME `
    --query id `
    --output tsv

# ---------------------------------------------------------------------------------
# Assign AD Group to SQL Server as Admins
# ---------------------------------------------------------------------------------
az sql server ad-admin create `
    --resource-group $ResourceGroupName `
    --server $SqlServerName `
    --display-name $SQL_GROUP_NAME `
    --object-id $GROUP_OBJECT_ID

# ---------------------------------------------------------------------------------
# Add Curent Logged in User to AD Group
# ---------------------------------------------------------------------------------
$CURRENT_USER_NAME_OBJECT_ID = az ad signed-in-user show `
    --query id `
    --output tsv

az ad group member add `
    --group $GROUP_OBJECT_ID `
    --member-id $CURRENT_USER_NAME_OBJECT_ID

# ---------------------------------------------------------------------------------
# Add Function App MSI to AD Group
# ---------------------------------------------------------------------------------
$FUNCTION_PRINCIPAL_ID = az functionapp identity show `
    --resource-group $ResourceGroupName `
    --name $FunctionAppName `
    --query "principalId" `
    --output tsv

az ad group member add `
    --group $GROUP_OBJECT_ID `
    --member-id $FUNCTION_PRINCIPAL_ID
