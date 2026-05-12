using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesAgreement.Dto;

namespace SalesManagement.Application.SalesAgreement.Commands.UpdateSalesAgreement
{
    public class UpdateSalesAgreementCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }
        public int CustomerId { get; set; }
        public int SalesGroupId { get; set; }
        public int PaymentTermsId { get; set; }
        public string? Remarks { get; set; }
        public int StatusId { get; set; }
        public int IsActive { get; set; }
        public List<UpdateSalesAgreementDetailDto>? SalesAgreementDetails { get; set; }
    }
}
