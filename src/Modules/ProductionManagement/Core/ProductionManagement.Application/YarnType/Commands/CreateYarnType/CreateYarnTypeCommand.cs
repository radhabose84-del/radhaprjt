using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.YarnType.Commands.CreateYarnType
{
    public class CreateYarnTypeCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? YarnTypeCode { get; set; }
        public string? YarnTypeName { get; set; }
        public string? Description { get; set; }
        public decimal? AdditionalPrice { get; set; }
        public int? CurrencyId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
