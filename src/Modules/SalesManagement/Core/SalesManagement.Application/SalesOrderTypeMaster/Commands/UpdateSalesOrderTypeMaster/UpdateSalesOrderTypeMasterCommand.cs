using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOrderTypeMaster.Commands.UpdateSalesOrderTypeMaster
{
    public class UpdateSalesOrderTypeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }

        // Identification (SoTypeId + TaxTypeId NOT included — immutable after create)
        public string? TypeName { get; set; }
        public string? Description { get; set; }

        // Type behavior
        public bool AllowsDispatch { get; set; }
        public bool RequiresValidity { get; set; }
        public bool AllowZeroPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MaxQty { get; set; }
        public bool AllowPriceOverride { get; set; }
        public decimal? OverrideLimitPercent { get; set; }
        public bool ApprovalRequired { get; set; }

        // Mode behavior
        public bool CurrencyRequired { get; set; }
        public bool AllowIGST { get; set; }
        public bool CountryMandatory { get; set; }
        public int? DefaultCurrencyId { get; set; }

        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
