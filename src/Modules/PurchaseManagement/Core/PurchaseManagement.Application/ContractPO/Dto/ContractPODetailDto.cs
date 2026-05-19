namespace PurchaseManagement.Application.ContractPO.Dto;

public class ContractPODetailDto
{
    public int Id { get; set; }
    public int ContractPOHeaderId { get; set; }
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public int UOMId { get; set; }
    public string? UOMName { get; set; }
    public decimal ContractQuantity { get; set; }
    public decimal ContractRate { get; set; }
    public decimal ContractValue { get; set; }
    public decimal UtilizedQuantity { get; set; }
    public decimal BalanceQuantity { get; set; }
    public decimal UtilizedValue { get; set; }
    public decimal BalanceValue { get; set; }
    public int? HSNId { get; set; }
    public string? HSNCode { get; set; }
    public decimal? GSTPercentage { get; set; }
}
