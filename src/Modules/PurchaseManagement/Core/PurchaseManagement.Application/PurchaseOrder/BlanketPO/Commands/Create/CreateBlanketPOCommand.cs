using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Create;

public sealed record CreateBlanketPOCommand(BlanketPOCreateDto Data)
    : IRequest<ApiResponseDTO<int>>;
