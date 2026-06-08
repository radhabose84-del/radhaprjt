using Contracts.Dtos.Lookups.QC;
using MediatR;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCRQualityTemplateParameters
{
    /// <summary>
    /// Returns the active parameters of a QC Quality Template so the OCR form can render the
    /// cotton-quality parameter inputs dynamically.
    /// </summary>
    public sealed record GetOCRQualityTemplateParametersQuery(int QualityTemplateId)
        : IRequest<IReadOnlyList<QualityTemplateParameterLookupDto>>;
}
