#nullable disable
using UserManagement.Domain.Entities;
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Domain.Events;
using FluentValidation;


namespace UserManagement.Application.Departments.Commands.UpdateDepartment
{

    public class UpdateDepartmentCommandHandler  : IRequestHandler<UpdateDepartmentCommand ,bool>
    {

        public readonly IDepartmentCommandRepository _IDepartmentCommandRepository;
       private readonly IMapper _Imapper;
        private readonly ILogger<UpdateDepartmentCommandHandler> _logger;
        private readonly IDepartmentQueryRepository _departmentQueryRepository;
        private readonly IMediator _mediator; 
         

        public UpdateDepartmentCommandHandler(IDepartmentCommandRepository iDepartmentcommandRepository,IDepartmentQueryRepository idepartmentQueryRepository, IMapper Imapper, ILogger<UpdateDepartmentCommandHandler> logger ,IMediator mediator  )
        {

            _IDepartmentCommandRepository = iDepartmentcommandRepository;
            _departmentQueryRepository = idepartmentQueryRepository;
            _Imapper = Imapper;
            _logger = logger;
             _mediator = mediator;
        }
            

    

       public async Task<bool> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
       {            
            _logger.LogInformation("Starting UpdateDepartmentCommandHandler for request: {@Request}", request);
      

            _logger.LogInformation("Department with ID {DepartmentId} retrieved successfully.", request.Id);

            var departmentMap  = _Imapper.Map<Department>(request);
            // Save updates to the repository
            var result = await _IDepartmentCommandRepository.UpdateAsync(request.Id, departmentMap);
           // var resultCode = result ?? 0;
            if(result <= 0)
            {
                _logger.LogWarning("Failed to update Department with ID {DepartmentId}.", request.Id);
                throw new ValidationException("Failed to update department");
                
            }
            if (request.IsActive == 0)
            {
                var linked = await _departmentQueryRepository.IsDepartmentLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }
            
              var duplicateCheck = await _IDepartmentCommandRepository.ExistsByNameupdateAsync(request.DeptName, request.Id);
                if (duplicateCheck)
                {
                    throw new ValidationException("Department Name  Already Exists");
                   
                }

            _logger.LogInformation("Department with ID {DepartmentId} updated successfully.", request.Id);

            // Map the updated entity to DTO
            var dept = await _departmentQueryRepository.GetByIdAsync(request.Id);
            
           
            // Publish domain event for audit logs
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: departmentMap.Id.ToString(),
                actionName: departmentMap.DeptName,
                details: $"Department '{departmentMap.DeptName}' was updated. Department ID: {request.Id}",
                module: "Department"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation("AuditLogsDomainEvent published for Department ID {DepartmentId}.", departmentMap.Id);

            return result > 0;


       }


    }
}