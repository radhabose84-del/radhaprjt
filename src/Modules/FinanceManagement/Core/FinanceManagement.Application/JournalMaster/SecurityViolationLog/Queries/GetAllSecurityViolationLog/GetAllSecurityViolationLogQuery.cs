using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.SecurityViolationLog.Queries.GetAllSecurityViolationLog
{
    public class GetAllSecurityViolationLogQuery : IRequest<ApiResponseDTO<List<SecurityViolationLogDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int? JournalHeaderId { get; set; }
    }
}
