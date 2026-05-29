using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;

namespace PurchaseManagement.Application.BlanketMaster.Commands.Create;

public sealed class CreateBlanketMasterCommand : IRequest<BlanketHeaderDto>
{
    public DateTimeOffset BlanketDate { get; set; }
    public int VendorId { get; set; }
    public int CurrencyId { get; set; }
    public int ProcurementTypeId { get; set; }
    public string? BrokerName { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryTerms { get; set; }
    public string? Remarks { get; set; }
    public List<CreateBlanketMasterDetailItem> Details { get; set; } = new();
}

public sealed class CreateBlanketMasterDetailItem
{
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public int UOMId { get; set; }
    public decimal EstimatedQuantity { get; set; }
    public decimal Rate { get; set; }
    public int? HSNId { get; set; }
    public decimal? GSTPercentage { get; set; }
    public string? QualitySpecification { get; set; }
    public List<CreateBlanketMasterScheduleItem> Schedules { get; set; } = new();
}

public sealed class CreateBlanketMasterScheduleItem
{
    public int ScheduleNo { get; set; }
    public DateTimeOffset ScheduleDate { get; set; }
    public decimal ScheduleQuantity { get; set; }
    public string? Remarks { get; set; }
}
