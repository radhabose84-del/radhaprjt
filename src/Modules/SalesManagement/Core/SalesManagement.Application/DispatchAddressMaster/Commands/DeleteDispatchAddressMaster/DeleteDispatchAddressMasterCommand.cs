using MediatR;

namespace SalesManagement.Application.DispatchAddressMaster.Commands.DeleteDispatchAddressMaster;

public sealed record DeleteDispatchAddressMasterCommand(int Id) : IRequest<bool>;
