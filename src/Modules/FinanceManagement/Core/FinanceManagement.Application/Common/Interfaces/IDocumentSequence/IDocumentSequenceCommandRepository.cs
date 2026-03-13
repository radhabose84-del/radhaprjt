namespace FinanceManagement.Application.Common.Interfaces.IDocumentSequence
{
    public interface IDocumentSequenceCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.DocumentSequence entity);
        Task<int> UpdateAsync(Domain.Entities.DocumentSequence entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
