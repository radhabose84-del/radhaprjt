using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;

namespace PurchaseManagement.Application.BlanketMaster.Commands.Update;

public sealed class UpdateBlanketMasterCommand : IRequest<BlanketHeaderDto>
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public int CurrencyId { get; set; }
    public int ProcurementTypeId { get; set; }
    public string? BrokerName { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public int StatusId { get; set; }
    public string? Remarks { get; set; }
    public int IsActive { get; set; }
    public List<UpdateBlanketMasterDetailItem> Details { get; set; } = new();
}

public sealed class UpdateBlanketMasterDetailItem
{
    public int Id { get; set; }
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public int UOMId { get; set; }
    public decimal EstimatedQuantity { get; set; }
    public decimal Rate { get; set; }
    public int? HSNId { get; set; }
    public decimal? GSTPercentage { get; set; }
    public string? QualitySpecification { get; set; }
    public List<UpdateBlanketMasterScheduleItem> Schedules { get; set; } = new();
}

public sealed class UpdateBlanketMasterScheduleItem
{
    public int Id { get; set; }
    public int ScheduleNo { get; set; }
    public DateTimeOffset ScheduleDate { get; set; }
    public decimal ScheduleQuantity { get; set; }
    public string? Remarks { get; set; }
}
