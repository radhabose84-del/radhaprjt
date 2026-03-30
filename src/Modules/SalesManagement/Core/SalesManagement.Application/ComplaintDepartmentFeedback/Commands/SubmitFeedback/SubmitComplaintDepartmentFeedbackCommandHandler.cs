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

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.SubmitFeedback
{
    public class SubmitComplaintDepartmentFeedbackCommandHandler : IRequestHandler<SubmitComplaintDepartmentFeedbackCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintDepartmentFeedbackCommandRepository _commandRepository;
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SubmitComplaintDepartmentFeedbackCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(SubmitComplaintDepartmentFeedbackCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ComplaintDepartmentFeedback>(request);

            // Auto-set FeedbackStatus to "Submitted"
            var submittedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.FeedbackStatus, MiscEnumEntity.FeedbackSubmitted);
            entity.FeedbackStatusId = submittedStatus?.Id ?? 0;

            // Auto-set submission audit fields
            var userId = _ipAddressService.GetUserId();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

            entity.SubmittedBy = userId;
            entity.SubmittedDate = currentTime;
            entity.ReworkCount = 0;

            // Map attachments
            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                entity.Attachments = request.Attachments.Select(a => new Domain.Entities.ComplaintFeedbackAttachment
                {
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var newId = await _commandRepository.CreateAsync(entity);

            // Update Assignment status to "Submitted"
            var assignmentSubmittedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.QCAssignmentStatus, MiscEnumEntity.AssignmentSubmitted);
            if (assignmentSubmittedStatus != null)
            {
                await _commandRepository.UpdateAssignmentStatusAsync(request.AssignmentId, assignmentSubmittedStatus.Id);
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COMPLAINT_FEEDBACK_SUBMIT",
                actionName: newId.ToString(),
                details: $"Department feedback submitted for AssignmentId {request.AssignmentId} with Id {newId}.",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Feedback submitted successfully.",
                Data = newId
            };
        }
    }
}
