using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Power.Feeder.Queries.GetFeeder
{
    public class GetFeederQueryHandler : IRequestHandler<GetFeederQuery, ApiResponseDTO<List<GetFeederDto>>>
    {
        private readonly IFeederQueryRepository _feederQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUnitLookup _unitLookup;

        public GetFeederQueryHandler(
            IFeederQueryRepository feederQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUnitLookup unitLookup)
        {
            _feederQueryRepository = feederQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _unitLookup = unitLookup;
        }

        public async Task<ApiResponseDTO<List<GetFeederDto>>> Handle(GetFeederQuery request, CancellationToken cancellationToken)
        {
            var (Feeder, totalCount) = await _feederQueryRepository.GetAllFeederAsync(request.PageNumber, request.PageSize, request.SearchTerm);            
            var Feederlist = _mapper.Map<List<GetFeederDto>>(Feeder);  

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            var units = await _unitLookup.GetAllUnitAsync();
            var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            foreach (var data in Feederlist)
            {
                if (departmentLookup.TryGetValue(data.DepartmentId, out var departmentName) && departmentName != null)
                {
                    data.DepartmentName = departmentName;
                }

                if (unitLookup.TryGetValue(data.UnitId, out var unitName) && unitName != null)
                {
                    data.UnitName = unitName;
                }
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetFeederGroupQuery",
                actionCode: "Get",        
                actionName: Feeder.Count().ToString(),
                details: $"FeederGroup details was fetched",
                module:"FeederGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<GetFeederDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = Feederlist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            
        }
    }
}
