using System.Globalization;

namespace QCManagement.Application.Common.Services
{
    /// <summary>
    /// Pure, backend-authoritative evaluation of the 6 validation types
    /// (RNG / MIN / MAX / FIX / PFL / LST). Fully unit-testable.
    /// </summary>
    public sealed class InspectionEvaluator : IInspectionEvaluator
    {
        public const string Pass = "PASS";
        public const string Fail = "FAIL";

        public string? Evaluate(
            string? validationTypeCode,
            string? actualValue,
            decimal? minValue,
            decimal? maxValue,
            string? expectedValue,
            string? allowedValues)
        {
            if (string.IsNullOrWhiteSpace(actualValue))
                return null; // incomplete — not yet evaluated

            var actual = actualValue.Trim();

            switch ((validationTypeCode ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "RNG":
                    return TryNum(actual, out var rv) && minValue.HasValue && maxValue.HasValue
                        && rv >= minValue.Value && rv <= maxValue.Value ? Pass : Fail;

                case "MIN":
                    return TryNum(actual, out var mn) && minValue.HasValue && mn >= minValue.Value ? Pass : Fail;

                case "MAX":
                    return TryNum(actual, out var mx) && maxValue.HasValue && mx <= maxValue.Value ? Pass : Fail;

                case "FIX":
                    return !string.IsNullOrWhiteSpace(expectedValue)
                        && actual.Equals(expectedValue.Trim(), StringComparison.OrdinalIgnoreCase) ? Pass : Fail;

                case "PFL":
                    var u = actual.ToUpperInvariant();
                    return (u == Pass || u == Fail) ? u : Fail;

                case "LST":
                    if (string.IsNullOrWhiteSpace(allowedValues))
                        return Fail;
                    return allowedValues
                        .Split('|', StringSplitOptions.RemoveEmptyEntries)
                        .Any(v => v.Trim().Equals(actual, StringComparison.OrdinalIgnoreCase)) ? Pass : Fail;

                default:
                    return Fail;
            }
        }

        private static bool TryNum(string s, out decimal d) =>
            decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d);
    }
}
