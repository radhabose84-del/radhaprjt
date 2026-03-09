#nullable disable
using System.Text;
using System.Text.RegularExpressions;
using Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;

/// <summary>
/// Global MVC action filter that intercepts SQL Server exceptions thrown during
/// EF Core operations and converts them to user-friendly ApiResponseDTO responses.
///
/// SQL error codes handled:
///   2627 / 2601 – UNIQUE constraint / duplicate key  → 400
///   515         – NOT NULL constraint                → 400
///   547         – FOREIGN KEY constraint             → 400
///   2628 / 8152 – String or binary data truncation  → 400
///   other       – Unexpected database error          → 500
/// </summary>
public sealed class DbUniqueToValidationFilter : IExceptionFilter
{
    private readonly IHostEnvironment _env;

    public DbUniqueToValidationFilter(IHostEnvironment env)
    {
        _env = env;
    }

    public void OnException(ExceptionContext context)
    {
        var sqlEx = FindSqlException(context.Exception);
        if (sqlEx is null) return;

        var (statusCode, errors, message) = Classify(sqlEx, _env.IsDevelopment());

        var response = new ApiResponseDTO<object>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors
        };

        context.Result = new ObjectResult(response) { StatusCode = statusCode };
        context.ExceptionHandled = true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Classification logic
    // ─────────────────────────────────────────────────────────────────────────

    private static (int statusCode, List<string> errors, string message) Classify(
        SqlException sqlEx, bool isDevelopment)
    {
        var number = sqlEx.Number;
        var msg = sqlEx.Message ?? string.Empty;

        // 1. UNIQUE constraint / duplicate key (2627 = primary key, 2601 = unique index)
        if (number is 2627 or 2601)
        {
            var indexName   = ExtractMatch(msg, @"(?:index|constraint) '([^']+)'");
            var tupleValues = ExtractMatch(msg, @"duplicate key value is \((.*?)\)");

            string fieldLabel = "value";
            if (!string.IsNullOrWhiteSpace(indexName))
            {
                // Convention: IX_TableName_Col1_Col2 → "Col 1, Col 2"
                var parts = indexName.Split('_', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 2)
                    fieldLabel = string.Join(", ", parts.Skip(2).Select(Humanize));
            }

            string detail;
            if (!string.IsNullOrWhiteSpace(tupleValues))
                detail = $"'{fieldLabel}' with value ({tupleValues}) already exists. Please use a unique value.";
            else
                detail = $"'{fieldLabel}' already exists. Please use a unique value.";

            return (400, new List<string> { detail }, $"Duplicate '{fieldLabel}' detected.");
        }

        // 2. NOT NULL constraint
        if (number == 515)
        {
            var col   = ExtractMatch(msg, @"NULL into column '([^']+)'");
            var label = string.IsNullOrWhiteSpace(col) ? "A required field" : $"'{Humanize(col)}'";

            return (400, new List<string> { $"{label} cannot be empty." }, "Required field is missing.");
        }

        // 3. FOREIGN KEY constraint
        if (number == 547)
        {
            var constraintName = ExtractMatch(msg, @"FOREIGN KEY constraint ""([^""]+)""");
            var columnName     = ExtractMatch(msg, @"column '([^']+)'");

            // Use last segment of FK name as field: FK_Table_ParentTable_CapitalTypeId → CapitalTypeId
            var key   = constraintName?.Split('_').LastOrDefault() ?? columnName ?? "value";
            var label = Humanize(key);

            return (400, new List<string> { $"Invalid {label}. Please select a valid {label}." },
                "Invalid reference value.");
        }

        // 4. String or binary data truncation (2628 = newer, 8152 = classic)
        if (number is 2628 or 8152)
        {
            var col   = ExtractMatch(msg, @"column '([^']+)'");
            var label = string.IsNullOrWhiteSpace(col) ? "A field" : $"'{Humanize(col)}'";

            return (400, new List<string> { $"{label} value exceeds the maximum allowed length." },
                "Value too long.");
        }

        // 5. Fallback – any other SQL error → 500
        var detail500 = isDevelopment
            ? $"[SQL {number}] {sqlEx.Message}"
            : "An unexpected database error occurred. Please contact support.";

        return (500, new List<string> { detail500 }, "Database error.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Walks the exception chain to find the innermost SqlException.</summary>
    private static SqlException FindSqlException(Exception ex)
    {
        var current = ex;
        while (current is not null)
        {
            if (current is SqlException sqlEx) return sqlEx;
            current = current.InnerException;
        }
        return null;
    }

    /// <summary>Returns the first capture group of the regex, or null.</summary>
    private static string ExtractMatch(string input, string pattern)
    {
        var m = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value : null;
    }

    /// <summary>
    /// Converts a DB identifier to a readable label.
    /// "CapitalTypeId" → "Capital Type"
    /// "required_date" → "Required Date"
    /// </summary>
    private static string Humanize(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier)) return identifier;

        // Drop trailing "Id"
        var name = identifier.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
            ? identifier[..^2]
            : identifier;

        // Replace underscores with space
        name = name.Replace('_', ' ');

        // Insert space before each uppercase letter in PascalCase
        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (i > 0 && char.IsUpper(c) && !char.IsWhiteSpace(name[i - 1]))
                sb.Append(' ');
            sb.Append(c);
        }

        return sb.ToString().Trim();
    }
}
