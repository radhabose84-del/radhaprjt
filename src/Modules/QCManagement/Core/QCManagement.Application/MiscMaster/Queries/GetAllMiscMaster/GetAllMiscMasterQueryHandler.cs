using AutoMapper;
using Contracts.Common;
using MediatR;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Application.MiscMaster.Dto;
using QCManagement.Domain.Events;

namespace QCManagement.Application.MiscMaster.Queries.GetAllMiscMaster
{
    public class GetAllMiscMasterQueryHandler : IRequestHandler<GetAllMiscMasterQuery, ApiResponseDTO<List<MiscMasterDto>>>
    {
        private readonly IMiscMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllMiscMasterQueryHandler(IMiscMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<MiscMasterDto>>> Handle(GetAllMiscMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.MiscTypeId);

            var miscMasterDtos = _mapper.Map<List<MiscMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllMiscMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "MiscMaster details were fetched.",
                module: "MiscMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<MiscMasterDto>>
            {
                IsSuccess = true,
                Message = "Misc Master list retrieved successfully.",
                Data = miscMasterDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
