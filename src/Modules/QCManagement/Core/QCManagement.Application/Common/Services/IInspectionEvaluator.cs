namespace QCManagement.Application.Common.Services
{
    public interface IInspectionEvaluator
    {
        /// <summary>
        /// Evaluates a single parameter against its validation rule.
        /// Returns "PASS" / "FAIL", or null when the actual value is blank (incomplete).
        /// </summary>
        string? Evaluate(
            string? validationTypeCode,
            string? actualValue,
            decimal? minValue,
            decimal? maxValue,
            string? expectedValue,
            string? allowedValues);
    }
}
