using FinanceManagement.Domain.Events;
using MediatR;
using System.Text.Json;

namespace FinanceManagement.Application.Common
{
    public static class AuditLogPublisher
    {
        public static async Task PublishAuditLogAsync<T>(
            IMediator mediator,
            T data,
            string actionDetail,
            string actionCode,
            string actionName,
            string module,
            CancellationToken cancellationToken = default)
        {
            var details = JsonSerializer.Serialize(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: actionDetail,
                actionCode: actionCode,
                actionName: actionName,
                details: details,
                module: module
            );

            await mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
