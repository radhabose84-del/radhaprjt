using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoTypeMaster.Queries.GetAllStoTypeMaster
{
    public class GetAllStoTypeMasterQueryHandler : IRequestHandler<GetAllStoTypeMasterQuery, ApiResponseDTO<List<StoTypeMasterDto>>>
    {
        private readonly IStoTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllStoTypeMasterQueryHandler(IStoTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<StoTypeMasterDto>>> Handle(GetAllStoTypeMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<StoTypeMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllStoTypeMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "StoTypeMaster details were fetched.",
                module: "StoTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<StoTypeMasterDto>>
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
