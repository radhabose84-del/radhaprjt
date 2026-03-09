using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.TransactionTypeMaster.Commands.UpdateTransactionTypeMaster
{
    public class UpdateTransactionTypeMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public int ModuleId { get; set; }
        public string? TypeName { get; set; }
        public string? ShortName { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
    }
}
