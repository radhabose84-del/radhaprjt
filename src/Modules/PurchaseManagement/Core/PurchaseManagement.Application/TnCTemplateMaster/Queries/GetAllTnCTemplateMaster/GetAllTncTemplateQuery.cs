using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster
{
    public class GetAllTncTemplateQuery  : IRequest <ApiResponseDTO<List<TncTemplateMasterDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }    
    }
}