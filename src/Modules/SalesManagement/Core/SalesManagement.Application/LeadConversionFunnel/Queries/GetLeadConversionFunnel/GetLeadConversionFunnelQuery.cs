using Contracts.Common;
using MediatR;
using SalesManagement.Application.LeadConversionFunnel.Dto;

namespace SalesManagement.Application.LeadConversionFunnel.Queries.GetLeadConversionFunnel
{
    public class GetLeadConversionFunnelQuery : IRequest<ApiResponseDTO<LeadConversionFunnelDto>>
    {
    }
}
