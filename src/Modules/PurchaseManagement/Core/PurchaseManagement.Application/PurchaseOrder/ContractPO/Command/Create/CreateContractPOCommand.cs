using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Create;

public sealed record CreateContractPOCommand(ContractPOCreateDto Data)
    : IRequest<ApiResponseDTO<int>>;
