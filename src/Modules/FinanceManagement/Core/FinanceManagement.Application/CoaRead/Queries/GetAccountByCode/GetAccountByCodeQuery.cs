using Contracts.Common;
using FinanceManagement.Application.CoaRead.Dto;
using MediatR;

namespace FinanceManagement.Application.CoaRead.Queries.GetAccountByCode
{
    // US-GL02-16 (AC1) — downstream get-by-code.
    public class GetAccountByCodeQuery : IRequest<ApiResponseDTO<CoaAccountReadDto?>>
    {
        public GetAccountByCodeQuery(string accountCode) => AccountCode = accountCode;
        public string AccountCode { get; }
    }
}
