using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetMyPendingFeedbacks
{
    public class GetMyPendingFeedbacksQueryHandler : IRequestHandler<GetMyPendingFeedbacksQuery, ApiResponseDTO<List<MyPendingFeedbackDto>>>
    {
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetMyPendingFeedbacksQueryHandler(
            IComplaintDepartmentFeedbackQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<MyPendingFeedbackDto>>> Handle(GetMyPendingFeedbacksQuery request, CancellationToken cancellationToken)
        {
            var userId = _ipAddressService.GetUserId();
            var data = await _queryRepository.GetMyPendingAsync(userId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetMyPending",
                actionCode: "GetMyPendingFeedbacksQuery",
                actionName: data.Count.ToString(),
                details: $"Pending feedbacks for user {userId} were fetched.",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<MyPendingFeedbackDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
