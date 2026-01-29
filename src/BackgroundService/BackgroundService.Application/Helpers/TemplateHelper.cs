using System.Collections.Generic;

namespace BackgroundService.Application.Helpers
{
    public static class TemplateHelper
    {
       public static string ReplaceTokens(string template, Dictionary<string, string> tokens)
        {
            if (string.IsNullOrWhiteSpace(template) || tokens == null)
                return template;

            foreach (var token in tokens)
            {
                template = template.Replace($"{{{token.Key}}}", token.Value ?? string.Empty);
            }
            return template;
        }
    }
}
