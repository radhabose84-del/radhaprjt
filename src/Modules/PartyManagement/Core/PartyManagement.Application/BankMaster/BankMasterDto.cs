namespace PartyManagement.Application.BankMaster;

public record BankMasterDto(
    int Id, string BankCode, string BankName,
    int IsActive, int IsDeleted, DateTimeOffset? CreatedDate);

public record CreateBankMasterDto(
    string BankName);

public record UpdateBankMasterDto(
    int Id,  string BankName,int IsActive );

public record AutocompleteItemDto(int Id, string Label, string Code);
