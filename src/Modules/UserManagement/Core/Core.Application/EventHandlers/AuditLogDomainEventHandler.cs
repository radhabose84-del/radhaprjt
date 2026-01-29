using MediatR;
using Core.Application.Common.Interfaces;
using Core.Application.Common;
using Core.Domain.Events;

namespace Core.Application.EventHandlers
{
    public class DomainEventHandler : INotificationHandler<AuditLogsDomainEvent>
    {
        private readonly IMongoDbContext _mongoDbContext;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;

        public DomainEventHandler(IMongoDbContext mongoDbContext, IIPAddressService ipAddressService, ITimeZoneService timeZoneService)
        {
            _mongoDbContext = mongoDbContext;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;  
        }

        public async Task Handle(AuditLogsDomainEvent notification, CancellationToken cancellationToken)
        {          
            var userIdString = _ipAddressService.GetUserId();   
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId); 
            
            var auditLog = new
            {                
                Module = notification.Module,   
                Action = notification.ActionDetail,
                Details = notification.Details,             
                MachineName = Environment.MachineName,
                OS = _ipAddressService.GetUserOS(),
                IPAddress = _ipAddressService.GetSystemIPAddress(),
                Browser = _ipAddressService.GetUserAgent(),
                CreatedAt = currentTime,
                CreatedBy =userIdString,
                CreatedByName=_ipAddressService.GetUserName(),
            };

            await _mongoDbContext.GetCollection<dynamic>("AuditLogs").InsertOneAsync(auditLog, cancellationToken: cancellationToken);
        }
    }
}
