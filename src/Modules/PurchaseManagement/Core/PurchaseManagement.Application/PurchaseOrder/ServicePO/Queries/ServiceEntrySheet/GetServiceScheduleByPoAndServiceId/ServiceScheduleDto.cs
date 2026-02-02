using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServiceScheduleByPoAndServiceId
{
    public class ServiceScheduleDto
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ServicePoHeaderId { get; set; }
        public int ServiceItemId { get; set; }
        public int ScheduleNo { get; set; }
        public string? OccurrencePeriod { get; set; }
        public string? OccurrenceDescription { get; set; }
        public int? ActivityTypeId { get; set; }
        public string? ActivityType { get; set; }
        public decimal? PlannedDurationHrs { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset? ServiceStartDate { get; set; }
        public DateTimeOffset? ServiceEndDate { get; set; }
        public decimal? PlannedQuantity { get; set; }
        public decimal? PlannedRate { get; set; }
        public decimal? PlannedValue { get; set; }
        public bool AutoGenerateSES { get; set; }
        public string? Remarks { get; set; }
        public string? SESAlreadyGenerated { get; set; }    
    }
}