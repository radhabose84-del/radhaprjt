using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete
{
    public class TnCTemplateAutoCompleteQuery : IRequest<List<TnCAutoCompleteDto>>
    {

        public int? TemplateTypeId { get; set; }     // 28=Purchase, 29=Sales (from your MiscMaster)
        public int? ApplicabilityId { get; set; }    // 30=RFQ, 32=PO, ...
        public string? SearchPattern { get; set; } 
        
    }
}