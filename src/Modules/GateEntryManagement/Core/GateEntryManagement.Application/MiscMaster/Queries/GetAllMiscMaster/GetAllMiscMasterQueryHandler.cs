using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.MiscMaster.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.MiscMaster.Queries.GetAllMiscMaster
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
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
