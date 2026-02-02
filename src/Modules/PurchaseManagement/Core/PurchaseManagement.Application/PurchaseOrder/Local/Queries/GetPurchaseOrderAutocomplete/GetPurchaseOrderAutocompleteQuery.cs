// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using MediatR;
using PurchaseLocalDetailDto.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderAutocomplete;


public record GetPurchaseOrderAutocompleteQuery(string? Term, int? poMethodId ,int? budgetGroupId)
    : IRequest<IEnumerable<AutocompleteDto>>;