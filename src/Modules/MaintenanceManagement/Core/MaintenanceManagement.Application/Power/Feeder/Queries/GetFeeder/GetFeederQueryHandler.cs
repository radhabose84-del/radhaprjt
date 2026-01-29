using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
// using Contracts.Interfaces.External.IUser;
using MaintenanceManagement.Application.Common.HttpResponse;
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
        // private readonly IDepartmentAllGrpcClient _departmentAllGrpcClient;
        // private readonly IUnitGrpcClient _unitGrpcClient;


        public GetFeederQueryHandler(IFeederQueryRepository feederQueryRepository, IMapper mapper, IMediator mediator
        // , IDepartmentAllGrpcClient departmentAllGrpcClient, IUnitGrpcClient unitGrpcClient
        )
        {
            _feederQueryRepository = feederQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            // _departmentAllGrpcClient = departmentAllGrpcClient;
            // _unitGrpcClient = unitGrpcClient;
        }

        public async Task<ApiResponseDTO<List<GetFeederDto>>> Handle(GetFeederQuery request, CancellationToken cancellationToken)
        {
            var (Feeder, totalCount) = await _feederQueryRepository.GetAllFeederAsync(request.PageNumber, request.PageSize, request.SearchTerm);            
            var Feederlist = _mapper.Map<List<GetFeederDto>>(Feeder);  

            //  var departments = await _departmentAllGrpcClient.GetDepartmentAllAsync();
            // var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            // 🔥 Map department & unit names with DataControl to costCenters
            // foreach (var dto in Feederlist)
            // {
            //     if (departmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
            //         dto.DepartmentName = deptName;

            // }
            
            //   var units = await _unitGrpcClient.GetAllUnitAsync();
            // var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);  
            var powerConsumptionDictionary = new Dictionary<int, GetFeederDto>();

            // 🔥 Map unit names with DataControl to costCenters
            foreach (var data in Feederlist)
            {
                // if (unitLookup.TryGetValue(data.UnitId, out var unitName) && unitName != null)
                // {
                //     data.UnitName = unitName;
                // }

                powerConsumptionDictionary[data.UnitId] = data;

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