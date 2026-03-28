using MediatR;
using ProductionManagement.Application.YarnTwistMaster.Dto;

namespace ProductionManagement.Application.YarnTwistMaster.Queries.GetYarnTwistMasterById
{
    public class GetYarnTwistMasterByIdQuery : IRequest<YarnTwistMasterDto>
    {
        public int Id { get; set; }
    }
}
