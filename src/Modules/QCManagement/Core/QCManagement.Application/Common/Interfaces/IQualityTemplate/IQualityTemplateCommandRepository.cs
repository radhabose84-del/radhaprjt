namespace QCManagement.Application.Common.Interfaces.IQualityTemplate
{
    public interface IQualityTemplateCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.QualityTemplate entity);
        Task<int> UpdateAsync(Domain.Entities.QualityTemplate entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
