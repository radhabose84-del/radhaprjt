using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using PurchaseManagement.Domain.Entities;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand
{
    public class CreateTnCTemplateMasterCommand  : IRequest<int>
    {
       // public string? TemplateCode { get; set; }
        public string TemplateName { get; set; } = null!;
        public int TemplateTypeId { get; set; }
        public string TermsHtml { get; set; } = null!;
        public bool? ApprovalFlag { get; set; }
        
        public List<TncApplicabilityDto>? Applicabilities { get; set; } 
        
       
    }
}