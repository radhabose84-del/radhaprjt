using MediatR;
using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetAllReturnTypes;

public sealed record GetAllReturnTypesQuery(int PageNumber, int PageSize, string? SearchTerm)
    : IRequest<PagedResult<ReturnTypeDto>>;
