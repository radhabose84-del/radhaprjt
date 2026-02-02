using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public sealed class DbUniqueToValidationFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        // If you only want to handle EF DB update errors, uncomment this:
        // if (context.Exception is not DbUpdateException) return;

        var sqlEx = FindSqlException(context.Exception);
        if (sqlEx is null) return;

        var number = sqlEx.Number;
        var msg = sqlEx.Message ?? string.Empty;

        var problem = new ValidationProblemDetails
        {
            Title = "Validation failed.",
            Status = StatusCodes.Status400BadRequest
        };

        // 1) UNIQUE constraint / duplicate key
        if (number is 2627 or 2601)
        {
            // Example SQL Server message:
            var idxMatch = Regex.Match(
                msg,
                @"(?:index|constraint) '([^']+)'",
                RegexOptions.IgnoreCase);

            var indexName = idxMatch.Success ? idxMatch.Groups[1].Value : null;

            var tupleMatch = Regex.Match(
                msg,
                @"duplicate key value is \((.*?)\)",
                RegexOptions.IgnoreCase);

            var tupleValues = tupleMatch.Success ? tupleMatch.Groups[1].Value : null;

            string fieldKey = "General";
            string fieldLabel = "value";

            string[] columnParts = Array.Empty<string>();

            if (!string.IsNullOrWhiteSpace(indexName))
            {
                // Split: IX_RfqItem_RfqId_ItemId_UomId_RequiredDate
                var parts = indexName.Split('_', StringSplitOptions.RemoveEmptyEntries);

                // Convention: [0] = IX/UX, [1] = TableName, [2...] = columns
                columnParts = parts.Length > 2
                    ? parts.Skip(2).ToArray()
                    : Array.Empty<string>();

                if (columnParts.Length > 0)
                {
                    // Key used in Errors dictionary (you can tweak this if needed)
                    fieldKey = string.Join(",", columnParts);

                    // Human readable label: "Rfq, Item, Uom, Required Date"
                    fieldLabel = string.Join(", ",
                        columnParts.Select(HumanizeFkName));
                }
            }

            var baseMessage = columnParts.Length == 0
                ? "Duplicate value violates a unique constraint."
                : $"Duplicate {fieldLabel}. The combination of {fieldLabel} must be unique.";

            if (!string.IsNullOrWhiteSpace(tupleValues))
            {
                baseMessage += $" Duplicate key value: ({tupleValues}).";
            }

            problem.Errors.Add(fieldKey, new[] { baseMessage });
        }
        // 2) NOT NULL constraint
        else if (number == 515)
        {            
            var col = Regex.Match(
                    msg,
                    @"Cannot insert the value NULL into column '([^']+)'",
                    RegexOptions.IgnoreCase)
                .Groups[1]
                .Value;

            var field = string.IsNullOrWhiteSpace(col) ? "General" : col;

            problem.Errors.Add(
                field,
                new[]
                {
                    $"{(string.IsNullOrWhiteSpace(col) ? "A required field" : $"'{col}'")} cannot be null."
                });
        }
        // 3) FK constraint
        else if (number == 547)
        {
            var constraintName = Regex.Match(
                    msg,
                    @"FOREIGN KEY constraint ""([^""]+)""",
                    RegexOptions.IgnoreCase)
                .Groups[1]
                .Value;

            var columnName = Regex.Match(
                    msg,
                    @"column '([^']+)'",
                    RegexOptions.IgnoreCase)
                .Groups[1]
                .Value;

            // Decide which key to use in the ValidationProblemDetails.Errors:           
            string key = "General";

            if (!string.IsNullOrWhiteSpace(constraintName))
            {
                var parts = constraintName.Split('_', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                    key = parts[^1]; // e.g. CapitalTypeId
            }
            else if (!string.IsNullOrWhiteSpace(columnName))
            {
                key = columnName; // e.g. Id (fallback)
            }

            var label = HumanizeFkName(key); // e.g. "Capital Type"

            problem.Errors.Add(
                key,
                new[] { $"Invalid {label}. Please select a valid {label}." }
            );
        }
        // 4) Fallback for other SQL errors (e.g. 207 Invalid column name)
        else
        {
            var dbProblem = new ValidationProblemDetails
            {
                Title = "Database error.",
                Status = StatusCodes.Status500InternalServerError
            };

            dbProblem.Errors.Add("Database", new[]
            {
                // In Dev/QA it's useful to see the raw SQL message
                sqlEx.Message
            });

            context.Result = new ObjectResult(dbProblem)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            context.ExceptionHandled = true;
            return;
        }

        // For handled validation cases -> 400
        context.Result = new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
        context.ExceptionHandled = true;
    }

    private static SqlException? FindSqlException(Exception ex)
    {
        var current = ex;
        while (current != null)
        {
            if (current is SqlException sqlEx)
                return sqlEx;

            current = current.InnerException;
        }

        return null;
    }

    private static string HumanizeFkName(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return key;

        var name = key;

        // If it ends with "Id", drop that for display: CapitalTypeId -> CapitalType
        if (name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
            name = name.Substring(0, name.Length - 2);

        // Replace _ with space: Some_Field_Name -> Some Field Name
        name = name.Replace('_', ' ');

        // Insert spaces before capital letters: CapitalType -> Capital Type
        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (i > 0 && char.IsUpper(c) && !char.IsWhiteSpace(name[i - 1]))
                sb.Append(' ');

            sb.Append(c);
        }

        return sb.ToString().Trim();
    }
}
