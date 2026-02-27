using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesLead.Queries.GetAllSalesLead
{
    public class GetAllSalesLeadQueryHandler : IRequestHandler<GetAllSalesLeadQuery, ApiResponseDTO<List<SalesLeadDto>>>
    {
        private readonly ISalesLeadQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesLeadQueryHandler(ISalesLeadQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesLeadDto>>> Handle(GetAllSalesLeadQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<SalesLeadDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesLeadQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "SalesLead details were fetched.",
                module: "SalesLead"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesLeadDto>>
            {
                IsSuccess = true,
                Message = "Sales Leads retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
