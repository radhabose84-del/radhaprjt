using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.UpdateAccountGroup
{
    public class UpdateAccountGroupCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }

        // GroupCode is immutable — re-parenting is done via the Move command, not here.
        public string GroupName { get; set; } = null!;
        public int IsActive { get; set; }
    }
}
