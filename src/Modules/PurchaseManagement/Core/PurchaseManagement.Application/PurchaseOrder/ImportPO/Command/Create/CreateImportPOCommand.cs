using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using MediatR;
namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
public sealed record CreateImportPOCommand : IRequest<int>
{
    public ImportPOCreateDto Data { get; init; }= default!;
}
