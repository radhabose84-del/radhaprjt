using MediatR;

namespace SalesManagement.Application.StoTypeMaster.Commands.DeleteStoTypeMaster
{
    public sealed record DeleteStoTypeMasterCommand(int Id) : IRequest<bool>;
}
