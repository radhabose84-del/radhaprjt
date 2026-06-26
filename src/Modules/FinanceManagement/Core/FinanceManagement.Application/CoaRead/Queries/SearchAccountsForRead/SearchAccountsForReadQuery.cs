using Contracts.Common;
using FinanceManagement.Application.CoaRead.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaRead.Queries.SearchAccountsForRead
{
    // US-GL02-16 (AC5) — downstream search by type/group; returns accounts with status.
    public class SearchAccountsForReadQuery : IRequest<ApiResponseDTO<List<CoaAccountReadDto>>>
    {
        public int? AccountTypeId { get; set; }
        public int? AccountGroupId { get; set; }
        public bool ActiveOnly { get; set; }
    }
}
