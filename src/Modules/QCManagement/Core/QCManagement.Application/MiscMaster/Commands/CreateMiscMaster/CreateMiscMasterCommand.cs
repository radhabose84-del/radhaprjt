using Contracts.Common;
using MediatR;

namespace QCManagement.Application.MiscMaster.Commands.CreateMiscMaster
{
    public class CreateMiscMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
}
