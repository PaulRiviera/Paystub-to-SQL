Param(
    [Parameter(Mandatory = $true)]
    [String]
    $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]
    $FunctionAppName,
    [Parameter(Mandatory = $true)]
    [String[]]
    $AppSettings
)

# ---------------------------------------------------------------------------------
# Define Variables
# ---------------------------------------------------------------------------------
$RESOURCE_GROUP_NAME = $ResourceGroupName
$FUNC_APP_NAME = $FunctionAppName
$APP_SETTINGS = $AppSettings

# ---------------------------------------------------------------------------------
# Assign Azure Function App Settings
# ---------------------------------------------------------------------------------
az functionapp config appsettings set `
    --resource-group $RESOURCE_GROUP_NAME `
    --name $FUNC_APP_NAME `
    --settings $APP_SETTINGS