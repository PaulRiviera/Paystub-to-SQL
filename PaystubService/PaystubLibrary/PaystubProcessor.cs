using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using AzFormRecognizer.Table;
using AzFormRecognizer.Table.ToSQL;
using Microsoft.Data.SqlClient;

public class PaystubProcessor
{
    public async Task<(DocumentDetails, IEnumerable<string>)> ProcessPaystubAsync(Stream fileStream, string fileName, string keyVaultName)
    {
        var details = new DocumentDetails()
        {
            Title = fileName,
            Id = Guid.NewGuid().ToString()
        };

        var (endpoint, apiKey) = await GetKeyVaultSecretsAsync(keyVaultName);
        var result = await AnalyzeWithFormRecognizerAsync(fileStream, endpoint, apiKey);

        var commands = ConvertToSQL(PaystubFormat.Workday, result, details);

        return (details, commands);
    }

    public async Task<(string Endpoint, string ApiKey)> GetKeyVaultSecretsAsync(string keyVaultName)
    {
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net");
        var secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());

        var formRecognizerEndpoint = await secretClient.GetSecretAsync("FormRecognizerEndpoint");
        var formRecognizerKey = await secretClient.GetSecretAsync("FormRecognizerKey");

        return (formRecognizerEndpoint.Value.Value, formRecognizerKey.Value.Value);
    }

    public async Task<AnalyzeResult> AnalyzeWithFormRecognizerAsync(Stream fileStream, string endpoint, string apiKey)
    {
        string modelName = "prebuilt-layout";

        AzureKeyCredential credential = new AzureKeyCredential(apiKey);
        var documentClient = new DocumentAnalysisClient(new Uri(endpoint), credential);

        AnalyzeDocumentOperation operation = await documentClient.AnalyzeDocumentAsync(WaitUntil.Completed, modelName, fileStream);
        return operation.Value;
    }

    public IEnumerable<string> ConvertToSQL(PaystubFormat format, AnalyzeResult result, DocumentDetails details)
    {
        switch (format)
        {
            case PaystubFormat.Workday:
                var listOfSQLCommands = result.Tables.ToSQL(details, Workday.FormatPaystub);
                return listOfSQLCommands;
            default:
                throw new Exception("Paystub format not supported.");
        }
    }

    public async Task UploadToSQLAsync(string serverName, string databaseName, IEnumerable<string> commands)
    {
        string ConnectionString = $"Server={serverName}.database.windows.net; Authentication=Active Directory Default; Encrypt=True; Database={databaseName}";

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            foreach (var sqlCmd in commands)
            {
                using (var command = new SqlCommand(sqlCmd, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            await connection.CloseAsync();
        }
    }
}