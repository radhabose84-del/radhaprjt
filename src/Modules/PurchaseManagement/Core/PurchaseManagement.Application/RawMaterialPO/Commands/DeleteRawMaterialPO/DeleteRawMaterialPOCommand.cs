using MediatR;

namespace PurchaseManagement.Application.RawMaterialPO.Commands.DeleteRawMaterialPO
{
    public sealed record DeleteRawMaterialPOCommand(int Id) : IRequest<bool>;
}
