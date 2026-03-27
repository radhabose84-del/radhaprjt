using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Application.YarnType.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnType.Queries.GetAllYarnType
{
    public class GetAllYarnTypeQueryHandler : IRequestHandler<GetAllYarnTypeQuery, ApiResponseDTO<List<YarnTypeDto>>>
    {
        private readonly IYarnTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllYarnTypeQueryHandler(
            IYarnTypeQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<YarnTypeDto>>> Handle(GetAllYarnTypeQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<YarnTypeDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllYarnTypeQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Yarn Type details were fetched.",
                module: "YarnType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<YarnTypeDto>>
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
