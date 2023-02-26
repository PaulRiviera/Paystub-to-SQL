Param(
    [Parameter(Mandatory = $true)]
    [String]
    $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]
    $KeyVaultName
)

# ------------------------------------------------------------------------------
# Variables
# ------------------------------------------------------------------------------
$RESOURCE_GROUP_NAME = $ResourceGroupName
$KEY_VAULT_NAME = $KeyVaultName

# ------------------------------------------------------------------------------
# Update Key Vault to Enable RBAC Authorization
# ------------------------------------------------------------------------------
az keyvault update `
    --name $KEY_VAULT_NAME `
    --resource-group $RESOURCE_GROUP_NAME `
    --enable-rbac-authorization true