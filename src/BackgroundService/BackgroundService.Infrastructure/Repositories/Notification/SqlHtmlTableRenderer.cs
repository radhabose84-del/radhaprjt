using System.Data;
using System.Text.Json;
using BackgroundService.Application.Interfaces.Notification;
using BackgroundService.Application.Notification.Common.Models;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Repositories.Notification
{
    public sealed class SqlHtmlTableRenderer : IHtmlTableRenderer
    {
        private readonly IDbConnection _db;
        private readonly ILogger<SqlHtmlTableRenderer> _logger;

        public SqlHtmlTableRenderer(
            [FromKeyedServices("Notification")] IDbConnection db,
            ILogger<SqlHtmlTableRenderer> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<string> RenderAsync(HtmlTableSpec spec, CancellationToken ct = default)
        {
            var specJson = JsonSerializer.Serialize(spec, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var p = new DynamicParameters();
            p.Add("@Spec", specJson);

            try
            {
                return await _db.ExecuteScalarAsync<string>(
                    new CommandDefinition("[Sp_BuildHtmlTableFromJson]", p,
                        commandType: CommandType.StoredProcedure, cancellationToken: ct)
                ) ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RenderAsync failed.");
                return string.Empty;
            }
        }

        public async Task<string> RenderFromTemplateAsync(int templateId, string rowsJson, CancellationToken ct = default)
        {
            var p = new DynamicParameters();
            p.Add("@TemplateId", templateId);
            p.Add("@RowsJson", rowsJson);

            try
            {
                return await _db.ExecuteScalarAsync<string>(
                    new CommandDefinition("[Sp_RenderHtmlTableFromTemplate]", p,
                        commandType: CommandType.StoredProcedure, cancellationToken: ct)
                ) ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RenderFromTemplateAsync failed for TemplateId {TemplateId}.", templateId);
                return string.Empty;
            }
        }
    }
}
