using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MovementTypeConfig.Queries.GetAllMovementTypeConfig
{
    public class GetAllMovementTypeConfigQueryHandler : IRequestHandler<GetAllMovementTypeConfigQuery, ApiResponseDTO<List<MovementTypeConfigDto>>>
    {
        private readonly IMovementTypeConfigQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllMovementTypeConfigQueryHandler(
            IMovementTypeConfigQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<MovementTypeConfigDto>>> Handle(GetAllMovementTypeConfigQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<MovementTypeConfigDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllMovementTypeConfigQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "MovementTypeConfig details were fetched.",
                module: "MovementTypeConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<MovementTypeConfigDto>>
            {
                IsSuccess = true,
                Message = "Movement Type Configurations retrieved successfully.",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
