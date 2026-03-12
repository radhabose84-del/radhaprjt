using GateEntryManagement.Application.Common.Interfaces;
using GateEntryManagement.Domain.Entities;
using GateEntryManagement.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace GateEntryManagement.Application.EventHandlers
{
    public class DomainEventHandler : INotificationHandler<AuditLogsDomainEvent>
    {
        private readonly IMongoDbContext _mongoDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DomainEventHandler(IMongoDbContext mongoDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _mongoDbContext = mongoDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Handle(AuditLogsDomainEvent notification, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var auditLog = new AuditLogs
            {
                Action = notification.ActionDetail,
                Details = notification.Details,
                Module = notification.Module,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetClaimInt("UserId"),
                CreatedByName = GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"),
                IPAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                MachineName = Environment.MachineName,
                OS = httpContext?.Request.Headers["User-Agent"].ToString(),
                Browser = httpContext?.Request.Headers["User-Agent"].ToString()
            };

            await _mongoDbContext.GetCollection<AuditLogs>("AuditLogs").InsertOneAsync(auditLog, null, cancellationToken);
        }

        private string? GetClaim(string claimType)
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        }

        private int GetClaimInt(string claimType)
        {
            var value = GetClaim(claimType);
            return int.TryParse(value, out var result) ? result : 0;
        }
    }
}
