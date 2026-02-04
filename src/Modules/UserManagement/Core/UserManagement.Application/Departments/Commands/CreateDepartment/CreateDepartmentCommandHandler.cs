using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Application.Departments.Commands.CreateDepartment;
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace UserManagement.Application.Departments.Commands.CreateDepartment
{

    public class CreateDepartmentCommandHandler :IRequestHandler<CreateDepartmentCommand, DepartmentDto>
    {
        private readonly IDepartmentCommandRepository _departmentRepository;
        private readonly IMapper _mapper;
          private readonly IMediator _mediator; 
          private readonly ILogger<CreateDepartmentCommandHandler> _logger;
           
    public CreateDepartmentCommandHandler(IDepartmentCommandRepository departmentRepository,IMapper mapper, IMediator mediator ,ILogger<CreateDepartmentCommandHandler> logger)
        {
             _departmentRepository=departmentRepository;
            _mapper=mapper;
            _mediator=mediator;
            _logger=logger;

        }     

       public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
                _logger.LogInformation("Starting CreateDepartmentCommandHandler for request: {@Request}", request);
      
           // ✅ Check if DeptName already exists
                    var exists = await _departmentRepository.ExistsByCodeAsync(request.DeptName);
                    if (exists)
                    {
                        _logger.LogWarning("Department Name {DeptName} already exists.", request.DeptName);
                        throw new ValidationException("Department Name already exists.");
                       
                    }
            // Map the request to the entity
            var departmentEntity = _mapper.Map<Department>(request);
            _logger.LogInformation("Mapped CreateDepartmentCommand to Department entity: {@DepartmentEntity}", departmentEntity);

            // Save the department
            var createdDepartment = await _departmentRepository.CreateAsync(departmentEntity);

            if (createdDepartment is null)
            {
                _logger.LogWarning("Failed to create department. Department entity: {@DepartmentEntity}", departmentEntity);
                throw new ValidationException("Department not created");
            
            }

            _logger.LogInformation("Department successfully created with ID: {DepartmentId}", createdDepartment.Id);

            // Publish the domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: createdDepartment.Id.ToString(),
                actionName: createdDepartment.DeptName,
                details: $"Department '{createdDepartment.DeptName}' was created. DepartmentID: {createdDepartment.Id}",
                module: "Department"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation("AuditLogsDomainEvent published for Department ID: {DepartmentId}", createdDepartment.Id);

            // Map the result to DTO
            var deptDto = _mapper.Map<DepartmentDto>(createdDepartment);

            _logger.LogInformation("Returning success response for Department ID: {DepartmentId}", createdDepartment.Id);

            return deptDto;
           
        }
    }  
}