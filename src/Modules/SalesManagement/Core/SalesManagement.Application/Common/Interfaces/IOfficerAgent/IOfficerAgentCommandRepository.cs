namespace SalesManagement.Application.Common.Interfaces.IOfficerAgent
{
    public interface IOfficerAgentCommandRepository
    {
        Task<int> CreateBatchAsync(List<Domain.Entities.OfficerAgent> entities);
        Task<int> UpdateBatchAsync(List<Domain.Entities.OfficerAgent> entities);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }
}
