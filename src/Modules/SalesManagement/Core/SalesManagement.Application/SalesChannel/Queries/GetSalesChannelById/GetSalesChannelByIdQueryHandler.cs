#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesChannel.Queries.GetSalesChannelById
{
    public class GetSalesChannelByIdQueryHandler : IRequestHandler<GetSalesChannelByIdQuery, ApiResponseDTO<SalesChannelDto>>
    {
        private readonly ISalesChannelQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesChannelByIdQueryHandler(ISalesChannelQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<SalesChannelDto>> Handle(GetSalesChannelByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var salesChannel = _mapper.Map<SalesChannelDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesChannelByIdQuery",
                actionName: salesChannel.Id.ToString(),
                details: $"SalesChannel details {salesChannel.Id} was fetched.",
                module: "SalesChannel"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<SalesChannelDto>
            {
                IsSuccess = true,
                Message = "Sales Channel retrieved successfully.",
                Data = salesChannel
            };
        }
    }
}
