using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Queries.GetPOById;
public record GetImportPOByIdQuery(int Id) : IRequest<ImportPOFullVm?>;