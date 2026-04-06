using MediatR;

namespace LogisticsManagement.Application.FreightMaster.Commands.DeleteFreightMaster
{
    public sealed record DeleteFreightMasterCommand(int Id) : IRequest<bool>;
}
