namespace PartyManagement.Domain.Entities
{
    public class PartyBank
    {
        public int Id { get; set; }   // PK
        public int PartyId { get; set; }     // FK to PartyMaster
        public PartyMaster PartyBankId { get; set; } = null!;            // FK to PartyMaster
        public string? BankName { get; set; }   
        public string? BankAccountNumber { get; set; } // Required       // Required
        public string? BankBranch { get; set; }
        public string? IFSCCode { get; set; }          // Required
        public string? SWIFTCode { get; set; }
        public int? AccountTypeId { get; set; }      // e.g., Savings, Current
        public MiscMaster? BankAccountType { get; set; } = null!;
        public bool IsDefaultAccount { get; set; }
        public bool IsPrimaryAccount { get; set; }
    
    }
}