#nullable disable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Application.Common.Interfaces.IStcokLedger;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Reports.GetCurrentAllStockItems
{
    public class GetCurrentAllStockItemsQueryHandler : IRequestHandler<GetCurrentAllStockItemsQuery,ApiResponseDTO<List<CurrentStockDto>>>
    {
        private readonly IReportRepository _stockLedgerQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;

        public GetCurrentAllStockItemsQueryHandler(IReportRepository stockLedgerQueryRepository, IMapper mapper, IMediator mediator, IDepartmentLookup departmentLookup)
        {
            _stockLedgerQueryRepository = stockLedgerQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
        }

        public async Task<ApiResponseDTO<List<CurrentStockDto>>> Handle(GetCurrentAllStockItemsQuery request, CancellationToken cancellationToken)
        {
            var result = await _stockLedgerQueryRepository.GetStockDetails(request.OldUnitcode,request.DepartmentId);
            var substores = _mapper.Map<List<CurrentStockDto>>(result);
              // 🔥 Fetch departments using lookup
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            foreach (var data in substores)
            {
                if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
                {
                    data.DepartmentName = departmentName;
                }
            }

             //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAllStock",
                    actionCode: "GetCurrentAllStockItemsQuery",        
                    actionName: substores.Count.ToString(),
                    details: $"Stock details was fetched.",
                    module:"SubStoresStock"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<CurrentStockDto>> { IsSuccess = true, Message = "Success", Data = substores };
        }
    }
}
