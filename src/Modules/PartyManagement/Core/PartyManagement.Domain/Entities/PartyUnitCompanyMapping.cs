namespace PartyManagement.Domain.Entities
{
    public class PartyUnitCompanyMapping
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public PartyMaster PartyUnitCompany { get; set; } = null!;
        public int CompanyId { get; set; }
        public int UnitId { get; set; }
                
    }
}