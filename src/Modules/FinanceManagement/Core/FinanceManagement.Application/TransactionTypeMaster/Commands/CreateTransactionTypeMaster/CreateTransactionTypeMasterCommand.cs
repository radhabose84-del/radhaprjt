using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Commands.CreateTransactionTypeMaster
{
    public class CreateTransactionTypeMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int ModuleId { get; set; }
        public int MenuId { get; set; }
        public string? TypeName { get; set; }
        public string? ShortName { get; set; }
        public string? Description { get; set; }
    }
}
