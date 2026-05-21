using MediatR;
using PurchaseManagement.Application.ContractPOMaster.Dto;

namespace PurchaseManagement.Application.ContractPOMaster.Commands.Update;

public sealed class UpdateContractPOMasterCommand : IRequest<ContractPOHeaderDto>
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public int CurrencyId { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public int StatusId { get; set; }
    public string? Remarks { get; set; }
    public int IsActive { get; set; }
    public List<UpdateContractPOMasterDetailItem> Details { get; set; } = new();
}

public sealed class UpdateContractPOMasterDetailItem
{
    public int Id { get; set; } // 0 = new detail line
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public int UOMId { get; set; }
    public decimal ContractQuantity { get; set; }
    public decimal ContractRate { get; set; }
    public int? HSNId { get; set; }
    public decimal? GSTPercentage { get; set; }
}
