using MediatR;

namespace Core.Domain.Events
{
    public class AuditLogsDomainEvent : INotification
    {
        public string ActionDetail { get; }
        public string ActionCode { get; }
        public string ActionName { get; }
        public string Details { get; }
        public string Module { get; }
        public AuditLogsDomainEvent(string actionDetail, string actionCode, string actionName, string details,string module)
        {
            ActionDetail = actionDetail;
            ActionCode = actionCode;
            ActionName = actionName;
            Details = details;
            Module=module;
        }

    }
}
