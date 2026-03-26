using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Application.CountGroup.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountGroup.Queries.GetAllCountGroup
{
    public class GetAllCountGroupQueryHandler : IRequestHandler<GetAllCountGroupQuery, ApiResponseDTO<List<CountGroupDto>>>
    {
        private readonly ICountGroupQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllCountGroupQueryHandler(
            ICountGroupQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CountGroupDto>>> Handle(GetAllCountGroupQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<CountGroupDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllCountGroupQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Count Group details were fetched.",
                module: "CountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<CountGroupDto>>
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
