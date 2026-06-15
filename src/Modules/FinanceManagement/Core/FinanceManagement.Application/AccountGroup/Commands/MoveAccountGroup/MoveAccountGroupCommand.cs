using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.MoveAccountGroup
{
    // Hierarchy restructuring changes statutory financial-statement presentation, so it
    // carries a documented justification and a Finance Controller approver.
    public class MoveAccountGroupCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int NewParentAccountGroupId { get; set; }
        public string Justification { get; set; } = null!;
        public int ApproverId { get; set; }
    }
}
