Param(
    [Parameter(Mandatory = $true)]
    [String]
    $ResourceGroupName,
    [Parameter(Mandatory = $true)]
    [String]
    $FormRecognizerName,
    [Parameter(Mandatory = $true)]
    [String]
    $KeyVaultName,
    [Parameter(Mandatory = $true)]
    [String]
    $Location
)

# ------------------------------------------------------------------------------
# Variables
# ------------------------------------------------------------------------------
$RESOURCE_GROUP_NAME = $ResourceGroupName
$FORM_RECOGNIZER_ACCOUNT = $FormRecognizerName
$KEY_VAULT_NAME = $KeyVaultName

# ------------------------------------------------------------------------------
# Provision Resource Group
# ------------------------------------------------------------------------------
az group create `
    --name $RESOURCE_GROUP_NAME `
    --location $LOCATION

# ------------------------------------------------------------------------------
# Provision Azure Key Vault
# ------------------------------------------------------------------------------

az keyvault create `
    --name $KEY_VAULT_NAME `
    --resource-group $RESOURCE_GROUP_NAME `
    --location $LOCATION

# ------------------------------------------------------------------------------
# Provision Azure Form Recognizer
# ------------------------------------------------------------------------------

$FORM_RECOGNIZER_ACCOUNT_ENDPOINT = az cognitiveservices account create `
    --kind "FormRecognizer" `
    --name $FORM_RECOGNIZER_ACCOUNT `
    --resource-group $RESOURCE_GROUP_NAME `
    --location $LOCATION `
    --sku "S0" `
    --assign-identity `
    --yes `
    --query "properties.endpoint" `
    --output tsv

$FORM_RECOGNIZER_ACCOUNT_KEY = az cognitiveservices account keys list `
    --name $FORM_RECOGNIZER_ACCOUNT `
    --resource-group $RESOURCE_GROUP_NAME `
    --query "key1" `
    --output tsv

# ------------------------------------------------------------------------------
# Store Azure Form Recognizer Keys in Vault
# ------------------------------------------------------------------------------

az keyvault secret set `
    --vault-name $KEY_VAULT_NAME `
    --name "FormRecognizerEndpoint" `
    --value $FORM_RECOGNIZER_ACCOUNT_ENDPOINT

az keyvault secret set `
    --vault-name $KEY_VAULT_NAME `
    --name "FormRecognizerKey" `
    --value $FORM_RECOGNIZER_ACCOUNT_KEY