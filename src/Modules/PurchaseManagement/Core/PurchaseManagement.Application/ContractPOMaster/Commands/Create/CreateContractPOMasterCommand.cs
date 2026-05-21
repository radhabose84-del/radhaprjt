using MediatR;
using PurchaseManagement.Application.ContractPOMaster.Dto;

namespace PurchaseManagement.Application.ContractPOMaster.Commands.Create;

public sealed class CreateContractPOMasterCommand : IRequest<ContractPOHeaderDto>
{
    public DateTimeOffset ContractDate { get; set; }
    public int VendorId { get; set; }
    public int CurrencyId { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public int StatusId { get; set; }
    public string? Remarks { get; set; }
    public List<CreateContractPODetailItem> Details { get; set; } = new();
}

public sealed class CreateContractPODetailItem
{
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public int UOMId { get; set; }
    public decimal ContractQuantity { get; set; }
    public decimal ContractRate { get; set; }
    public int? HSNId { get; set; }
    public decimal? GSTPercentage { get; set; }
}
