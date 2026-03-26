using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Queries.GetAllGateInward
{
    public class GetAllGateInwardQueryHandler : IRequestHandler<GetAllGateInwardQuery, ApiResponseDTO<List<GateInwardHdrDto>>>
    {
        private readonly IGateInwardQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllGateInwardQueryHandler(IGateInwardQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GateInwardHdrDto>>> Handle(GetAllGateInwardQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllGateInwardQuery", actionCode: "Get",
                actionName: data.Count.ToString(), details: "GateInward details were fetched.", module: "GateInward");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GateInwardHdrDto>>
            {
                IsSuccess = true, Message = "Success", Data = data,
                TotalCount = totalCount, PageNumber = request.PageNumber, PageSize = request.PageSize
            };
        }
    }
}
