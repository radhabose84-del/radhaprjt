using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ItemPriceMaster.Queries.GetExMillRateByPaymentTerm
{
    public class GetExMillRateByPaymentTermQueryHandler
        : IRequestHandler<GetExMillRateByPaymentTermQuery, ApiResponseDTO<List<ExMillRateDto>>>
    {
        private readonly IItemPriceMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetExMillRateByPaymentTermQueryHandler(
            IItemPriceMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ExMillRateDto>>> Handle(
            GetExMillRateByPaymentTermQuery request,
            CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetExMillRateByPaymentTermAsync(request.PaymentTermId, request.ItemId, request.SalesSegmentId);

            var dtos = _mapper.Map<List<ExMillRateDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetExMillRateByPaymentTermQuery",
                actionCode: "Get",
                actionName: dtos.Count.ToString(),
                details: $"ExMillRate details fetched for PaymentTermId {request.PaymentTermId}.",
                module: "ItemPriceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ExMillRateDto>>
            {
                IsSuccess = true,
                Message = "ExMillRate details retrieved successfully.",
                Data = dtos,
                TotalCount = dtos.Count
            };
        }
    }
}
