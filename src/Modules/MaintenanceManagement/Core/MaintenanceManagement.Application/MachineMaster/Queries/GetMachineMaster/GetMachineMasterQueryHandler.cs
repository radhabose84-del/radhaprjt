using AutoMapper;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster
{
    public class GetMachineMasterQueryHandler : IRequestHandler<GetMachineMasterQuery, ApiResponseDTO<List<MachineMasterDto>>>
    {
        private readonly IMachineMasterQueryRepository _imachineMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IAssetSpecificationLookup _assetSpecificationLookup;

        public GetMachineMasterQueryHandler(IMachineMasterQueryRepository imachineMasterQueryRepository, IMapper mapper, IMediator mediator, IDepartmentLookup departmentLookup, IAssetSpecificationLookup assetSpecificationLookup)
        {
            _imachineMasterQueryRepository = imachineMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _assetSpecificationLookup = assetSpecificationLookup;
        }

        public async Task<ApiResponseDTO<List<MachineMasterDto>>> Handle(GetMachineMasterQuery request, CancellationToken cancellationToken)
        {
            var MachineMaster = await _imachineMasterQueryRepository.GetAllMachineAsync(request.SearchTerm);
            var machineMastersgroup = _mapper.Map<List<MachineMasterDto>>(MachineMaster);
            // Fetch departments using lookup
            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var departmentLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            foreach (var dto in machineMastersgroup)
            {
                if (departmentLookup.TryGetValue(dto.DepartmentId, out var deptName))
                    dto.ProductionDepartmentName = deptName;
            }

            // Fetch all specifications using lookup
            var assetSpecifications = await _assetSpecificationLookup.GetAllAssetSpecificationAsync();

            // 🔹 Build dictionary: AssetId → Only "Make" SpecificationValue
            var assetSpecLookup = assetSpecifications
                .Where(s => s.SpecificationName == "Make")
                .GroupBy(d => d.AssetId)
                .ToDictionary(
                    g => g.Key,
                    g => g.FirstOrDefault()?.SpecificationValue ?? string.Empty
                );

            // 🔹 Map specification(s) into Machine DTO
            foreach (var machine in machineMastersgroup)
            {
                if (assetSpecLookup.TryGetValue(machine.AssetId, out var make))
                {
                    machine.SpecificationName = make;
                }
            }
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetMachineMasterQuery",
                actionCode: "Get",
                actionName: MachineMaster.Count.ToString(),
                details: "MachineMaster details were fetched.",
                module: "MachineMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<MachineMasterDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = machineMastersgroup,
                TotalCount = MachineMaster.Count
            };
        }
    }
}