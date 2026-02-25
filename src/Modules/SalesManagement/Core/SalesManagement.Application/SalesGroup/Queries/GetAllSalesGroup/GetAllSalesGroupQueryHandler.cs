#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesGroup.Queries.GetAllSalesGroup
{
    public class GetAllSalesGroupQueryHandler : IRequestHandler<GetAllSalesGroupQuery, ApiResponseDTO<List<SalesGroupDto>>>
    {
        private readonly ISalesGroupQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesGroupQueryHandler(ISalesGroupQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesGroupDto>>> Handle(GetAllSalesGroupQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<SalesGroupDto>>(data);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesGroupQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "SalesGroup details were fetched.",
                module: "SalesGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesGroupDto>>
            {
                IsSuccess = true,
                Message = "Sales Groups retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
