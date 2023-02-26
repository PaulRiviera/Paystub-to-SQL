Param(
    [Parameter(Mandatory = $true)]
    [String]
    $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]
    $FunctionAppName,
    [Parameter(Mandatory = $true)]
    [String[]]
    $RoleAssignments
)

# ---------------------------------------------------------------------------------
# Define Variables
# ---------------------------------------------------------------------------------
$RESOURCE_GROUP_NAME = $ResourceGroupName
$FUNC_APP_NAME = $FunctionAppName

# ---------------------------------------------------------------------------------
# Retrieve Resource Group ID for use as Scope
# ---------------------------------------------------------------------------------
$RESOURCE_GROUP_ID = az group show `
    --name $RESOURCE_GROUP_NAME `
    --query id

# ---------------------------------------------------------------------------------
# Assign Role to Function App
# ---------------------------------------------------------------------------------
$FUNC_ID = az functionapp show `
    --resource-group $RESOURCE_GROUP_NAME `
    --name $FUNC_APP_NAME `
    --query id

$PRINCIPAL_ID = az functionapp identity show `
    --ids $FUNC_ID `
    --query principalId

foreach ($role in $RoleAssignments) {
    az role assignment create `
        --assignee $PRINCIPAL_ID `
        --role $role `
        --scope $RESOURCE_GROUP_ID
}