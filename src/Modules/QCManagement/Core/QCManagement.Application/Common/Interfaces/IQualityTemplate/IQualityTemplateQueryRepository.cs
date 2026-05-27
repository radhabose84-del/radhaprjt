using QCManagement.Application.QualityTemplate.Dto;

namespace QCManagement.Application.Common.Interfaces.IQualityTemplate
{
    public interface IQualityTemplateQueryRepository
    {
        Task<(List<QualityTemplateListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, bool? isActive = null);
        Task<QualityTemplateDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<QualityTemplateLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string templateName, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> QualityParameterExistsAndActiveAsync(int qualityParameterId);
        Task<bool> InspectionMethodExistsAsync(int inspectionMethodId);
        Task<int> GetMaxTemplateCodeSequenceAsync();
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}
