using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.YarnTwistMaster.Commands.UpdateYarnTwistMaster
{
    public class UpdateYarnTwistMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? TwistName { get; set; }
        public string? Description { get; set; }
        public int IsActive { get; set; }
    }
}
