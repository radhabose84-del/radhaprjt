using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.RawMaterialPO.Queries.GetAllRawMaterialPO
{
    public class GetAllRawMaterialPOQueryHandler : IRequestHandler<GetAllRawMaterialPOQuery, ApiResponseDTO<List<RawMaterialPODto>>>
    {
        private readonly IRawMaterialPOQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllRawMaterialPOQueryHandler(IRawMaterialPOQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<RawMaterialPODto>>> Handle(GetAllRawMaterialPOQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.FromDate, request.ToDate);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllRawMaterialPOQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Raw Material PO details were fetched.",
                module: "RawMaterialPO");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<RawMaterialPODto>>
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
