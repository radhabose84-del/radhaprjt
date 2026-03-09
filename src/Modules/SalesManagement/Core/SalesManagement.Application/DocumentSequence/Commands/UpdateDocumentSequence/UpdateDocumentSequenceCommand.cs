using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DocumentSequence.Commands.UpdateDocumentSequence
{
    public class UpdateDocumentSequenceCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int FinancialYearId { get; set; }
        public int DocNo { get; set; }
        public int IsActive { get; set; }
    }
}
