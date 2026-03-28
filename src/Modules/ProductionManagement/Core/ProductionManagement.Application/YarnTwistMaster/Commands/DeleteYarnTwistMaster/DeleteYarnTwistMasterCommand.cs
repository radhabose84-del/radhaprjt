using MediatR;

namespace ProductionManagement.Application.YarnTwistMaster.Commands.DeleteYarnTwistMaster
{
    public sealed record DeleteYarnTwistMasterCommand(int Id) : IRequest<bool>;
}
