using PartyManagement.Domain.Common;

namespace PartyManagement.Domain.Entities;

public class BankMaster : BaseEntity
{ 
    public string BankCode { get; set; } = default!;
    public string BankName { get; set; } = default!;   
}