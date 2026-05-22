using MediatR;
using PurchaseManagement.Application.ContractPO.Dto;
using Contracts.Common;

namespace PurchaseManagement.Application.ContractPO.Commands.Create;

public sealed class CreateContractPOCommand : IRequest<ContractPOHeaderDto>, IRequirePermission
{
    public int UnitId { get; set; }
    public DateTimeOffset ContractDate { get; set; }
    public int VendorId { get; set; }
    public int CurrencyId { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public int StatusId { get; set; }
    public string? Remarks { get; set; }
    public List<CreateContractPODetailItem> Details { get; set; } = new();
    public PermissionType RequiredPermission => PermissionType.CanAdd;
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
