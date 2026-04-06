using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.ComplaintResolution.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintResolution.Queries.GetAllResolution
{
    public class GetAllResolutionQueryHandler : IRequestHandler<GetAllResolutionQuery, ApiResponseDTO<List<ResolutionListDto>>>
    {
        private readonly IComplaintResolutionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllResolutionQueryHandler(IComplaintResolutionQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ResolutionListDto>>> Handle(GetAllResolutionQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm, request.StatusFilter);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetAllResolutionQuery",
                actionName: data.Count.ToString(),
                details: "Resolution list was fetched.",
                module: "ComplaintResolution");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ResolutionListDto>>
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
