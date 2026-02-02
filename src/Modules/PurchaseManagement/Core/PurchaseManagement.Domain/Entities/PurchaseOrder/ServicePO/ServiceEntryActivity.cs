using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO
{
    public class ServiceEntryActivity : BaseEntity
    {
        public int EntrySheetId { get; set; }
        public ServiceEntrySheet EntrySheet { get; set; } = default!;
        public int? ActivityTypeId { get; set; }
        public MiscMaster? ActivityType { get; set; }  // Navigation property to MiscMaster for Activity Type       
        public string? Description { get; set; }
                public string? PerformedByName { get; set; }  // Optionally denormalize name here for historical reference      
        public int? SESActivityStatusId { get; set; }
        public MiscMaster? SESActivityStatus { get; set; }  // Navigation property to SES Activity Status
        public string? StatusRemarks { get; set; }
        



    }
}