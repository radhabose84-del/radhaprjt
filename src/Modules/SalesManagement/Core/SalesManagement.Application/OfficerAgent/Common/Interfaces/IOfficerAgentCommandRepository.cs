namespace SalesManagement.Application.Common.Interfaces.IOfficerAgent
{
    public interface IOfficerAgentCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.OfficerAgent entity);
        Task<int> UpdateAsync(Domain.Entities.OfficerAgent entity);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
