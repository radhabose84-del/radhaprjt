using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbackById
{
    public class GetComplaintDepartmentFeedbackByIdQueryHandler : IRequestHandler<GetComplaintDepartmentFeedbackByIdQuery, ApiResponseDTO<ComplaintDepartmentFeedbackDto>>
    {
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetComplaintDepartmentFeedbackByIdQueryHandler(
            IComplaintDepartmentFeedbackQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<ComplaintDepartmentFeedbackDto>> Handle(GetComplaintDepartmentFeedbackByIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByIdAsync(request.Id);

            if (data == null)
            {
                return new ApiResponseDTO<ComplaintDepartmentFeedbackDto>
                {
                    IsSuccess = false,
                    Message = "Feedback not found.",
                    Data = null
                };
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetComplaintDepartmentFeedbackByIdQuery",
                actionName: data.Id.ToString(),
                details: $"Department feedback {data.Id} was fetched.",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<ComplaintDepartmentFeedbackDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data
            };
        }
    }
}
