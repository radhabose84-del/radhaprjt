using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;

public sealed class DbUniqueToValidationFilter : IExceptionFilter
{
    private readonly IWebHostEnvironment _env;

    public DbUniqueToValidationFilter(IWebHostEnvironment env)
    {
        _env = env;
    }

    public void OnException(ExceptionContext context)
    {
        var sqlEx = FindSqlException(context.Exception);
        if (sqlEx is null) return;

        var number = sqlEx.Number;
        var msg = sqlEx.Message ?? string.Empty;

        var problem = new ValidationProblemDetails
        {
            Title = "Validation failed.",
            Status = StatusCodes.Status400BadRequest
        };

        if (number is 2627 or 2601) // UNIQUE / duplicate key
        {
            var indexOrConstraintName = ExtractIndexOrConstraintName(msg);

            // ✅ Try to pull a friendly message from DB (Extended Property)
            // This is still generic because it doesn't hardcode index names.
            string? friendly = null;
            if (!string.IsNullOrWhiteSpace(indexOrConstraintName))
            {
                friendly = TryGetUniqueMessageFromDb(indexOrConstraintName);
            }

            var finalMessage = !string.IsNullOrWhiteSpace(friendly)
                ? friendly!
                : "Duplicate record. This combination must be unique.";

            // Optional: show tuple only in Development
            if (_env.IsDevelopment())
            {
                var tuple = ExtractDuplicateTuple(msg);
                if (!string.IsNullOrWhiteSpace(tuple))
                    finalMessage += $" (Duplicate key: {tuple})";
            }

            // Key should be common and clean, not "Uniq,Month"
            problem.Errors.Add("Duplicate", new[] { finalMessage });

            context.Result = new ObjectResult(problem)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            context.ExceptionHandled = true;
            return;
        }

        // NOT NULL
        if (number == 515)
        {
            var col = Regex.Match(msg, @"Cannot insert the value NULL into column '([^']+)'",
                    RegexOptions.IgnoreCase)
                .Groups[1].Value;

            var field = string.IsNullOrWhiteSpace(col) ? "General" : col;
            problem.Errors.Add(field, new[]
            {
                $"{(string.IsNullOrWhiteSpace(col) ? "A required field" : $"'{col}'")} cannot be empty."
            });

            context.Result = new ObjectResult(problem) { StatusCode = 400 };
            context.ExceptionHandled = true;
            return;
        }

        // FK
        if (number == 547)
        {
            problem.Errors.Add("Reference", new[]
            {
                "Invalid reference value. Please select a valid record."
            });

            context.Result = new ObjectResult(problem) { StatusCode = 400 };
            context.ExceptionHandled = true;
            return;
        }

        // Other SQL -> 500
        var dbProblem = new ValidationProblemDetails
        {
            Title = "Database error.",
            Status = StatusCodes.Status500InternalServerError
        };

        dbProblem.Errors.Add("Database", new[]
        {
            _env.IsDevelopment() ? sqlEx.Message : "An unexpected database error occurred."
        });

        context.Result = new ObjectResult(dbProblem)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = true;
    }

    private static SqlException? FindSqlException(Exception ex)
    {
        var current = ex;
        while (current != null)
        {
            if (current is SqlException sqlEx) return sqlEx;
            current = current.InnerException!;
        }
        return null;
    }

    private static string? ExtractIndexOrConstraintName(string sqlMessage)
    {
        var m = Regex.Match(sqlMessage, @"(?:index|constraint) '([^']+)'", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value : null;
    }

    private static string? ExtractDuplicateTuple(string sqlMessage)
    {
        var m = Regex.Match(sqlMessage, @"duplicate key value is \((.*?)\)", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value : null;
    }
    private static string? TryGetUniqueMessageFromDb(string indexOrConstraintName)
    {
        var cs = Environment.GetEnvironmentVariable("DefaultConnection");
        if (string.IsNullOrWhiteSpace(cs)) return null;

        const string sql = @"
            DECLARE @msg NVARCHAR(4000);

            -- Try index extended property
            SELECT @msg = CAST(ep.value AS NVARCHAR(4000))
            FROM sys.indexes i
            JOIN sys.objects o ON o.object_id = i.object_id
            LEFT JOIN sys.extended_properties ep
                ON ep.major_id = i.object_id
            AND ep.minor_id = i.index_id
            AND ep.name = 'UX_MESSAGE'
            WHERE i.name = @name;

            -- Try constraint extended property (unique constraint)
            IF (@msg IS NULL)
            BEGIN
                SELECT @msg = CAST(ep.value AS NVARCHAR(4000))
                FROM sys.key_constraints kc
                JOIN sys.objects o ON o.object_id = kc.parent_object_id
                LEFT JOIN sys.extended_properties ep
                    ON ep.major_id = kc.parent_object_id
                AND ep.minor_id = kc.unique_index_id
                AND ep.name = 'UX_MESSAGE'
                WHERE kc.name = @name;
            END

            SELECT @msg;
            ";

        try
        {
            using var con = new SqlConnection(cs);
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@name", indexOrConstraintName);
            con.Open();
            var result = cmd.ExecuteScalar();
            return result as string;
        }
        catch
        {
            return null;
        }
    }
}
