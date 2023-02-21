using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PaystubFunction
{
    public static class Processor
    {
        [FunctionName("Processor")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            var formCollection = await req.ReadFormAsync();
            log.LogInformation($"$Processing ${formCollection.Files.Count} files.");

            if (formCollection.Files.Count == 0)
            {
                return new BadRequestObjectResult("No files were uploaded.");
            }

            string keyVaultName = Environment.GetEnvironmentVariable("KEY_VAULT_NAME");
            if (string.IsNullOrEmpty(keyVaultName))
            {
                throw new Exception("Please set the KEY_VAULT_NAME environment variable.");
            }

            string serverName = Environment.GetEnvironmentVariable("SQL_SERVER_NAME");
            if (string.IsNullOrEmpty(serverName))
            {
                throw new Exception("Please set the SQL_SERVER_NAME environment variable.");
            }

            string databaseName = Environment.GetEnvironmentVariable("SQL_DATABASE_NAME");
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new Exception("Please set the SQL_DATABASE_NAME environment variable.");
            }

            var processor = new PaystubProcessor();
            List<string> filesProcessed = new List<string>();
            List<string> commands = new List<string>();

            foreach (var file in formCollection.Files)
            {
                log.LogInformation($"Processing file: {file.FileName}");
                var fileStream = file.OpenReadStream();

                var (details, sqlCommands) = await processor.ProcessPaystubAsync(fileStream, file.FileName, keyVaultName);
                commands.AddRange(sqlCommands);
                filesProcessed.Add(details.Id);
            }

            try {
                await processor.UploadToSQLAsync(serverName, databaseName, commands, createTables: true);
            } catch (Exception ex) {
                log.LogError(ex, "Error uploading to SQL");
                return new BadRequestObjectResult(ex.Message);
            }

            return new OkObjectResult(filesProcessed);
        }
    }
}
