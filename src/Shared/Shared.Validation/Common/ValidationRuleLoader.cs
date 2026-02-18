#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Shared.Validation.Common
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
                    // Try multiple possible locations
                    var possiblePaths = new[]
                    {
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Validation", "Common", "validation-rules.json"),
                        Path.Combine(Directory.GetCurrentDirectory(), "Validation", "Common", "validation-rules.json"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "validation-rules.json"),
                        Path.Combine(Directory.GetCurrentDirectory(), "validation-rules.json")
                    };

                    rulesJsonPath = possiblePaths.FirstOrDefault(File.Exists);

                    if (string.IsNullOrEmpty(rulesJsonPath))
                    {
                        var searchedPaths = string.Join("\n", possiblePaths);
                        throw new FileNotFoundException(
                            $"Validation rules file not found. Searched in:\n{searchedPaths}\n\n" +
                            $"Current Directory: {Directory.GetCurrentDirectory()}\n" +
                            $"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}"
                        );
                    }
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
        public string Pattern { get; set; } = default!;
        public string Rule { get; set; } = default!;
        public string Error { get; set; } = default!;
        public List<string> allowedExtensions { get; set; } = default!;
    }
}