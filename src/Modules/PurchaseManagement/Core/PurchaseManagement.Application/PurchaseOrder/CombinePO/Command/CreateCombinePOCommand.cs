using PurchaseManagement.Application.Common.HttpResponse;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Command;

public sealed record CreateCombinePOCommand(CreateCombinePODto Data)
    : IRequest<ApiResponseDTO<int>>;
