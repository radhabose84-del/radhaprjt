using MediatR;

namespace SalesManagement.Application.FreightMaster.Commands.DeleteFreightMaster
{
    public sealed record DeleteFreightMasterCommand(int Id) : IRequest<bool>;
}
