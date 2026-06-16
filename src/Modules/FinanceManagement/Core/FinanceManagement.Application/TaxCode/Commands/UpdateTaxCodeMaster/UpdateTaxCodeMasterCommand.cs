using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.UpdateTaxCodeMaster
{
    public class UpdateTaxCodeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        // TaxCode and TaxType are immutable — excluded.
        public string? TaxName { get; set; }
        public int? TaxComponentId { get; set; }        // FK -> MiscMaster (TAX COMPONENT)
        public int? DirectionId { get; set; }           // FK -> MiscMaster (TAX DIRECTION)
        public string? StatutorySection { get; set; }
        public decimal? ThresholdAmount { get; set; }
        public decimal? ThresholdAggregate { get; set; }
        public string? HsnSacCode { get; set; }
        public bool IsSystemOnlyPosting { get; set; }
        public bool IsEefcRelevant { get; set; }
        public bool IsStatutoryFixed { get; set; }
        public int IsActive { get; set; }

        // Optional rate change (merged from the former rate-version API): when RatePercent
        // and RateEffectiveFrom are supplied, a new effective-dated rate version is created (AC3-A).
        public decimal? RatePercent { get; set; }
        public DateOnly? RateEffectiveFrom { get; set; }
        public string? RateChangeReason { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
