using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Queries.GetAllGatePass
{
    public class GetAllGatePassQueryHandler : IRequestHandler<GetAllGatePassQuery, ApiResponseDTO<List<GatePassHdrDto>>>
    {
        private readonly IGatePassQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllGatePassQueryHandler(IGatePassQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GatePassHdrDto>>> Handle(GetAllGatePassQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllGatePassQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "GatePass details were fetched.",
                module: "GatePass"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GatePassHdrDto>>
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
