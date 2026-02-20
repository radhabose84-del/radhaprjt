#nullable disable

using MediatR;

namespace SalesManagement.Application.BusinessUnit.Commands.DeleteBusinessUnit
{
    public sealed record DeleteBusinessUnitCommand(int Id) : IRequest<bool>;
}
