using AutoMapper;
using Contracts.Common;
using PartyManagement.Application.Common.Interfaces.IPartyGroup;
using PartyManagement.Domain.Events;
using MediatR;

namespace PartyManagement.Application.PartyGroup.Queries.GetPartyGroup
{
    public class GetPartyGroupQueryHandler : IRequestHandler<GetPartyGroupQuery, ApiResponseDTO<List<PartyGroupDto>>>
    {
        private readonly IPartyGroupQueryRepository _ipartygroupQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPartyGroupQueryHandler(IPartyGroupQueryRepository ipartygroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _ipartygroupQueryRepository = ipartygroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<PartyGroupDto>>> Handle(GetPartyGroupQuery request, CancellationToken cancellationToken)
        {
            var (PartyGroups, totalCount) = await _ipartygroupQueryRepository.GetAllPartyGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var partyGroupDtos = _mapper.Map<List<PartyGroupDto>>(PartyGroups);
              // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPartyGroupQuery",
                actionCode: "Get",
                actionName: PartyGroups.Count().ToString(),
                details: "PartyGroup details were fetched.",
                module: "PartyGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // ✅ Return
            return new ApiResponseDTO<List<PartyGroupDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = partyGroupDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}