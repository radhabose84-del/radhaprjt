using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.ISecurityViolationLog
{
    public interface ISecurityViolationLogQueryRepository
    {
        Task<(List<SecurityViolationLogDto>, int)> GetAllAsync(int pageNumber, int pageSize, int? journalHeaderId);
    }
}
