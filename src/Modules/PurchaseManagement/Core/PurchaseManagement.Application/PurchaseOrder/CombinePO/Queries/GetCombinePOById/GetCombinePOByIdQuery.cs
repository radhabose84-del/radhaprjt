// PurchaseManagement.Application/PurchaseOrder/CombinePO/Queries/GetCombinePOById/GetCombinePOByIdQuery.cs
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Queries.GetCombinePOById;

public sealed record GetCombinePOByIdQuery(int Id, int POMethodId) : IRequest<GetCombinePOByIdVm>;
