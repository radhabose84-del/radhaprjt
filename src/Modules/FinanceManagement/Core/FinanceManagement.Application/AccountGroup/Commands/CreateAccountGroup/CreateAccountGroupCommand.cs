using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.CreateAccountGroup
{
    public class CreateAccountGroupCommand : IRequest<ApiResponseDTO<int>>
    {
        public int CompanyId { get; set; }
        public string GroupCode { get; set; } = null!;
        public string GroupName { get; set; } = null!;

        // NULL = create a Level 1 (root) group.
        public int? ParentAccountGroupId { get; set; }

        public int SortOrder { get; set; }
    }
}
