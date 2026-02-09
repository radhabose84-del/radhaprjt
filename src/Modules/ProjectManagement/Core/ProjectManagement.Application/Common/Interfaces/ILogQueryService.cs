
namespace Core.Application.Common.Interfaces
{
    public interface ILogQueryService
    {
         Task<string?> GetLatestRollbackErrorAsync(Guid correlationId);
         Task<string?> GetLatestConnectionFailureAsync();

    }
}