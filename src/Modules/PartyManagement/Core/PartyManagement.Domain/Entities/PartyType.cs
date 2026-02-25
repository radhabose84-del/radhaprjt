namespace PartyManagement.Domain.Entities
{
    public class PartyType
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public PartyMaster Party { get; set; } = null!;
        public int PartyTypeId { get; set; }
        public MiscMaster PartyTypeMisc { get; set; } = null!;
        public int PartyGroupId { get; set; }
        public PartyGroup PartyGroup { get; set; } = null!;
    
    }
}