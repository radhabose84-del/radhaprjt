namespace PartyManagement.Application.PartyMaster.Command.CreatePartyMaster
{
    public class PartyMasterWorkFlowDto
    {
        public int Id { get; set; }
        public string? PartyName { get; set; }
        public string? PartyCode { get; set; }
        public int StatusId { get; set; }  
        public string? PartyStatus { get; set; }
        public int UnitId { get; set; }
       
    }
}