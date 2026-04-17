using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ILeadConversionFunnel;
using SalesManagement.Application.LeadConversionFunnel.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.LeadConversionFunnel.Queries.GetLeadConversionFunnel
{
    public class GetLeadConversionFunnelQueryHandler
        : IRequestHandler<GetLeadConversionFunnelQuery, ApiResponseDTO<LeadConversionFunnelDto>>
    {
        private readonly ILeadConversionFunnelRepository _repository;
        private readonly IMediator _mediator;

        public GetLeadConversionFunnelQueryHandler(
            ILeadConversionFunnelRepository repository,
            IMediator mediator)
        {
            _repository = repository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<LeadConversionFunnelDto>> Handle(
            GetLeadConversionFunnelQuery request,
            CancellationToken cancellationToken)
        {
            var data = await _repository.GetFunnelAsync(cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetLeadConversionFunnel",
                actionCode: "Get",
                actionName: data.Officers.Count.ToString(),
                details: "Lead conversion funnel details were fetched.",
                module: "LeadConversionFunnel"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<LeadConversionFunnelDto>
            {
                IsSuccess = true,
                Message = "Lead conversion funnel retrieved successfully.",
                Data = data
            };
        }
    }
}
