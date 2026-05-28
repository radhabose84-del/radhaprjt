namespace QCManagement.Application.Common.Interfaces.IQualityParameter
{
    public interface IQualityParameterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.QualityParameter entity);
        Task<int> UpdateAsync(Domain.Entities.QualityParameter entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task<int> GetMaxParameterCodeSequenceAsync();
    }
}
