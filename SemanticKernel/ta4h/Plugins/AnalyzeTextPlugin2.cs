using Azure;
using Azure.AI.TextAnalytics;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.SkillDefinition;

public class TextAnalysisPlugin
{
    [SKFunction()]
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
Console.WriteLine($"Created On   : {healthOperation.CreatedOn}");
Console.WriteLine($"Expires On   : {healthOperation.ExpiresOn}");
Console.WriteLine($"Status       : {healthOperation.Status}");
Console.WriteLine($"Last Modified: {healthOperation.LastModified}");

await foreach (AnalyzeHealthcareEntitiesResultCollection documentsInPage in healthOperation.Value)
{
    Console.WriteLine($"Results of \"Healthcare Async\" Model, version: \"{documentsInPage.ModelVersion}\"");
    Console.WriteLine("");

    foreach (AnalyzeHealthcareEntitiesResult entitiesInDoc in documentsInPage)
    {
        if (!entitiesInDoc.HasError)
        {
            foreach (var entity in entitiesInDoc.Entities)
            {
                // view recognized healthcare entities
                Console.WriteLine($"  Entity: {entity.Text}");
                Console.WriteLine($"  Category: {entity.Category}");
                Console.WriteLine($"  Offset: {entity.Offset}");
                Console.WriteLine($"  Length: {entity.Length}");
                Console.WriteLine($"  NormalizedText: {entity.NormalizedText}");
                Console.WriteLine($"  Links:");

                // view entity data sources
                foreach (EntityDataSource entityDataSource in entity.DataSources)
                {
                    Console.WriteLine($"    Entity ID in Data Source: {entityDataSource.EntityId}");
                    Console.WriteLine($"    DataSource: {entityDataSource.Name}");
                }

                // view assertion
                if (entity.Assertion != null)
                {
                    Console.WriteLine($"  Assertions:");

                    if (entity.Assertion?.Association != null)
                    {
                        Console.WriteLine($"    Association: {entity.Assertion?.Association}");
                    }

                    if (entity.Assertion?.Certainty != null)
                    {
                        Console.WriteLine($"    Certainty: {entity.Assertion?.Certainty}");
                    }
                    if (entity.Assertion?.Conditionality != null)
                    {
                        Console.WriteLine($"    Conditionality: {entity.Assertion?.Conditionality}");
                    }
                }
            }

            Console.WriteLine($"  We found {entitiesInDoc.EntityRelations.Count} relations in the current document:");
            Console.WriteLine("");

            // view recognized healthcare relations
            foreach (HealthcareEntityRelation relations in entitiesInDoc.EntityRelations)
            {
                Console.WriteLine($"    Relation: {relations.RelationType}");
                Console.WriteLine($"    For this relation there are {relations.Roles.Count} roles");

                // view relation roles
                foreach (HealthcareEntityRelationRole role in relations.Roles)
                {
                    Console.WriteLine($"      Role Name: {role.Name}");

                    Console.WriteLine($"      Associated Entity Text: {role.Entity.Text}");
                    Console.WriteLine($"      Associated Entity Category: {role.Entity.Category}");
                    Console.WriteLine("");
                }

                Console.WriteLine("");
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

    return "complete";

    }
}
