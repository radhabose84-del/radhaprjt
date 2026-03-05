namespace SalesManagement.Application.OfficerAgent.Dto
{
    public class OfficerAgentItemDto
    {
        public int AssignmentId { get; set; }
        public int AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? AgentMobile { get; set; }

        public DateOnly ValidityFrom { get; set; }
        public DateOnly ValidityTo { get; set; }
        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
