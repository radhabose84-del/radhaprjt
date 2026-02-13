using MediatR;
using UserManagement.Domain.Entities;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using FluentValidation;
using MaintenanceDeptValidation = Contracts.Interfaces.Lookups.Maintenance.IDepartmentValidationLookup;
using FixedAssetDeptValidation = Contracts.Interfaces.Lookups.FixedAssetManagement.IDepartmentValidationLookup;

namespace UserManagement.Application.Departments.Commands.DeleteDepartment
{
    public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, int>
    {
        private readonly IDepartmentCommandRepository _departmentCommandRepository;
        private readonly IMapper _mapper;
        private readonly IDepartmentQueryRepository _departmentQueryRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteDepartmentCommandHandler> _logger;
        private readonly MaintenanceDeptValidation _maintenanceDeptValidationLookup;
        private readonly FixedAssetDeptValidation _fixedAssetDeptValidationLookup;

        public DeleteDepartmentCommandHandler(
            IDepartmentCommandRepository departmentRepository,
            IDepartmentQueryRepository departmentQueryRepository,
            IMediator mediator,
            IMapper mapper,
            ILogger<DeleteDepartmentCommandHandler> logger,
            MaintenanceDeptValidation maintenanceDeptValidationLookup,
            FixedAssetDeptValidation fixedAssetDeptValidationLookup)
        {
            _departmentCommandRepository = departmentRepository;
            _mapper = mapper;
            _departmentQueryRepository = departmentQueryRepository;
            _mediator = mediator;
            _logger = logger;
            _maintenanceDeptValidationLookup = maintenanceDeptValidationLookup;
            _fixedAssetDeptValidationLookup = fixedAssetDeptValidationLookup;
        }


        public async Task<int> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("DeleteDepartmentCommandHandler started for Department ID: {DepartmentId}", request.Id);

            // Check if department is used in Maintenance or FixedAsset systems
            var maintenanceTask = _maintenanceDeptValidationLookup.IsDepartmentUsedAsync(request.Id, cancellationToken);
            var fixedAssetTask = _fixedAssetDeptValidationLookup.IsDepartmentUsedAsync(request.Id, cancellationToken);

            await Task.WhenAll(maintenanceTask, fixedAssetTask);

            bool isUsedInMaintenance = maintenanceTask.Result;
            bool isUsedInFixedAsset = fixedAssetTask.Result;

            if (isUsedInMaintenance || isUsedInFixedAsset)
            {
                _logger.LogWarning("Cannot delete Department ID {DepartmentId} - it is in use.", request.Id);
                throw new ValidationException("Cannot delete department. It is still in use in Maintenance or FixedAsset systems.");
            }

            if (await _departmentQueryRepository.IsDepartmentUsedByAnyUserAsync(request.Id))
            {
                throw new ValidationException("Cannot delete Department : this record is referenced by other data.");
            }

            // Map request to entity and delete
            var updatedDepartment = _mapper.Map<Department>(request);
            var result = await _departmentCommandRepository.DeleteAsync(request.Id, updatedDepartment);

            if (result <= 0)
            {
                _logger.LogWarning("Failed to delete Department with ID {DepartmentId}.", request.Id);
                return result;
            }

            _logger.LogInformation("Department with ID {DepartmentId} deleted successfully.", request.Id);

            // Publish domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: updatedDepartment.Id.ToString(),
                actionName: "",
                details: $"Department ID: {request.Id} was changed to status inactive.",
                module: "Department"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation("AuditLogsDomainEvent published for Department ID {DepartmentId}.", request.Id);

            return result;
        }

    }
}