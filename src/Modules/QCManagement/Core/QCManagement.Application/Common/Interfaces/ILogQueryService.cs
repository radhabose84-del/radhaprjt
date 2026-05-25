namespace QCManagement.Application.Common.Interfaces
{
    public interface ILogQueryService
    {
        Task<string?> GetLatestConnectionFailureAsync();
        Task<string?> GetLatestRollbackErrorAsync(Guid correlationId);
    }
}
