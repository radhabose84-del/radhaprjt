using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProductionPack.Queries.GetProductionStockRegister
{
    public class GetProductionStockRegisterQueryHandler
        : IRequestHandler<GetProductionStockRegisterQuery, ApiResponseDTO<List<ProductionStockRegisterDto>>>
    {
        private readonly IProductionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetProductionStockRegisterQueryHandler(
            IProductionQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ProductionStockRegisterDto>>> Handle(
            GetProductionStockRegisterQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetProductionStockRegisterAsync(
                request.FromDate, request.ToDate, request.LotId, request.ItemId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetProductionStockRegister",
                actionCode: "GetProductionStockRegisterQuery",
                actionName: data.Count.ToString(),
                details: $"Production stock register from {request.FromDate} to {request.ToDate} was fetched.",
                module: "Production"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ProductionStockRegisterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
