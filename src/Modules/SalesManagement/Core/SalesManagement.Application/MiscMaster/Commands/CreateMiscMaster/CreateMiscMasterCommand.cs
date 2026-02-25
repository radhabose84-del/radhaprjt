using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.MiscMaster.Commands.CreateMiscMaster
{
    public class CreateMiscMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int MiscTypeId { get; set; }
        public string Code { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
