using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Queries.GetAllPriceGroupMaster
{
    public class GetAllPriceGroupMasterQueryHandler : IRequestHandler<GetAllPriceGroupMasterQuery, ApiResponseDTO<List<PriceGroupMasterDto>>>
    {
        private readonly IPriceGroupMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllPriceGroupMasterQueryHandler(
            IPriceGroupMasterQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<PriceGroupMasterDto>>> Handle(GetAllPriceGroupMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "PRICEGROUP_GETALL",
                actionName: data.Count.ToString(),
                details: "Price Group records were fetched.",
                module: "PriceGroupMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<List<PriceGroupMasterDto>>
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
