using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Application.Common;

namespace PurchaseManagement.Application.BlanketMaster.Queries.GetAll;

public sealed record GetAllBlanketMasterQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null
) : IRequest<PagedResult<BlanketHeaderDto>>;
