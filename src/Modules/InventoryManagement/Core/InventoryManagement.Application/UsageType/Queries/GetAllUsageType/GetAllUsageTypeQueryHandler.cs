using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Application.UsageType.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UsageType.Queries.GetAllUsageType
{
    public class GetAllUsageTypeQueryHandler : IRequestHandler<GetAllUsageTypeQuery, ApiResponseDTO<List<UsageTypeDto>>>
    {
        private readonly IUsageTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllUsageTypeQueryHandler(IUsageTypeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<UsageTypeDto>>> Handle(GetAllUsageTypeQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<UsageTypeDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllUsageTypeQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "UsageType details were fetched.",
                module: "UsageType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<UsageTypeDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
