using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.RequestRework
{
    public class RequestReworkCommandHandler : IRequestHandler<RequestReworkCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintDepartmentFeedbackCommandRepository _commandRepository;
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMediator _mediator;

        public RequestReworkCommandHandler(
            IComplaintDepartmentFeedbackCommandRepository commandRepository,
            IComplaintDepartmentFeedbackQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(RequestReworkCommand request, CancellationToken cancellationToken)
        {
            // Get current rework info
            var (reworkCount, _) = await _queryRepository.GetReworkInfoAsync(request.FeedbackId);

            // Set status to "Rework Required"
            var reworkStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FeedbackStatus, MiscEnumEntity.FeedbackReworkRequired);

            var newReworkCount = reworkCount + 1;
            var result = await _commandRepository.UpdateStatusAsync(
                request.FeedbackId,
                reworkStatus?.Id ?? 0,
                request.ReworkReason,
                newReworkCount);

            // Update Assignment status back to "Pending"
            var assignmentId = await _queryRepository.GetAssignmentIdByFeedbackIdAsync(request.FeedbackId);
            var assignmentPendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.QCAssignmentStatus, MiscEnumEntity.AssignmentPending);
            if (assignmentPendingStatus != null)
            {
                await _commandRepository.UpdateAssignmentStatusAsync(assignmentId, assignmentPendingStatus.Id);
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Rework",
                actionCode: "COMPLAINT_FEEDBACK_REWORK",
                actionName: request.FeedbackId.ToString(),
                details: $"Rework requested for feedback Id {request.FeedbackId}. Rework count: {newReworkCount}. Reason: {request.ReworkReason}",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Rework requested successfully.",
                Data = result
            };
        }
    }
}
