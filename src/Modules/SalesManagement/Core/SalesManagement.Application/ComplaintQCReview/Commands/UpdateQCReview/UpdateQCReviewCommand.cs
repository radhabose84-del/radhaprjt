using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintQCReview.Dto;

namespace SalesManagement.Application.ComplaintQCReview.Commands.UpdateQCReview
{
    public class UpdateQCReviewCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int PhysicalVerificationId { get; set; }
        public int? ComplaintStatusId { get; set; }
        public int? SeverityId { get; set; }
        public int? CompensationStructureId { get; set; }
        public bool LabVerificationRequired { get; set; }
        public int? LabResponsiblePersonId { get; set; }
        public DateOnly? ExpectedResolutionDate { get; set; }
        public string? Comments { get; set; }
        public int IsActive { get; set; }
        public List<SubmitAssignmentDto>? Assignments { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
