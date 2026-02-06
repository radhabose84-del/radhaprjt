using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Departments.Commands.UpdateDepartment;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Mappings
{
    public class UpdateDepartmentProfile :Profile
    {
        
        public UpdateDepartmentProfile()
        {
            CreateMap<UpdateDepartmentCommand, Department>();
            
        }
    }
}