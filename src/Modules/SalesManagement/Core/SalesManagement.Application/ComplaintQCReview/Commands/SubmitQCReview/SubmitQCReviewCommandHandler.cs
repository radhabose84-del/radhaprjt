using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.ComplaintQCReview.Commands.SubmitQCReview
{
    public class SubmitQCReviewCommandHandler : IRequestHandler<SubmitQCReviewCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintQCReviewCommandRepository _commandRepository;
        private readonly IComplaintQCReviewQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SubmitQCReviewCommandHandler(
            IComplaintQCReviewCommandRepository commandRepository,
            IComplaintQCReviewQueryRepository queryRepository,
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

        public async Task<ApiResponseDTO<int>> Handle(SubmitQCReviewCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ComplaintQCReview>(request);

            // Set QC Review audit fields
            var userId = _ipAddressService.GetUserId();
            var userName = _ipAddressService.GetUserName();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

            entity.ReviewedBy = userId;
            entity.ReviewedDate = currentTime;

            // Set decision timestamp if a decision was made
            if (request.ComplaintStatusId.HasValue && request.ComplaintStatusId.Value > 0)
            {
                entity.DecisionTimestamp = currentTime;
            }

            // Map assignments
            if (request.Assignments != null && request.Assignments.Count > 0)
            {
                // Resolve default "Pending" status for assignments
                var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.QCAssignmentStatus, MiscEnumEntity.AssignmentPending);

                entity.Assignments = request.Assignments.Select(a => new ComplaintQCReviewAssignment
                {
                    RoleId = a.RoleId,
                    ResponsiblePersonId = a.ResponsiblePersonId,
                    IsMandatory = a.IsMandatory,
                    AssignmentStatusId = pendingStatus?.Id ?? 0,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COMPLAINT_QC_REVIEW_SUBMIT",
                actionName: newId.ToString(),
                details: $"QC Review submitted for Complaint HeaderId {request.ComplaintHeaderId} with Id {newId}.",
                module: "ComplaintQCReview");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "QC Review submitted successfully.",
                Data = newId
            };
        }
    }
}
