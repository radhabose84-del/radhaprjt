using AutoMapper;
using Contracts.Common;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;

namespace WarehouseManagement.Application.WarehouseMaster.GetWarehouseMasterById
{
    public class GetWarehouseMasterByIdQueryHandler : IRequestHandler<GetWarehouseMasterByIdQuery, ApiResponseDTO<WarehouseMasterDto>>
    {

        private readonly IWarehouseMasterQueryRepository _warehouseMasterQueryRepository;
        private readonly IUOMLookup _uomLookup;
        private readonly IMiscMasterLookup _miscMasterLookup;
        private readonly ICityLookup _cityLookup;
        private readonly ICountryLookup _countryLookup;
        private readonly IStateLookup _stateLookup;   
        private readonly IMapper _mapper;
        private readonly IDepartmentLookup _departmentLookup;

        public GetWarehouseMasterByIdQueryHandler(IWarehouseMasterQueryRepository warehouseMasterQueryRepository, IMiscMasterLookup miscMasterLookup,
            IMapper mapper, IUOMLookup uomLookup, ICityLookup cityLookup,
            ICountryLookup countryLookup, IStateLookup stateLookup, IDepartmentLookup departmentLookup)
        {
            _warehouseMasterQueryRepository = warehouseMasterQueryRepository;
            _miscMasterLookup = miscMasterLookup;
            _mapper = mapper;
            _uomLookup = uomLookup;
            _cityLookup = cityLookup;
            _countryLookup = countryLookup;
            _stateLookup = stateLookup;
            _departmentLookup = departmentLookup;
        }
        
        public async Task<ApiResponseDTO<WarehouseMasterDto>> Handle(GetWarehouseMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _warehouseMasterQueryRepository.GetByIdAsync(request.Id);
            if (entity == null)
            {
                return new ApiResponseDTO<WarehouseMasterDto>
                {
                    IsSuccess = false,
                    Message = "Warehouse Master not found"
                };
            }

            var dto = _mapper.Map<WarehouseMasterDto>(entity);
            // Fetch misc lookups sequentially to avoid sharing the same DB connection concurrently
            var warehouseTypes = await _miscMasterLookup.GetMiscMasterByIdAsync("WarehouseType") ?? new();
            var storageTypes   = await _miscMasterLookup.GetMiscMasterByIdAsync("StorageType") ?? new();
            var areaTypes      = await _miscMasterLookup.GetMiscMasterByIdAsync("AreaType") ?? new();
            var operationTypes = await _miscMasterLookup.GetMiscMasterByIdAsync("OperationType") ?? new();

            // Other lookups can stay parallel (each uses its own repo/connection)
            var uomTask       = _uomLookup.GetAllAsync();
            var cityTask      = _cityLookup.GetAllCityAsync();
            var stateTask     = _stateLookup.GetAllStatesAsync();
            var countryTask   = _countryLookup.GetAllCountriesAsync();
            var departmentTask= _departmentLookup.GetAllDepartmentAsync();

            await Task.WhenAll(uomTask, cityTask, stateTask, countryTask, departmentTask);

            // build dicts
            var whDict      = warehouseTypes.ToDictionary(x => x.Id,      x => x.Description);
            var stDict      = storageTypes.ToDictionary(x => x.Id,       x => x.Description);
            var areaDict    = areaTypes.ToDictionary(x => x.Id,         x => x.Description);
            var opDict      = operationTypes.ToDictionary(x => x.Id,    x => x.Description);

            var uomDict     = uomTask.Result.ToDictionary(x => x.Id,     x => x.UOMName);
            var cityDict    = cityTask.Result.ToDictionary(x => x.CityId,    x => x.CityName);
            var stateDict   = stateTask.Result.ToDictionary(x => x.StateId,   x => x.StateName);
            var countryDict = countryTask.Result.ToDictionary(x => x.CountryId,x => x.CountryName);
            var departmentDict = departmentTask.Result.ToDictionary(x => x.DepartmentId,x => x.DepartmentName);

            // fill names
            if (whDict.TryGetValue(dto.WarehouseTypeId, out var whName) && whName != null)
                dto.WarehouseTypeName = whName;

            if (stDict.TryGetValue(dto.StorageTypeId, out var stName) && stName != null)
                dto.StorageTypeName = stName;

            if (areaDict.TryGetValue(dto.AreaTypeId, out var areaName) && areaName != null)
                dto.AreaTypeName = areaName;

            if (opDict.TryGetValue(dto.OperationTypeId, out var opName) && opName != null)
                dto.OperationTypeName = opName;

            if (uomDict.TryGetValue(dto.CapacityUOMId, out var uomName) && uomName != null)
                dto.CapacityUOMName = uomName;

            if (cityDict.TryGetValue(dto.CityId, out var cityName) && cityName != null)
                dto.City = cityName;

            if (stateDict.TryGetValue(dto.StateId, out var stateName) && stateName != null)
                dto.State = stateName;

            if (countryDict.TryGetValue(dto.CountryId, out var countryName) && countryName != null)
                dto.Country = countryName;

            if (departmentDict.TryGetValue(dto.DepartmentId, out var departmentName) && departmentName != null)
                dto.DepartmentName = departmentName;    

       

            return new ApiResponseDTO<WarehouseMasterDto>
            {
                IsSuccess = true,
                Message = "Fetched successfully",
                Data = dto
            };
        }

    }
}
