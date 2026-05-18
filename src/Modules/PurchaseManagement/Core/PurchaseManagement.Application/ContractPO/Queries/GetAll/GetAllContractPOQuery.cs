using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.ContractPO.Dto;

namespace PurchaseManagement.Application.ContractPO.Queries.GetAll;

public sealed record GetAllContractPOQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null
) : IRequest<PagedResult<ContractPOHeaderDto>>;
