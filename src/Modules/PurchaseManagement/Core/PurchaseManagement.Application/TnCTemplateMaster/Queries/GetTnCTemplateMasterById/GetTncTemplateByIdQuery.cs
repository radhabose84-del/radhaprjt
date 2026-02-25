using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterById
{
    public class GetTncTemplateByIdQuery : IRequest<TncTemplateMasterDto>
    {
        public  int Id { get; set; }
        
    }
}