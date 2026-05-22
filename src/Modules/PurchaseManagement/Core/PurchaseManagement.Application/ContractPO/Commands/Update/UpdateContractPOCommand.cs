using MediatR;
using PurchaseManagement.Application.ContractPO.Dto;
using Contracts.Common;

namespace PurchaseManagement.Application.ContractPO.Commands.Update;

public sealed class UpdateContractPOCommand : IRequest<ContractPOHeaderDto>, IRequirePermission
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public int CurrencyId { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public int StatusId { get; set; }
    public string? Remarks { get; set; }
    public int IsActive { get; set; }
    public List<UpdateContractPODetailItem> Details { get; set; } = new();
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
}

public sealed class UpdateContractPODetailItem
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
