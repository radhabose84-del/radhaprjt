using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO
{
    public class ServiceEntrySheetDocument : BaseEntity
    {
        public int Id { get; set; }
        public int ServiceEntrySheetId { get; set; }
        public ServiceEntrySheet ServiceEntrySheet { get; set; } = null!;

        public int DocumentId { get; set; }          // DocumentType (MiscMaster)
        public string FileName { get; set; } = null!;
        public DateTimeOffset UploadedDate { get; set; }

        public string? UploadedPath { get; set; }
        public string? DocumentName { get; set; }
        
    }
}