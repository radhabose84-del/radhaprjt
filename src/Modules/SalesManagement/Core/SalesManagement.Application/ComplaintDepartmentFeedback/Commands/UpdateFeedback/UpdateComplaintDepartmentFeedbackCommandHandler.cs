using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UpdateFeedback
{
    public class UpdateComplaintDepartmentFeedbackCommandHandler : IRequestHandler<UpdateComplaintDepartmentFeedbackCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintDepartmentFeedbackCommandRepository _commandRepository;
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateComplaintDepartmentFeedbackCommandHandler(
            IComplaintDepartmentFeedbackCommandRepository commandRepository,
            IComplaintDepartmentFeedbackQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateComplaintDepartmentFeedbackCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ComplaintDepartmentFeedback>(request);

            // On re-submit after rework: reset status to "Submitted" and refresh SubmittedDate
            var (reworkCount, currentStatusId) = await _queryRepository.GetReworkInfoAsync(request.Id);

            var submittedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FeedbackStatus, MiscEnumEntity.FeedbackSubmitted);
            var reworkStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FeedbackStatus, MiscEnumEntity.FeedbackReworkRequired);

            // If current status is "Rework Required", re-submitting sets it back to "Submitted"
            if (reworkStatus != null && currentStatusId == reworkStatus.Id)
            {
                entity.FeedbackStatusId = submittedStatus?.Id ?? 0;
                var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
                var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);
                entity.SubmittedBy = _ipAddressService.GetUserId();
                entity.SubmittedDate = currentTime;

                // Update Assignment status back to "Submitted"
                var assignmentId = await _queryRepository.GetAssignmentIdByFeedbackIdAsync(request.Id);
                var assignmentSubmittedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.QCAssignmentStatus, MiscEnumEntity.AssignmentSubmitted);
                if (assignmentSubmittedStatus != null)
                {
                    await _commandRepository.UpdateAssignmentStatusAsync(assignmentId, assignmentSubmittedStatus.Id);
                }
            }
            else
            {
                entity.FeedbackStatusId = submittedStatus?.Id ?? 0;
            }

            entity.ReworkCount = reworkCount;

            // Map attachments
            ICollection<Domain.Entities.ComplaintFeedbackAttachment>? attachments = null;
            if (request.Attachments != null)
            {
                attachments = request.Attachments.Select(a => new Domain.Entities.ComplaintFeedbackAttachment
                {
                    Id = a.Id ?? 0,
                    FeedbackId = request.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var result = await _commandRepository.UpdateAsync(entity, attachments);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "COMPLAINT_FEEDBACK_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Department feedback with Id {request.Id} updated successfully.",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Feedback updated successfully.",
                Data = result
            };
        }
    }
}
