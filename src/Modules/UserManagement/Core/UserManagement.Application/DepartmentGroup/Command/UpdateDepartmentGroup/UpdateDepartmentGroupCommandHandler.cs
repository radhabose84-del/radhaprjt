using AutoMapper;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.DepartmentGroup.Command.UpdateDepartmentGroup
{
    public class UpdateDepartmentGroupCommandHandler : IRequestHandler<UpdateDepartmentGroupCommand, int>
    {
        private readonly IDepartmentGroupCommandRepository _departmentGroupCommandRepository;
        private readonly IMapper _mapper;

        private readonly IDepartmentGroupQueryRepository _departmentGroupQueryRepository;


        public UpdateDepartmentGroupCommandHandler(IDepartmentGroupCommandRepository departmentGroupCommandRepository, IMapper mapper, IDepartmentGroupQueryRepository departmentGroupQueryRepository)
        {
            _departmentGroupCommandRepository = departmentGroupCommandRepository;
            _mapper = mapper;
            _departmentGroupQueryRepository = departmentGroupQueryRepository;
        }
        
         public async Task<int> Handle(UpdateDepartmentGroupCommand request, CancellationToken cancellationToken)
        {
            var existing = await _departmentGroupQueryRepository.GetDepartmentGroupByIdAsync(request.Id);
            if (existing == null)
            {
                throw new ValidationException("DepartmentGroup not found.");
               
            }

            if (request.IsActive == 0) // Inactive
            {
                var linked = await _departmentGroupQueryRepository.IsLinkedWithDepartmentsAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }
            
            // Update fields
            existing.DepartmentGroupCode = request.DepartmentGroupCode;
            existing.DepartmentGroupName = request.DepartmentGroupName;            
            existing.IsActive = request.IsActive;

            var rows = await _departmentGroupCommandRepository.UpdateAsync(request.Id, existing);


            return rows ? 1 : 0;
        }
    }
}