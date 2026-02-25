#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesChannel.Queries.GetAllSalesChannel
{
    public class GetAllSalesChannelQueryHandler : IRequestHandler<GetAllSalesChannelQuery, ApiResponseDTO<List<SalesChannelDto>>>
    {
        private readonly ISalesChannelQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesChannelQueryHandler(ISalesChannelQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesChannelDto>>> Handle(GetAllSalesChannelQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var salesChannelDtos = _mapper.Map<List<SalesChannelDto>>(data);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesChannelQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "SalesChannel details were fetched.",
                module: "SalesChannel"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesChannelDto>>
            {
                IsSuccess = true,
                Message = "Sales Channels retrieved successfully.",
                Data = salesChannelDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
