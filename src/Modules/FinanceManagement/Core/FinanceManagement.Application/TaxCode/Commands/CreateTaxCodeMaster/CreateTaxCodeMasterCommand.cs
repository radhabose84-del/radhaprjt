using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster
{
    public class CreateTaxCodeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public string? TaxCode { get; set; }            // GST-OUT-5 (immutable after create)
        public string? TaxName { get; set; }
        public int TaxTypeId { get; set; }              // FK -> MiscMaster (TAX TYPE)
        public int? TaxComponentId { get; set; }        // FK -> MiscMaster (TAX COMPONENT)
        public int? ParentTaxCodeId { get; set; }
        public int? DirectionId { get; set; }           // FK -> MiscMaster (TAX DIRECTION)
        public string? StatutorySection { get; set; }
        public decimal? ThresholdAmount { get; set; }
        public decimal? ThresholdAggregate { get; set; }
        public string? HsnSacCode { get; set; }
        public bool IsSystemOnlyPosting { get; set; }
        public bool IsEefcRelevant { get; set; }
        public bool IsStatutoryFixed { get; set; }

        // Initial rate version (VersionNo 1)
        public decimal RatePercent { get; set; }
        public DateOnly EffectiveFrom { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
