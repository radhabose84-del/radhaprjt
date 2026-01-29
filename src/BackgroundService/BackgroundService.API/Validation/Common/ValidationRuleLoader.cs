    using Newtonsoft.Json;

    namespace BackgroundService.API.Validation.Common
    {
        public static class ValidationRuleLoader
        {
            public static List<ValidationRule> LoadValidationRules(string rulesJsonPath = null)
            {
                try
                {
                    // Default path if not provided
                    if (string.IsNullOrEmpty(rulesJsonPath))
                    {
                        rulesJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Validation","Common", "validation-rules.json");
                    }

                    if (!File.Exists(rulesJsonPath))
                    {
                        throw new FileNotFoundException($"Validation rules file not found at {rulesJsonPath}");
                    }

                    var rulesJson = File.ReadAllText(rulesJsonPath);
                    if (string.IsNullOrWhiteSpace(rulesJson))
                    {
                        throw new InvalidOperationException("Validation rules file is empty.");
                    }

                    var validationRules = JsonConvert.DeserializeObject<List<ValidationRule>>(rulesJson) 
                                        ?? new List<ValidationRule>();

                    if (!validationRules.Any())
                    {
                        throw new InvalidOperationException("No validation rules were loaded from the file.");
                    }

                    return validationRules;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error loading validation rules.", ex);
                }
            }
        }

        public class ValidationRule
        {
            public string? Pattern { get; set; }
            public string? Rule { get; set; }
            public string? Error { get; set; }
            public List<string>? allowedExtensions { get; set; }
        }
    }