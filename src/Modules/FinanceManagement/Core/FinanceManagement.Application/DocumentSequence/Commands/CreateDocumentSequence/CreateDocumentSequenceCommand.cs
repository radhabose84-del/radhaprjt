using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.DocumentSequence.Commands.CreateDocumentSequence
{
    public class CreateDocumentSequenceCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int TransactionTypeId { get; set; }
        public int FinancialYearId { get; set; }
        public int DocNo { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
