using PartyManagement.Domain.Common;

namespace PartyManagement.Domain.Entities;

public class BankAccount : BaseEntity
{    
    public int BankId { get; set; }
    public string AccountHolderName { get; set; } = null!; 
    public string AccountNumber { get; set; } = null!; 
    public int BranchId { get; set; }
    public MiscMaster? BankAccountMisc { get; set; } = null!;
    public string? IFSCCode { get; set; } 
    public string? SWIFTCode { get; set; } 
    public int AccountTypeId { get; set; } 
    public MiscMaster? BankAccountType { get; set; } = null!;
    public bool IsDefaultAccount { get; set; }
    public bool IsPrimaryAccount { get; set; }
    public string? IBan { get; set; } 
}