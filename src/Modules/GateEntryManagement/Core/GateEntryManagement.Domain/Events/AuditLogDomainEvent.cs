using MediatR;

namespace GateEntryManagement.Domain.Events
{
    public class AuditLogsDomainEvent : INotification
    {
        public string? ActionDetail { get; set; }
        public string? ActionCode { get; set; }
        public string? ActionName { get; set; }
        public string? Details { get; set; }
        public string? Module { get; set; }

        public AuditLogsDomainEvent(
            string actionDetail,
            string actionCode,
            string actionName,
            string details,
            string module)
        {
            ActionDetail = actionDetail;
            ActionCode = actionCode;
            ActionName = actionName;
            Details = details;
            Module = module;
        }
    }
}
