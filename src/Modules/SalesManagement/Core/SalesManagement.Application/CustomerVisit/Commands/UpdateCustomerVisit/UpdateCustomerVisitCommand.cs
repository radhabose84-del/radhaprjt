using Contracts.Common;
using MediatR;
using SalesManagement.Application.CustomerVisit.Dto;

namespace SalesManagement.Application.CustomerVisit.Commands.UpdateCustomerVisit
{
    public class UpdateCustomerVisitCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int VisitTypeId { get; set; }
        public DateTimeOffset VisitDateTime { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? ImageName { get; set; }
        public string? Remarks { get; set; }
        public int MarketingOfficerId { get; set; }
        public int IsActive { get; set; }

        // Detail list (replaces existing)
        public List<CreateCustomerVisitProductDto>? Products { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
