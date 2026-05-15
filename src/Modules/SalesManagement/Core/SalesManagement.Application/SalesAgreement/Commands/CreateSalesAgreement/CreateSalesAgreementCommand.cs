using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesAgreement.Dto;

namespace SalesManagement.Application.SalesAgreement.Commands.CreateSalesAgreement
{
    public class CreateSalesAgreementCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }
        public int CustomerId { get; set; }
        public int SalesGroupId { get; set; }
        public int PaymentTermsId { get; set; }
        public string? Remarks { get; set; }
        public string? CustomerPoRefno { get; set; }
        public string? AgentPOAttachment { get; set; }
        public List<CreateSalesAgreementDetailDto>? SalesAgreementDetails { get; set; }
    }
}
