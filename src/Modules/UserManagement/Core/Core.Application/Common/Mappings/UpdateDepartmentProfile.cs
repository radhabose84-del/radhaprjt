using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Departments.Commands.UpdateDepartment;
using Core.Domain.Entities;

namespace Core.Application.Common.Mappings
{
    public class UpdateDepartmentProfile :Profile
    {
        
        public UpdateDepartmentProfile()
        {
            CreateMap<UpdateDepartmentCommand, Department>();
            
        }
    }
}