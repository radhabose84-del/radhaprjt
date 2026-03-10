using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DocumentSequence.Commands.CreateDocumentSequence
{
    public class CreateDocumentSequenceCommand : IRequest<ApiResponseDTO<int>>
    {
        public int TransactionTypeId { get; set; }
        public int FinancialYearId { get; set; }
        public int DocNo { get; set; }
    }
}
