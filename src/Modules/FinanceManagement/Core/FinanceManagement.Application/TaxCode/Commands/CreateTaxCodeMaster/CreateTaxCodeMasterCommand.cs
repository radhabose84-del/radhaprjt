using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster
{
    public class CreateTaxCodeMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int CompanyId { get; set; }
        public string? TaxCode { get; set; }            // GST-OUT-5 (immutable after create)
        public string? TaxName { get; set; }
        public string? TaxType { get; set; }            // GST_IN / GST_OUT / IGST / TDS / CUSTOMS
        public string? TaxComponent { get; set; }       // COMBINED / CGST / SGST / IGST / CESS / NA
        public int? ParentTaxCodeId { get; set; }
        public string? Direction { get; set; }
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
