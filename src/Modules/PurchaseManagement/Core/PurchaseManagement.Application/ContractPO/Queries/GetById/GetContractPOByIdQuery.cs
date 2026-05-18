using MediatR;
using PurchaseManagement.Application.ContractPO.Dto;

namespace PurchaseManagement.Application.ContractPO.Queries.GetById;

public sealed record GetContractPOByIdQuery(int Id) : IRequest<ContractPOHeaderDto?>;
