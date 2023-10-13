using Microsoft.SemanticKernel.SkillDefinition;

public class OldeEnglishPlugin
{
    [SKFunction()]
    public string Translate(string input)
    {
        return $"Translate the following into olde English: {input}";
    }
}
