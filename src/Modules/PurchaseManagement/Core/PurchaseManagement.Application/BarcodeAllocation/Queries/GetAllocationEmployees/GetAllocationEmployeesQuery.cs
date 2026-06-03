using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetAllocationEmployees
{
    public sealed record GetAllocationEmployeesQuery(string? Term) : IRequest<IReadOnlyList<BarcodeAllocationEmployeeDto>>;
}
