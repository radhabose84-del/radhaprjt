using System;
using System.Collections.Generic;
using System.Linq;

namespace  Shared.Validation.Common
{
    public sealed class JsonValidationRuleProvider : IValidationRuleProvider
    {
        public IReadOnlyList<ValidationRule> GetRules()
        {
            var rules = ValidationRuleLoader.LoadValidationRules();
            // if (rules is null || !rules.Any())
            //     throw new InvalidOperationException("Validation rules could not be loaded.");
               // ✅ Provide fallback if JSON or DB is empty
            if (rules is null || !rules.Any())
            {
                rules = new List<ValidationRule>
                {
                    new ValidationRule { Rule = "NotEmpty", Error = "cannot be empty" },
                    new ValidationRule { Rule = "MaxLength", Error = "exceeds maximum length" }
                };
            }

            return rules;
        }
    }
}
