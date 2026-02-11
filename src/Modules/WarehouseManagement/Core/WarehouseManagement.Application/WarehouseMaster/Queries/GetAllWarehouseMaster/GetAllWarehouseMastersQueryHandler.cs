using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseManagement.Application.Common.HttpResponse;
using AutoMapper;
using WarehouseManagement.Application.Common.HttpResponse;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using Contracts.Interfaces.External.IInvetoryManagement;
using Contracts.Interfaces.External.IUser;
using DnsClient.Protocol;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;

namespace WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster
{
    public class GetAllWarehouseMastersQueryHandler : IRequestHandler<GetAllWarehouseMastersQuery, ApiResponseDTO<List<WarehouseMasterDto>>>
    {

        private readonly IWarehouseMasterQueryRepository _iWarehouseMasterQueryRepository;
        private readonly IUOMLookup _uomLookup;
        private readonly IMiscMasterLookup _miscMasterLookup;
        private readonly ICityLookup _cityLookup;
        private readonly ICountryLookup _countryLookup;
        private readonly IStateLookup _stateLookup;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        public GetAllWarehouseMastersQueryHandler(IWarehouseMasterQueryRepository warehouseMasterQueryRepository, IMapper mapper, IMediator mediator, IMiscMasterLookup miscMasterLookup, IUOMLookup uomLookup, ICityLookup cityLookup,
            ICountryLookup countryLookup, IStateLookup stateLookup, IDepartmentLookup departmentLookup)
        {
            _iWarehouseMasterQueryRepository = warehouseMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _miscMasterLookup = miscMasterLookup;
            _uomLookup = uomLookup;
            _cityLookup = cityLookup;
            _countryLookup = countryLookup;
            _stateLookup = stateLookup;
            _departmentLookup = departmentLookup;
        }
         public async Task<ApiResponseDTO<List<WarehouseMasterDto>>> Handle(GetAllWarehouseMastersQuery request, CancellationToken cancellationToken)
        {

          
             // var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            // Get paginated + searched data from DB
            var (warehouseEntities, totalCount) = await _iWarehouseMasterQueryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm
            );
            var warehouseListdto = _mapper.Map<List<WarehouseMasterDto>>(warehouseEntities);
            var uomTask    = _uomLookup.GetAllAsync();                 // add ct if your client supports it
            var cityTask   = _cityLookup.GetAllCityAsync();
            var stateTask  = _stateLookup.GetAllStatesAsync();
            var countryTask= _countryLookup.GetAllCountriesAsync();
            var departmentTask = _departmentLookup.GetAllDepartmentAsync(); 

            var warehouseTypes = await _miscMasterLookup.GetMiscMasterByIdAsync("WarehouseType") ?? new();
            var storageTypes = await _miscMasterLookup.GetMiscMasterByIdAsync("StorageType") ?? new();
            var areaTypes = await _miscMasterLookup.GetMiscMasterByIdAsync("AreaType") ?? new();
            var operationTypes = await _miscMasterLookup.GetMiscMasterByIdAsync("OperationType") ?? new();

            await Task.WhenAll(uomTask, cityTask, stateTask, countryTask, departmentTask);

            // build dicts
            var whDict   = warehouseTypes.ToDictionary(x => x.Id, x => x.Description);
            var stDict   = storageTypes.ToDictionary(x => x.Id, x => x.Description);
            var areaDict = areaTypes.ToDictionary(x => x.Id, x => x.Description);
            var opDict   = operationTypes.ToDictionary(x => x.Id, x => x.Description);

            var uomDict    = uomTask.Result.ToDictionary(x => x.Id,     x => x.UOMName);
            var cityDict   = cityTask.Result.ToDictionary(x => x.CityId,   x => x.CityName);
            var stateDict  = stateTask.Result.ToDictionary(x => x.StateId,  x => x.StateName);
            var countryDict= countryTask.Result.ToDictionary(x => x.CountryId,x => x.CountryName);
            var departmentDict = departmentTask.Result.ToDictionary(x => x.DepartmentId, x => x.DepartmentName);

            // enrich DTOs
            foreach (var d in warehouseListdto)
            {
                if (whDict.TryGetValue(d.WarehouseTypeId, out var whName) && whName != null)
                    d.WarehouseTypeName = whName;

                if (stDict.TryGetValue(d.StorageTypeId, out var stName) && stName != null)
                    d.StorageTypeName = stName;

                if (areaDict.TryGetValue(d.AreaTypeId, out var areaName) && areaName != null)
                    d.AreaTypeName = areaName;

                if (opDict.TryGetValue(d.OperationTypeId, out var opName) && opName != null)
                    d.OperationTypeName = opName;

                if (uomDict.TryGetValue(d.CapacityUOMId, out var uomName) && uomName != null)
                    d.CapacityUOMName = uomName;

                if (cityDict.TryGetValue(d.CityId, out var cityName) && cityName != null)
                    d.City = cityName;

                if (stateDict.TryGetValue(d.StateId, out var stateName) && stateName != null)
                    d.State = stateName;

                if (countryDict.TryGetValue(d.CountryId, out var countryName) && countryName != null)
                    d.Country = countryName;

                if (departmentDict.TryGetValue(d.DepartmentId, out var departmentName) && departmentName != null)
                    d.DepartmentName = departmentName;    
            }
           

                // Domain Event for auditing
                var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "Warehouse Master data fetched",
                module: "WarehouseMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            // Wrap response
            return new ApiResponseDTO<List<WarehouseMasterDto>>
            {
                IsSuccess = true,
                Message = "Fetched successfully",
                Data = warehouseListdto,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        
    }
}
