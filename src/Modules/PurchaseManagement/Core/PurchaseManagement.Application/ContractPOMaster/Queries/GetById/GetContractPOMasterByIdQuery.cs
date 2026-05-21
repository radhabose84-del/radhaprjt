using MediatR;
using PurchaseManagement.Application.ContractPOMaster.Dto;

namespace PurchaseManagement.Application.ContractPOMaster.Queries.GetById;

public sealed record GetContractPOMasterByIdQuery(int Id) : IRequest<ContractPOHeaderDto?>;
