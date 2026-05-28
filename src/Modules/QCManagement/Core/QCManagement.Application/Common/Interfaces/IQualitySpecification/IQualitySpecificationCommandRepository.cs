namespace QCManagement.Application.Common.Interfaces.IQualitySpecification
{
    public interface IQualitySpecificationCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.QualitySpecification entity);
        Task<int> UpdateAsync(Domain.Entities.QualitySpecification entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
