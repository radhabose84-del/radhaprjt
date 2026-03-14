using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountMaster.Queries.GetAllCountMaster
{
    public class GetAllCountMasterQueryHandler : IRequestHandler<GetAllCountMasterQuery, ApiResponseDTO<List<CountMasterDto>>>
    {
        private readonly ICountMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllCountMasterQueryHandler(
            ICountMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CountMasterDto>>> Handle(GetAllCountMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<CountMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllCountMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Count Master details were fetched.",
                module: "CountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<CountMasterDto>>
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
