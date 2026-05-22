using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.ContractPOMaster.Dto;

namespace PurchaseManagement.Application.ContractPOMaster.Queries.GetAll;

public sealed record GetAllContractPOMasterQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null
) : IRequest<PagedResult<ContractPOHeaderDto>>;
    