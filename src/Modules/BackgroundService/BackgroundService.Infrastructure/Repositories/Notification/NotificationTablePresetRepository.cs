using System.Data;
using BackgroundService.Application.Interfaces.Notification;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Notification
{
   public sealed class NotificationTablePresetRepository : INotificationTablePresetRepository
    {
        private readonly IDbConnection _db;

        public NotificationTablePresetRepository(
            [FromKeyedServices("Notification")] IDbConnection db)
        {
            _db = db;
        }
        public async Task<string?> GetColumnsJsonByTemplateIdAsync(int templateId, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP(1) ColumnsJson
                FROM Notification.TablePresets
                WHERE TemplateId = @tid AND IsActive = 1
                ORDER BY ISNULL(Version, 0) DESC";

            return await _db.ExecuteScalarAsync<string>(
                new CommandDefinition(sql, new { tid = templateId }, cancellationToken: ct));
        }
    }
}
