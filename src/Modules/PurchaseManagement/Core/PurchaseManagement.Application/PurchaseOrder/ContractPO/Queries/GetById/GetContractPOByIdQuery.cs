using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetById;

public sealed record GetContractPOByIdQuery(int Id) : IRequest<ContractPODetailVm?>;
