using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingHeader.Queries.GetAllRepackingHeader
{
    public class GetAllRepackingHeaderQueryHandler
        : IRequestHandler<GetAllRepackingHeaderQuery, ApiResponseDTO<List<RepackingHeaderDto>>>
    {
        private readonly IRepackingHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllRepackingHeaderQueryHandler(
            IRepackingHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<RepackingHeaderDto>>> Handle(
            GetAllRepackingHeaderQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, request.TypeId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllRepackingHeaderQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "RepackingHeader details were fetched.",
                module: "RepackingHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<RepackingHeaderDto>>
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
