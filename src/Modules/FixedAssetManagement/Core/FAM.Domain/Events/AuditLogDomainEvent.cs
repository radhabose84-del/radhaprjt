using MediatR;

namespace FAM.Domain.Events
{
    public class AuditLogsDomainEvent : INotification
    {
        public string ActionDetail { get; }
        public string ActionCode { get; }
        public string ActionName { get; }
        public string Details { get; }
        public string Module { get; }
        public AuditLogsDomainEvent(string? actionDetail, string? actionCode, string? actionName, string? details, string? module)
        {
            ActionDetail = actionDetail ?? string.Empty;
            ActionCode = actionCode ?? string.Empty;
            ActionName = actionName ?? string.Empty;
            Details = details ?? string.Empty;
            Module = module ?? string.Empty;
        }

    }
}
