using MediatR;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetReturnTypeAutoComplete;

public sealed record GetReturnTypeAutoCompleteQuery(string? Term)
    : IRequest<IReadOnlyList<ReturnTypeLookupDto>>;
