using Contracts.Common;
using MediatR;

namespace QCManagement.Application.MiscMaster.Commands.UpdateMiscMaster
{
    public class UpdateMiscMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int IsActive { get; set; }
    }
}
