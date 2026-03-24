using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoHeader.Queries.GetPendingStoHeader
{
    public class GetPendingStoHeaderQueryHandler : IRequestHandler<GetPendingStoHeaderQuery, ApiResponseDTO<List<StoHeaderDto>>>
    {
        private readonly IStoHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPendingStoHeaderQueryHandler(
            IStoHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<StoHeaderDto>>> Handle(GetPendingStoHeaderQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetPendingAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPendingStoHeader",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Pending STO headers were fetched.",
                module: "StoHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<StoHeaderDto>>
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
