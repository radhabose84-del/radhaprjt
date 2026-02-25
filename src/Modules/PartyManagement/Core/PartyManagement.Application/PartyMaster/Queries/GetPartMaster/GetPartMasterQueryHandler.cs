using AutoMapper;
using Contracts.Common;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartMaster
{
    public class GetPartMasterQueryHandler : IRequestHandler<GetPartMasterQuery, ApiResponseDTO<List<GetPartyMasterDto>>>
    {
        private readonly IPartyMasterQueryRepository _ipartyMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPartMasterQueryHandler(IPartyMasterQueryRepository ipartyMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _ipartyMasterQueryRepository = ipartyMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GetPartyMasterDto>>> Handle(GetPartMasterQuery request, CancellationToken cancellationToken)
        {
            var (PartyMaster, totalCount) = await _ipartyMasterQueryRepository.GetAllPartyMasterAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var partyMasterDtos = _mapper.Map<List<GetPartyMasterDto>>(PartyMaster);
              // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPartMasterQuery",
                actionCode: "Get",
                actionName: PartyMaster.Count().ToString(),
                details: "PartyMaster details were fetched.",
                module: "PartyMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // ✅ Return
            return new ApiResponseDTO<List<GetPartyMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = partyMasterDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}