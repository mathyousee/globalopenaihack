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
        string configFile = "settings.json";
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
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(healthOperation.Value, options);
    }
}