using QCManagement.Application.QualitySpecification.Dto;

namespace QCManagement.Application.Common.Interfaces.IQualitySpecification
{
    public interface IQualitySpecificationQueryRepository
    {
        Task<(List<QualitySpecificationListDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm,
            int? qualityTemplateId, int? applicableLevelId, int? qcTypeId,
            int? itemCategoryId, int? itemId, bool? isActive);

        Task<QualitySpecificationDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<QualitySpecificationLookupDto>> AutocompleteAsync(string term, CancellationToken ct);

        Task<bool> AlreadyExistsAsync(string specificationName, int? id = null);
        Task<bool> NotFoundAsync(int id);

        Task<bool> ApplicableLevelExistsAsync(int applicableLevelId);
        Task<bool> QcTypeExistsAsync(int qcTypeId);
        Task<bool> ValidationTypeExistsAsync(int validationTypeId);
        Task<bool> SeverityExistsAsync(int severityId);
        Task<bool> FailureActionExistsAsync(int failureActionId);

        Task<string?> GetApplicableLevelCodeAsync(int applicableLevelId);

        Task<Dictionary<int, string>> GetValidationTypeCodesByIdsAsync(IEnumerable<int> validationTypeIds);

        Task<List<int>> GetExistingParameterRowIdsAsync(int qualitySpecificationId);

        Task<(int? ItemCategoryId, int? ItemId)> GetSpecificationItemContextAsync(int id);

        Task<bool> HasOverlappingActiveSpecAsync(
            int? itemCategoryId, int? itemId,
            DateTimeOffset effectiveFrom, DateTimeOffset? effectiveTo,
            int? excludeSpecId = null);

        Task<int> GetMaxSpecificationCodeSequenceAsync();
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}
