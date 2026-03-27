using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.ComplaintQCReview.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintQCReview.Queries.GetAllQCReview
{
    public class GetAllQCReviewQueryHandler : IRequestHandler<GetAllQCReviewQuery, ApiResponseDTO<List<QCReviewListDto>>>
    {
        private readonly IComplaintQCReviewQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllQCReviewQueryHandler(
            IComplaintQCReviewQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<QCReviewListDto>>> Handle(GetAllQCReviewQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm, request.StatusFilter);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllQCReviewQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "QC Review list was fetched.",
                module: "ComplaintQCReview");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<QCReviewListDto>>
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
