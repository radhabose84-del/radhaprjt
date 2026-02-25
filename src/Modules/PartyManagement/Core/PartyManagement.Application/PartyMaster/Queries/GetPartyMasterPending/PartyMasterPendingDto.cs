namespace PartyManagement.Application.PartyMaster.Queries.GetPartyMasterPending
{
    public class PartyMasterPendingDto
    {
        public int Id { get; set; }
        public string PartyCode { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string RegistrationType { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;
        public string PAN { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Party_GroupType { get; set; } = string.Empty;
        public string PartyStatus { get; set; } = string.Empty;
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public byte IsPortalAccessEnabled { get; set; }
        public byte IsUpdate { get; set; }
        public byte IsEdit { get; set; }


    }
}