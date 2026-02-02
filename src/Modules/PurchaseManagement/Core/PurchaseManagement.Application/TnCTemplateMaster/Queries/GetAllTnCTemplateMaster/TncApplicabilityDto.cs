using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster
{
    public class TncApplicabilityDto
    {
        public int Id { get; set; }            // TnCTemplateApplicability.Id  (junction PK)
        public int TnCTemplateMasterId { get; set; } // FK to master (optional but handy)
        public int ApplicabilityId { get; set; }   // FK -> MiscMaster.Id
        public string Code { get; set; } = null!;  // from MiscMaster
        public string Description { get; set; } = null!;
    }
}