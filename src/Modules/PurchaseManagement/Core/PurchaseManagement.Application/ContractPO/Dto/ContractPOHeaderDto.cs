namespace PurchaseManagement.Application.ContractPO.Dto;

public class ContractPOHeaderDto
{
    public int Id { get; set; }
    public string? ContractPONumber { get; set; }
    public int UnitId { get; set; }
    public DateTimeOffset ContractDate { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public int CurrencyId { get; set; }
    public string? CurrencyName { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public decimal TotalContractValue { get; set; }
    public decimal UtilizedValue { get; set; }
    public decimal BalanceValue { get; set; }
    public int StatusId { get; set; }
    public string? StatusName { get; set; }
    public string? Remarks { get; set; }
    public int IsActive { get; set; }
    public int IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public string? CreatedIP { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedByName { get; set; }
    public string? ModifiedIP { get; set; }

    // Nested details (populated in GetById)
    public List<ContractPODetailDto>? Details { get; set; }

    // Release history (populated in GetById)
    public List<ContractPOReleaseHistoryDto>? ReleaseHistory { get; set; }
}
