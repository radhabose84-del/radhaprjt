using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetAllReturnReasons;

public sealed record GetAllReturnReasonsQuery(int PageNumber, int PageSize, string? SearchTerm)
    : IRequest<PagedResult<ReturnReasonDto>>;
