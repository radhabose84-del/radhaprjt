using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetById;

public sealed record GetContractReleasePOByIdQuery(int Id) : IRequest<ContractReleasePODetailVm?>;
