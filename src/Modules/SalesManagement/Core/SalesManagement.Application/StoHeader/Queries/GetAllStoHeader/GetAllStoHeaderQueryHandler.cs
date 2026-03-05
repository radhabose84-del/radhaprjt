using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoHeader.Queries.GetAllStoHeader
{
    public class GetAllStoHeaderQueryHandler : IRequestHandler<GetAllStoHeaderQuery, ApiResponseDTO<List<StoHeaderDto>>>
    {
        private readonly IStoHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllStoHeaderQueryHandler(IStoHeaderQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<StoHeaderDto>>> Handle(GetAllStoHeaderQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var dtos = _mapper.Map<List<StoHeaderDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllStoHeaderQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "StoHeader details were fetched.",
                module: "StoHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<StoHeaderDto>>
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
