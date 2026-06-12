using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.MixCodeMaster.Queries.GetAllMixCodeMaster
{
    public class GetAllMixCodeMasterQueryHandler : IRequestHandler<GetAllMixCodeMasterQuery, ApiResponseDTO<List<MixCodeMasterDto>>>
    {
        private readonly IMixCodeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllMixCodeMasterQueryHandler(IMixCodeMasterQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<MixCodeMasterDto>>> Handle(GetAllMixCodeMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllMixCodeMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "MixCodeMaster details were fetched.",
                module: "MixCodeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<MixCodeMasterDto>>
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
