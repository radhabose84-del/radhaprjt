using MediatR;
namespace PurchaseManagement.Application.Port.Commands;
public sealed record DeletePortMasterCommand(int Id) : IRequest<bool>;
