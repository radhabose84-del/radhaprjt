using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.UpdateTaxCodeMaster
{
    public class UpdateTaxCodeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        // TaxCode and TaxType are immutable — excluded.
        public string? TaxName { get; set; }
        public string? TaxComponent { get; set; }
        public string? Direction { get; set; }
        public string? StatutorySection { get; set; }
        public decimal? ThresholdAmount { get; set; }
        public decimal? ThresholdAggregate { get; set; }
        public string? HsnSacCode { get; set; }
        public bool IsSystemOnlyPosting { get; set; }
        public bool IsEefcRelevant { get; set; }
        public bool IsStatutoryFixed { get; set; }
        public int IsActive { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
