using Azure;
using Azure.AI.TextAnalytics;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.SkillDefinition;

public class TextAnalysisPlugin
{
    [SKFunction]
    [Description("Analyze the healthcare entities in the text")]
    public async Task<string> AnalyzeHealthcareEntities(string text)
    {
        string configFile = "../config/settings.json";
        return await CallTextAnalyticsForHealth(text, configFile);
    }

    public static async Task<string> CallTextAnalyticsForHealth(string text, string configFile = "settings.json")
    {
        // Load the settings from the config file
        var settings = JsonSerializer.Deserialize<MySettings>(File.ReadAllText(configFile));

        // Create a new TextAnalyticsClient using the provided credentials
        var credentials = new AzureKeyCredential(settings.LanguageKey);
        var endpoint = new Uri(settings.LanguageEndpoint);
        var client = new TextAnalyticsClient(endpoint, credentials);

        // Call the AnalyzeHealthcareEntitiesAsync method to analyze the healthcare entities in the text
        List<string> batchInput = new List<string>() { text };
        AnalyzeHealthcareEntitiesOperation healthOperation = await client.StartAnalyzeHealthcareEntitiesAsync(batchInput);
        await healthOperation.WaitForCompletionAsync();

        // Serialize the response to JSON and return it as a string
        StringBuilder sb = new StringBuilder();

//Console.WriteLine($"Created On   : {healthOperation.CreatedOn}");
//Console.WriteLine($"Expires On   : {healthOperation.ExpiresOn}");
//Console.WriteLine($"Status       : {healthOperation.Status}");
//Console.WriteLine($"Last Modified: {healthOperation.LastModified}");

await foreach (AnalyzeHealthcareEntitiesResultCollection documentsInPage in healthOperation.Value)
{
    //Console.WriteLine($"Results of \"Healthcare Async\" Model, version: \"{documentsInPage.ModelVersion}\"");
    //Console.WriteLine("");

    foreach (AnalyzeHealthcareEntitiesResult entitiesInDoc in documentsInPage)
    {
        if (!entitiesInDoc.HasError)
        {
            foreach (var entity in entitiesInDoc.Entities)
            {
                // view recognized healthcare entities
                sb.AppendLine($"  Entity: {entity.Text}");
                sb.AppendLine($"  Category: {entity.Category}");
                //Console.WriteLine($"  Entity: {entity.Text}");
                //Console.WriteLine($"  Category: {entity.Category}");
                //Console.WriteLine($"  Offset: {entity.Offset}");
                //Console.WriteLine($"  Length: {entity.Length}");
                //Console.WriteLine($"  NormalizedText: {entity.NormalizedText}");
                //Console.WriteLine($"  Links:");

                // view entity data sources
                //foreach (EntityDataSource entityDataSource in entity.DataSources)
                //{
                //    Console.WriteLine($"    Entity ID in Data Source: {entityDataSource.EntityId}");
                //    Console.WriteLine($"    DataSource: {entityDataSource.Name}");
                //}

                // view assertion
                //if (entity.Assertion != null)
                //{
                //    Console.WriteLine($"  Assertions:");
//
                //    if (entity.Assertion?.Association != null)
                //    {
                //        Console.WriteLine($"    Association: {entity.Assertion?.Association}");
                //    }
//
                //    if (entity.Assertion?.Certainty != null)
                //    {
                //        Console.WriteLine($"    Certainty: {entity.Assertion?.Certainty}");
                //    }
                //    if (entity.Assertion?.Conditionality != null)
                //    {
                //        Console.WriteLine($"    Conditionality: {entity.Assertion?.Conditionality}");
                //    }
                //}
            }

            sb.AppendLine("");
            sb.AppendLine($"  We found {entitiesInDoc.EntityRelations.Count} relations in the current document:");
            sb.AppendLine("");
            //Console.WriteLine($"  We found {entitiesInDoc.EntityRelations.Count} relations in the current document:");
            //Console.WriteLine("");

            // view recognized healthcare relations
            foreach (HealthcareEntityRelation relations in entitiesInDoc.EntityRelations)
            {
                sb.AppendLine($"    Relation: {relations.RelationType}");
                sb.AppendLine($"    For this relation there are {relations.Roles.Count} roles");

                //Console.WriteLine($"    Relation: {relations.RelationType}");
                //Console.WriteLine($"    For this relation there are {relations.Roles.Count} roles");

                // view relation roles
                foreach (HealthcareEntityRelationRole role in relations.Roles)
                {
                    sb.AppendLine($"      Role Name: {role.Name}");
                    sb.AppendLine($"      Associated Entity Text: {role.Entity.Text}");
                    sb.AppendLine($"      Associated Entity Category: {role.Entity.Category}");
                    sb.AppendLine("");
                    //Console.WriteLine($"      Role Name: {role.Name}");
                    //Console.WriteLine($"      Associated Entity Text: {role.Entity.Text}");
                    //Console.WriteLine($"      Associated Entity Category: {role.Entity.Category}");
                    //Console.WriteLine("");
                }

                sb.AppendLine("");
                //Console.WriteLine("");
            }
        }
        else
        {
            Console.WriteLine("  Error!");
            Console.WriteLine($"  Document error code: {entitiesInDoc.Error.ErrorCode}.");
            Console.WriteLine($"  Message: {entitiesInDoc.Error.Message}");
        }

        Console.WriteLine("");
    }
}

    string myString = sb.ToString();

    return myString;

    }
}
