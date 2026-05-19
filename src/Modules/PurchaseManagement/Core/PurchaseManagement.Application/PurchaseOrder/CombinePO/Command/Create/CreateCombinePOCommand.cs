using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Create.Command;

public sealed record CreateCombinePOCommand(CreateCombinePODto Data)
    : IRequest<ApiResponseDTO<int>>;
