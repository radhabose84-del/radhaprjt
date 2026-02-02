using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetAllSES
{
    public class GetAllServiceEntryActivityDto
    {
          public int Id { get; set; }
        public int EntrySheetId { get; set; }
        public int? ActivityTypeId { get; set; }
        public string? Description { get; set; }      
        public string? PerformedByName { get; set; }
        public int? SESActivityStatusId { get; set; }
        public string? StatusRemarks { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
    }
}