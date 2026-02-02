using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class TnCTemplateApplicability : BaseEntity
    {
        
        public int TnCTemplateMasterId { get; set; }
        public TnCTemplateMaster TnCTemplate { get; set; } = null!;

        // points to Misc item like RFQ/PO/SO/Invoice etc.
        public int ApplicabilityId { get; set; }
        public MiscMaster Applicability { get; set; } = null!;


    }
}