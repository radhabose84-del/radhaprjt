using MediatR;
using UserManagement.Domain.Entities;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
// using Contracts.Interfaces.External.IMaintenance;
// using Contracts.Interfaces.External.IFixedAssetManagement;
using FluentValidation;

namespace UserManagement.Application.Departments.Commands.DeleteDepartment
{

    public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, int>
    {

        private readonly IDepartmentCommandRepository _IdepartmentCommandRepository;
        private readonly IMapper _Imapper;
        private readonly IDepartmentQueryRepository _IdepartmentQueryRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteDepartmentCommandHandler> _logger;
        // private readonly IDepartmentValidationGrpcClient _departmentValidationGrpcClient;
        // private readonly IFixedAssetDepartmentValidationGrpcClient _fixedAssetDepartmentValidationGrpcClient;

        public DeleteDepartmentCommandHandler(IDepartmentCommandRepository departmentRepository, IDepartmentQueryRepository departmentQueryRepository, IMediator mediator, IMapper mapper, ILogger<DeleteDepartmentCommandHandler> logger
        // , IDepartmentValidationGrpcClient departmentValidationGrpcClient, IFixedAssetDepartmentValidationGrpcClient fixedAssetDepartmentValidationGrpcClient
        )
        {

            _IdepartmentCommandRepository = departmentRepository;
            _Imapper = mapper;
            _IdepartmentQueryRepository = departmentQueryRepository;
            _mediator = mediator;
            _logger = logger;
            // _departmentValidationGrpcClient = departmentValidationGrpcClient;
            // _fixedAssetDepartmentValidationGrpcClient = fixedAssetDepartmentValidationGrpcClient;
        }


        public async Task<int> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
        {

            _logger.LogInformation("DeleteDepartmentCommandHandler started for Department ID: {DepartmentId}", request.Id);
            // ✅Call MaintenanceService via gRPC to check usage
        //    // bool isUsed = await _departmentValidationGrpcClient.CheckIfDepartmentIsUsedAsync(request.Id);
            // bool isUsedInMaintenance = await _departmentValidationGrpcClient.CheckIfDepartmentIsUsedAsync(request.Id);
            // bool isUsedInFixedAsset = await _fixedAssetDepartmentValidationGrpcClient.CheckIfDepartmentIsUsedForFixedAssetAsync(request.Id);



            // if (isUsedInMaintenance || isUsedInFixedAsset)
            // {
            //     _logger.LogWarning("Cannot delete Department ID {DepartmentId} - it is in use by CostCenters.", request.Id);
            //     throw new ValidationException("Cannot delete department. It is still in use in Maintenance or FixedAsset systems.");
               
            // }
            if (await _IdepartmentQueryRepository.IsDepartmentUsedByAnyUserAsync(request.Id))
            {
                throw new ValidationException("Cannot delete Department : this record is referenced by other data.");
            }


            // Map request to entity and delete
            var updatedDepartment = _Imapper.Map<Department>(request);
            var result = await _IdepartmentCommandRepository.DeleteAsync(request.Id, updatedDepartment);

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