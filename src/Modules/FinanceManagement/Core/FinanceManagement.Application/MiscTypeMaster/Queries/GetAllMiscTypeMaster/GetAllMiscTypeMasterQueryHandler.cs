using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using FinanceManagement.Application.MiscTypeMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster
{
    public class GetAllMiscTypeMasterQueryHandler : IRequestHandler<GetAllMiscTypeMasterQuery, ApiResponseDTO<List<MiscTypeMasterDto>>>
    {
        private readonly IMiscTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllMiscTypeMasterQueryHandler(IMiscTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<MiscTypeMasterDto>>> Handle(GetAllMiscTypeMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var miscTypeMasterDtos = _mapper.Map<List<MiscTypeMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllMiscTypeMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "MiscTypeMaster details were fetched.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<MiscTypeMasterDto>>
            {
                IsSuccess = true,
                Message = "Misc Type Master list retrieved successfully.",
                Data = miscTypeMasterDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
