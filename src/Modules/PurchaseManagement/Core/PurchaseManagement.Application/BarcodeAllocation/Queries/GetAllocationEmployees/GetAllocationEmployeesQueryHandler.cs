using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetAllocationEmployees
{
    public class GetAllocationEmployeesQueryHandler : IRequestHandler<GetAllocationEmployeesQuery, IReadOnlyList<BarcodeAllocationEmployeeDto>>
    {
        private readonly IBarcodeAllocationQueryRepository _queryRepository;

        public GetAllocationEmployeesQueryHandler(IBarcodeAllocationQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public async Task<IReadOnlyList<BarcodeAllocationEmployeeDto>> Handle(GetAllocationEmployeesQuery request, CancellationToken cancellationToken)
        {
            return await _queryRepository.GetEmployeesAsync(request.Term, cancellationToken);
        }
    }
}
