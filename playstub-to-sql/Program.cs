using AzFormRecognizer.Table;
using AzFormRecognizer.Table.ToSQL;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Data.SqlClient;

if (args.Length == 0) {
    throw new Exception("Please pass in the PDF file path.");
}

string filePath = args[0];

if (!File.Exists(filePath)) {
    throw new Exception("File does not exist.");
}

var details = new DocumentDetails() // This is used later as keys for database table
{ 
    Title = Path.GetFileName(filePath), 
    Id = Guid.NewGuid().ToString()
};

var bytes = await File.ReadAllBytesAsync(filePath);
var memoryStream = new MemoryStream(bytes);

Console.WriteLine("Reading PDF file from " + filePath);

string? keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
if (keyVaultName == null || keyVaultName == "") {
    throw new Exception("Please set the KEY_VAULT_NAME environment variable.");
}

var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net");
var secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());

var formRecognizerEndpoint = await secretClient.GetSecretAsync("FormRecognizerEndpoint");
var formRecognizerKey = await secretClient.GetSecretAsync("FormRecognizerKey");

Console.WriteLine("Form Recognizer endpoint: " + formRecognizerEndpoint.Value.Value);

string endpoint = formRecognizerEndpoint.Value.Value;
string apiKey = formRecognizerKey.Value.Value;

string modelName = "prebuilt-layout";

AzureKeyCredential credential = new AzureKeyCredential(apiKey);
var documentClient = new DocumentAnalysisClient(new Uri(endpoint), credential);  

AnalyzeDocumentOperation operation = await documentClient.AnalyzeDocumentAsync(WaitUntil.Completed, modelName, memoryStream);
AnalyzeResult result = operation.Value;

Console.WriteLine("Document analysis completed.");
Console.WriteLine("Document Tables: " + result.Tables.Count);

void AddMissingInfoAndRelations(List<Table> tables, DocumentDetails details)
{
    if (tables.Count == 0)
    {
        throw new Exception("No tables found in the document.");
    }

    if (tables[0].Title == null && tables[0].Headers != null)
    {
        tables[0].Title = "Payslip";
        var payslipTablePrimaryKey = new ColumnHeader() 
        { 
            Name= "DocumentId",
            TableKey = new TableKey() { Type = TableKeyType.Primary },
            DataType = ColumnDataTypes.VARCHAR
        };

        var primaryKeyColumnIndex = tables[0].Headers.Last().Key + 1;
        tables[0].Headers.Add(primaryKeyColumnIndex, payslipTablePrimaryKey);
    }

    var payslipTableForignKey = new ColumnHeader() 
    { 
        Name= "DocumentId",
        TableKey = new TableKey() { Type = TableKeyType.Foreign, Reference = "Payslip(DocumentId)" },
        DataType = ColumnDataTypes.VARCHAR
    };


    if (tables[1].Title == null)
    {
        tables[1].Title = "Summary";
    }

    if (tables[8].Title == null)
    {
        tables[8].Title = "Allowances";
    }

    foreach (var table in tables)
    {
        if (table.Headers.All(header => header.Value.TableKey == null))
        {
            var primaryKey = new ColumnHeader() { Name= "Id", TableKey = new TableKey() { Type = TableKeyType.Primary }, DataType = ColumnDataTypes.INT };
            table.Headers.Add(table.Headers.Last().Key + 1, primaryKey);
            table.Headers.Add(table.Headers.Last().Key + 1, payslipTableForignKey);
        }

        foreach (var row in table.Rows)
        {
            row.Add("DocumentId", details.Id);
        }
    }
}

var listOfSQLCommands = result.Tables.ToSQL(details, AddMissingInfoAndRelations);

string? serverName = Environment.GetEnvironmentVariable("SQL_SERVER_NAME");
if (serverName == null || serverName == "") {
    Console.WriteLine("Please set the SQL_SERVER_NAME environment variable.");
    return;
}

string? databaseName = Environment.GetEnvironmentVariable("SQL_DATABASE_NAME");
if (databaseName == null || databaseName == "") {
    Console.WriteLine("Please set the SQL_DATABASE_NAME environment variable.");
    return;
}

string ConnectionString = $"Server={serverName}.database.windows.net; Authentication=Active Directory Default; Encrypt=True; Database={databaseName}";

using (SqlConnection connection = new SqlConnection(ConnectionString)) {
    await connection.OpenAsync();

    // First create all tables and relations
    // This will cause an error if run after tables exist, this is currently a limitation of the AzFormRecognizer.Table.ToSQL library
    // which will be fixed in a future release, in the meantime, you can comment out the following line and run the program again for new PDFs
    var createTableCmds = listOfSQLCommands.Where(cmd => cmd.Contains("CREATE TABLE")).ToList();
    foreach (var sqlTableStr in createTableCmds) {
        using (var command = new SqlCommand(sqlTableStr, connection)) {
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine(rowsAffected + " = rows affected.");
        }
    }

    // Insert values into tables
    var inserValuesCmds = listOfSQLCommands.Where(cmd => !cmd.Contains("CREATE TABLE")).ToList();
    foreach (var sqlTableStr in inserValuesCmds) {
        using (var command = new SqlCommand(sqlTableStr, connection)) {
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine(rowsAffected + " = rows affected.");
        }
    }

    await connection.CloseAsync();
}