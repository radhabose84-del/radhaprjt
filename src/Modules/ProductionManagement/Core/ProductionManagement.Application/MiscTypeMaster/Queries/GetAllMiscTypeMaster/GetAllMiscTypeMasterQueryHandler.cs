using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProductionManagement.Application.MiscTypeMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster
{
    public class GetAllMiscTypeMasterQueryHandler : IRequestHandler<GetAllMiscTypeMasterQuery, ApiResponseDTO<List<MiscTypeMasterDto>>>
    {
        private readonly IMiscTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllMiscTypeMasterQueryHandler(
            IMiscTypeMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<MiscTypeMasterDto>>> Handle(GetAllMiscTypeMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<MiscTypeMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllMiscTypeMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Misc Type Master details were fetched.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<MiscTypeMasterDto>>
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
