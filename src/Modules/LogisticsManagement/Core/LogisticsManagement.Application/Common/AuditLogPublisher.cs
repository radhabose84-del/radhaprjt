using System.Text.Json;
using LogisticsManagement.Domain.Events;
using MediatR;

namespace LogisticsManagement.Application.Common
{
    public static class AuditLogPublisher
    {
        public static async Task PublishAuditLogAsync<T>(
        IMediator mediator,
        string actionDetail,
        string actionCode,
        string actionName,
        string module,
        T requestData,
        CancellationToken cancellationToken)
          {
              var serializedRequest = JsonSerializer.Serialize(requestData);

              var domainEvent = new AuditLogsDomainEvent(
                  actionDetail: actionDetail,
                  actionCode: actionCode,
                  actionName: actionName,
                  details: $"{module} request: {serializedRequest}",
                  module: module
              );

              await mediator.Publish(domainEvent, cancellationToken);
          }
    }
}
