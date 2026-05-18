using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Create;

public sealed record CreateContractReleasePOCommand(ContractReleasePOCreateDto Data)
    : IRequest<ApiResponseDTO<int>>;
