using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster
{
    public class UpdateMiscTypeMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public int IsActive { get; set; }
    }
}
