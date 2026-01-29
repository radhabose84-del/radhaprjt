using System.Collections.Generic;

namespace Shared.Validation.Common
{
    public interface IValidationRuleProvider
    {
        IReadOnlyList<ValidationRule> GetRules();
    }
}
