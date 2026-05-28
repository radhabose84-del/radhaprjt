using QCManagement.Application.QualityParameter.Dto;

namespace QCManagement.Application.Common.Interfaces.IQualityParameter
{
    public interface IQualityParameterQueryRepository
    {
        Task<(List<QualityParameterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? parameterGroupId = null);
        Task<QualityParameterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<QualityParameterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string parameterName, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ParameterGroupExistsAsync(int parameterGroupId);
        Task<bool> DataTypeExistsAsync(int dataTypeId);
        Task<bool> ValidationTypeExistsAsync(int validationTypeId);
        Task<bool> IsUomRequiredForDataTypeAsync(int dataTypeId);
        Task<int?> GetDataTypeIdByQualityParameterIdAsync(int qualityParameterId);
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}
